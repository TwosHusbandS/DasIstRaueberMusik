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

			Dictionary<string, string> SpotifySearchReturns = new Dictionary<string, string>();
			if (mySearchType == SearchRequest.Types.Track)
			{
				foreach (var tmp in MySearchResponse.Tracks.Items)
				{
					SpotifySearchReturns.Add(tmp.Name, tmp.ExternalUrls["spotify"]);
				}
			}
			else
			{
				foreach (var tmp in MySearchResponse.Albums.Items)
				{
					SpotifySearchReturns.Add(tmp.Name, tmp.ExternalUrls["spotify"]);
				}
			}

			string MyClosestLink = "";
			int bestComparison = 9999;
			foreach (KeyValuePair<string, string> KVP in SpotifySearchReturns)
			{
				int currComparison = Helper.FileHandling.getLevenshteinDistance(KVP.Key, myRelease.Title);
				if (currComparison < bestComparison)
				{
					MyClosestLink = KVP.Value;
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
