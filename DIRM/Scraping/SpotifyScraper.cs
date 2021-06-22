using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM.Scraping
{
	class SpotifyScraper
	{
		static SpotifyClientConfig MySpotifyClientConfig;
		static SpotifyClient MySpotifyClient;

		static bool ManualStaticConstructorRunAlready = false;

		public static void Init()
		{
			string what = "";
			// See Spotify\MyAPIConfig_Example.ini
			Uri jsfURI = new Uri(@"Scraping\MyAPIConfig.ini", UriKind.Relative);
			var jsfStream = System.Windows.Application.GetResourceStream(jsfURI);
			using (var reader = new StreamReader(jsfStream.Stream))
			{
				what = reader.ReadToEnd();
			}

			MySpotifyClientConfig = SpotifyClientConfig
	.CreateDefault()
	.WithAuthenticator(new ClientCredentialsAuthenticator(Helper.FileHandling.GetXMLTagContent(what, "SPOTIFY_CLIENT_ID"), Helper.FileHandling.GetXMLTagContent(what, "SPOTIFY_CLIENT_SECRET")));

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

			SearchRequest MySearchRequest = new SearchRequest(mySearchType, myRelease.SearchString);
			ISearchClient MySearchClient = MySpotifyClient.Search;
			SearchResponse MySearchResponse = await MySearchClient.Item(MySearchRequest);

			List<string> mySpotifySearchReturnsTitles = new List<string>();
			List<string> mySpotifySearchReturnsLinks = new List<string>();
			if (mySearchType == SearchRequest.Types.Track)
			{
				foreach (var tmp in MySearchResponse.Tracks.Items)
				{
					mySpotifySearchReturnsTitles.Add(tmp.Name);
					mySpotifySearchReturnsLinks.Add(tmp.ExternalUrls["spotify"]);
				}
			}
			else
			{
				foreach (var tmp in MySearchResponse.Albums.Items)
				{
					mySpotifySearchReturnsTitles.Add(tmp.Name);
					mySpotifySearchReturnsLinks.Add(tmp.ExternalUrls["spotify"]);
				}
			}

			string MyClosestLink = "";
			int bestComparison = 9999;
			for (int i = 0; i <= mySpotifySearchReturnsTitles.Count -1; i++)
			{
				int currComparison = Helper.FileHandling.getLevenshteinDistance(mySpotifySearchReturnsTitles[i], myRelease.Title);
				if (currComparison < bestComparison)
				{
					MyClosestLink = mySpotifySearchReturnsLinks[i];
					bestComparison = currComparison;
				}
			}

			if (!String.IsNullOrWhiteSpace(MyClosestLink))
			{
				return MyClosestLink;
			}

			return "";
		}

	}
}
