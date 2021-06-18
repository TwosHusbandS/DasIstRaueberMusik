using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM.Spotify
{
	class SpotifyScraper
	{
		//static string CLIENT_ID = "";
		//static string CLIENT_SECRET = "";

		static SpotifyClientConfig MySpotifyClientConfig;
		static SpotifyClient MySpotifyClient;

		static bool ManualStaticConstructorRunAlready = false;

		public static void Init()
		{
			string what = "";
			// See Spotify\MyAPIConfig_Example.ini
			Uri jsfURI = new Uri(@"Spotify\MyAPIConfig.ini", UriKind.Relative);
			var jsfStream = System.Windows.Application.GetResourceStream(jsfURI);
			using (var reader = new StreamReader(jsfStream.Stream))
			{
				what = reader.ReadToEnd();
			}

			MySpotifyClientConfig = SpotifyClientConfig
	.CreateDefault()
	.WithAuthenticator(new ClientCredentialsAuthenticator(Helper.FileHandling.GetXMLTagContent(what, "CLIENT_ID"), Helper.FileHandling.GetXMLTagContent(what, "CLIENT_SECRET")));

			MySpotifyClient = new SpotifyClient(MySpotifyClientConfig);
		}

		public static async Task<string> GetLinkFromSearch(Helper.Release myRelease)
		{
			if (!ManualStaticConstructorRunAlready)
			{
				Init();
				ManualStaticConstructorRunAlready = true;
			}

			SearchRequest.Types mySearchType = SearchRequest.Types.Track;
			if (myRelease.ReleaseKind == Helper.ReleaseKinds.Album)
			{
				mySearchType = SearchRequest.Types.Album;
			}

			string SearchString = myRelease.Artist + " " + myRelease.Title;

			SearchRequest MySearchRequest = new SearchRequest(mySearchType, SearchString);
			ISearchClient MySearchClient = MySpotifyClient.Search;
			SearchResponse MySearchResponse = await MySearchClient.Item(MySearchRequest);

			if (mySearchType == SearchRequest.Types.Track)
			{
				List<FullTrack> ListOfReturns = MySearchResponse.Tracks.Items;
				if (ListOfReturns.Count > 0)
				{
					if ((ListOfReturns[0].Name.ToLower().Contains(myRelease.Title.ToLower())) || (myRelease.Title.ToLower().Contains(ListOfReturns[0].Name.ToLower())))
					{
						return ListOfReturns[0].ExternalUrls["spotify"];
					}
				}
			}
			else
			{
				List<SimpleAlbum> ListOfReturns = MySearchResponse.Albums.Items;
				if (ListOfReturns.Count > 0)
				{
					if ((ListOfReturns[0].Name.ToLower().Contains(myRelease.Title.ToLower())) || (myRelease.Title.ToLower().Contains(ListOfReturns[0].Name.ToLower())))
					{
						return ListOfReturns[0].ExternalUrls["spotify"];
					}
				}
			}
			return "";

		}
	}
}
