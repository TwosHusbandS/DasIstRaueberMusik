using CodeHollow.FeedReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM.Scraping
{
//	https://www.deinupdate.de/?feed=rss2&cat=4
//	https://www.deinupdate.de/?feed=rss2&cat=4&paged=2
//	https://www.deinupdate.de/?feed=rss2&cat=4&s=Alle%20Rap-Songs%2C%20die%20heute%20erschienen%20sind%21
//	https://www.feed-reader.net/index.php
//	https://validator.w3.org/feed/check.cgi?url=https%3A%2F%2Fwww.deinupdate.de%2F%3Ffeed%3Drss2%26cat%3D4
//	https://www.deinupdate.de/?feed=rss2&orderby=pubdate&order=DESC
//	https://www.deinupdate.de/?feed=rss2&cat=4&m=20191010&paged=5
//	https://www.reddit.com/r/GermanRap/submit?selftext=true




	/// <summary>
	/// Class we are using to get Information off of the RSS Feed by DeinUpdate
	/// </summary>
	class RSS_Scraper
	{

		/// <summary>
		/// The Base-URL of the RSS Feed we are using
		/// </summary>
		public static string URLFeed
		{
			get
			{
				return "https://www.deinupdate.de/?feed=rss2&cat=4";
			}
		}

		/// <summary>
		/// The String we are searching for in Release Titles.
		/// </summary>
		public static string SearchString
		{
			get
			{
				return "Alle Rap-Songs, die heute erschienen sind";
			}
		}

		/// <summary>
		/// Builds the URL Search Part of the RSS Feed URL, based on the SearchString above
		/// </summary>
		public static string URLPartSearch
		{
			get
			{
				return "&s=" + SearchString.Replace(" ", "%20").Replace(",", "%2C");
			}
		}

		/// <summary>
		/// Builds the URL Part of the RSS Feed URL
		/// </summary>
		/// <param name="myDateTime"></param>
		/// <returns></returns>
		public static string URLPartDate(DateTime myDateTime)
		{
			return "&m=" + myDateTime.Year.ToString("0000") + myDateTime.Month.ToString("00") + myDateTime.Day.ToString("00");
		}


		public static async Task<List<string>> GetLinksAsync(DateTime myStartDate, DateTime myEndDate)
		{
			List<string> rtrn = new List<string>();

			// myStartDate is officially a friday now.
			while (myStartDate.DayOfWeek != DayOfWeek.Friday)
			{
				myStartDate = myStartDate.AddDays(1);
			}

			// myEndDate if oficially a friday now
			if (myEndDate.DayOfWeek == DayOfWeek.Thursday)
			{
				myEndDate = myEndDate.AddDays(1);
			}
			else
			{
				while (myEndDate.DayOfWeek != DayOfWeek.Friday)
				{
					myEndDate = myEndDate.AddDays(-1);
				}
			}

			// if enddate is before startdate
			if (myEndDate < myStartDate)
			{
				return rtrn;
			}


			List<DateTime> ListOfFridays = new List<DateTime>();

			foreach (DateTime OneFriday in ListOfFridays)
			{
				List<string> ReleaseLinksFromOneFriday = await GetLinksAsync(OneFriday);

				rtrn = rtrn.Concat(ReleaseLinksFromOneFriday).ToList<string>();
			}

			return rtrn;
		}







		/// <summary>
		/// Gets all Release Post Links for a Friday and the corresponding Thursday
		/// </summary>
		/// <param name="MyDateTime"></param>
		/// <returns></returns>
		/// 
		public static async Task<List<string>> GetLinksAsync(DateTime MyDateTime)
		{
			Helper.Logger.Log("Inside scraping RSS - Start");

			// Return List
			List<string> rtrn = new List<string>();

			// MyDateTime is always a friday
			if (MyDateTime.DayOfWeek == DayOfWeek.Thursday)
			{
				MyDateTime = MyDateTime.AddDays(1);
			}
			else
			{
				while (MyDateTime.DayOfWeek != DayOfWeek.Friday)
				{
					MyDateTime = MyDateTime.AddDays(-1);
				}
			}

			Helper.Logger.Log("Inside scraping RSS - Before Tasks");

			// Starting Tasks to get Feed Items from Yesterday and Today (in relation to myDateTime Param)
			Task<List<FeedItem>> myFIYesterdayTask = RSS_Scraper.GetItemsFromDay(MyDateTime.AddDays(-1), true);
			Task<List<FeedItem>> myFITodayTask = RSS_Scraper.GetItemsFromDay(MyDateTime, true);

			// Awaiting the Task to get the actual List of FeedItems.
			List<FeedItem> myFIYesterday = await myFIYesterdayTask;
			List<FeedItem> myFIToday = await myFITodayTask;

			Helper.Logger.Log("Inside scraping RSS - Post Tasks");

			// Combining them to one List
			List<FeedItem> myFI = myFIYesterday.Concat(myFIToday).ToList<FeedItem>();

			// Looping through the List
			foreach (FeedItem FI in myFI)
			{
				// Add it to return List
				rtrn.Add(FI.Link);
			}

			Helper.Logger.Log("Inside scraping RSS - End");

			return rtrn;
		}

		public static async Task<List<FeedItem>> GetItemsFromDay(DateTime myDay, bool OnlyGetReleasePosts = false)
		{
			Helper.Logger.Log("Inside scraping RSS - Specific Day - Start");

			// Return Variable
			List<FeedItem> myFeedItems = new List<FeedItem>();

			// Building the URL
			string myUrl = RSS_Scraper.URLFeed + RSS_Scraper.URLPartDate(myDay);

			// Adding Search Part if wanted
			if (OnlyGetReleasePosts)
			{
				myUrl += RSS_Scraper.URLPartSearch;
			}

			// Page we are currently looking at
			int Page = 1;

			Helper.Logger.Log("Inside scraping RSS - Specific Day - Before While");

			// infinite loop
			while (true)
			{
				Helper.Logger.Log("Inside scraping RSS - Specific Day - In While At Top");

				// Build Feed with current Page
				Feed myFeed = await FeedReader.ReadAsync(myUrl + "&paged=" + Page.ToString());

				// If no items in Page, break out of Loop
				if (myFeed.Items.Count == 0)
				{
					break;
				}

				// Loop through all Items in current Feed
				foreach (FeedItem FI in myFeed.Items)
				{
					// Add to return Variable
					myFeedItems.Add(FI);
				}

				// Increment Page
				Page += 1;
			}

			Helper.Logger.Log("Inside scraping RSS - Specific Day - End");

			// return List of all FeedItems we got
			return myFeedItems;
		}
	}
}
