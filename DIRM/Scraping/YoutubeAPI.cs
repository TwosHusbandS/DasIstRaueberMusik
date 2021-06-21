using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace DIRM.Scraping
{
	class YoutubeAPI
	{
		static bool ManualStaticConstructorRunAlready = false;

		static YouTubeService myYoutubeService = null;

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

			myYoutubeService = new YouTubeService(new BaseClientService.Initializer()
			{
				ApiKey = Helper.FileHandling.GetXMLTagContent(what, "YOUTUBE_API_KEY"),
				ApplicationName = "DIRM"
			});
		}


		public static async Task<string> GetLinkFromSearch(Helper.Release myRelease)
		{
			if (!ManualStaticConstructorRunAlready)
			{
				Init();
				ManualStaticConstructorRunAlready = true;
			}

			if (myRelease.ReleaseKind == Helper.ReleaseKinds.Album)
			{
				return "";
			}

			var searchListRequest = myYoutubeService.Search.List("snippet");
			searchListRequest.Q = myRelease.SearchString;
			searchListRequest.MaxResults = 50;

			var searchListResponse = await searchListRequest.ExecuteAsync();

			string myYoutubeLink = "";
			int bestComparison = 9999;

			foreach (var item in searchListResponse.Items)
			{
				switch (item.Id.Kind)
				{
					case "youtube#video":
						int currComparison = Helper.FileHandling.getLevenshteinDistance(item.Snippet.Title, myRelease.SearchString);
						if (currComparison < bestComparison)
						{
							myYoutubeLink = "https://www.youtube.com/watch?v=" + item.Id.VideoId;
							bestComparison = currComparison;
						}
						break;
				}
			}

			if (!String.IsNullOrWhiteSpace(myYoutubeLink))
			{
				return myYoutubeLink;
			}

			return "";
		}
	}
}
