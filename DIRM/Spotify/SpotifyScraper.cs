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

			string SearchString = myRelease.Artist.Replace("&", "") + " " + myRelease.Title;
			Helper.Logger.Log("Searching for: \"" + mySearchType.ToString() + "\", SearchString: \"" + SearchString + "\", Title: \"" + myRelease.Title + "\"");

			SearchRequest MySearchRequest = new SearchRequest(mySearchType, SearchString);
			ISearchClient MySearchClient = MySpotifyClient.Search;
			SearchResponse MySearchResponse = await MySearchClient.Item(MySearchRequest);

			if (mySearchType == SearchRequest.Types.Track)
			{
				List<FullTrack> ListOfReturns = MySearchResponse.Tracks.Items;
				FullTrack myClosestFullTrack = null;
				int bestComparison = 9999;
				foreach (FullTrack myFullTrack in ListOfReturns)
				{
					int currComparison = getLevenshteinDistance(myFullTrack.Name, myRelease.Title);
					Helper.Logger.Log("Found a Title = \"" + myFullTrack.Name.ToString() + "\", Link: \"" + myFullTrack.ExternalUrls["spotify"] + "\", Similarity: \"" + getLevenshteinDistance(myFullTrack.Name, myRelease.Title).ToString() + "\"", 4);
					if (currComparison < bestComparison)
					{
						myClosestFullTrack = myFullTrack;
						bestComparison = currComparison;
					}
				}

				if (myClosestFullTrack != null)
				{
					Helper.Logger.Log("Found a Search Result for that. Title = \"" + myClosestFullTrack.Name.ToString() + "\", Link: \"" + myClosestFullTrack.ExternalUrls["spotify"] + "\", Similarity: \"" + getLevenshteinDistance(myClosestFullTrack.Name, myRelease.Title).ToString() + "\"", 2);
					return myClosestFullTrack.ExternalUrls["spotify"];
				}
			}
			else
			{
				List<SimpleAlbum> ListOfReturns = MySearchResponse.Albums.Items;
				SimpleAlbum myClosestSimpleAlbum = null;
				int bestComparison = 9999;
				foreach (SimpleAlbum mySimpleAlbum in ListOfReturns)
				{
					int currComparison = getLevenshteinDistance(mySimpleAlbum.Name, myRelease.Title);
					if (currComparison < bestComparison)
					{
						myClosestSimpleAlbum = mySimpleAlbum;
						bestComparison = currComparison;
					}
				}

				if (myClosestSimpleAlbum != null)
				{
					Helper.Logger.Log("Found a Search Result for that. Title = \"" + myClosestSimpleAlbum.Name.ToString() + "\", Link: \"" + myClosestSimpleAlbum.ExternalUrls["spotify"] + "\", Similarity: \"" + getLevenshteinDistance(myClosestSimpleAlbum.Name, myRelease.Title).ToString() + "\"", 2);
					return myClosestSimpleAlbum.ExternalUrls["spotify"];
				}
			}


			Helper.Logger.Log("-----------");


			return "";

		}





		// https://web.archive.org/web/20120526085419/http://www.merriampark.com/ldjava.htm


		public static int getLevenshteinDistance(string s, string t)
		{

			/*
			  The difference between this impl. and the previous is that, rather 
			   than creating and retaining a matrix of size s.length()+1 by t.length()+1, 
			   we maintain two single-dimensional arrays of length s.length()+1.  The first, d,
			   is the 'current working' distance array that maintains the newest distance cost
			   counts as we iterate through the characters of String s.  Each time we increment
			   the index of String t we are comparing, d is copied to p, the second int[].  Doing so
			   allows us to retain the previous cost counts as required by the algorithm (taking 
			   the minimum of the cost count to the left, up one, and diagonally up and to the left
			   of the current cost count being calculated).  (Note that the arrays aren't really 
			   copied anymore, just switched...this is clearly much better than cloning an array 
			   or doing a System.arraycopy() each time  through the outer loop.)

			   Effectively, the difference between the two implementations is this one does not 
			   cause an out of memory condition when calculating the LD over two very large strings.  		
			*/

			if (String.IsNullOrWhiteSpace(s) || String.IsNullOrWhiteSpace(t))
			{
				return 9999;
			}

			s = s.ToLower();
			t = t.ToLower();

			int n = s.Length; // length of s
			int m = t.Length; // length of t

			if (n == 0)
			{
				return m;
			}
			else if (m == 0)
			{
				return n;
			}

			int[] p = new int[n + 1]; //'previous' cost array, horizontally
			int[] d = new int[n + 1]; // cost array, horizontally
			int[] _d; //placeholder to assist in swapping p and d

			// indexes into strings s and t
			int i; // iterates through s
			int j; // iterates through t

			char t_j; // jth character of t

			int cost; // cost

			for (i = 0; i <= n; i++)
			{
				p[i] = i;
			}

			for (j = 1; j <= m; j++)
			{
				t_j = t[j - 1];
				d[0] = j;

				for (i = 1; i <= n; i++)
				{
					cost = s[i - 1] == t_j ? 0 : 1;
					// minimum of cell to the left+1, to the top+1, diagonally left and up +cost				
					d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
				}

				// copy current distance counts to 'previous row' distance counts
				_d = p;
				p = d;
				d = _d;
			}

			// our last action in the above loop was to switch d and p, so p now 
			// actually has the most recent cost counts
			return p[n];
		}

	}
}
