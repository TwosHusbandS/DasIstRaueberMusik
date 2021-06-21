using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeSearch;

namespace DIRM.Scraping
{
	class YoutubeSearch
	{
		public static async Task<string> GetLinkFromSearch(Helper.Release myRelease)
		{
			if (myRelease.ReleaseKind == Helper.ReleaseKinds.Album)
			{
				return "";
			}

			VideoSearch vs = new VideoSearch();
			List<VideoSearchComponents> items = await vs.GetVideos(myRelease.SearchString, 1);

			string myYoutubeLink = "";
			int bestComparison = 9999;

			foreach (var item in items)
			{
				int currComparison = Helper.FileHandling.getLevenshteinDistance(item.getTitle(), myRelease.SearchString);
				if (currComparison < bestComparison)
				{
					myYoutubeLink = item.getUrl();
					bestComparison = currComparison;
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
