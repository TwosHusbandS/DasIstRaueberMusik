using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM.Scraping
{
	class ScrapingLogic
	{
		public static void ExceptionTest()
		{
			string tmp = "asdf";
			Globals.DebugPopup(tmp[999].ToString());
		}

		public static async void ScrapeSpotify()
		{
			MainWindow.MW.btn_GetSpotifyLinks.IsEnabled = false;
			MainWindow.MW.btn_GetSpotifyLinks.Content += "...";

			List<Task<string>> tmp = new List<Task<string>>();
			List<int> ugly = new List<int>();
			for (int i = 0; i <= MainWindow.MW.viewModel.Releases.Count - 1; i++)
			{
				if (!MainWindow.MW.viewModel.Releases[i].Link.ToLower().Contains("spotify"))
				{
					Task<string> tmpTask = Scraping.SpotifyScraper.GetLinkFromSearch(MainWindow.MW.viewModel.Releases[i]);
					tmp.Add(tmpTask);
					ugly.Add(i);
				}
			}

			for (int i = 0; i <= tmp.Count - 1; i++)
			{
				string SpotifyLink = await tmp[i];
				if (!String.IsNullOrWhiteSpace(SpotifyLink))
				{
					MainWindow.MW.viewModel.Change(ugly[i], MainWindow.MW.viewModel.Releases[ugly[i]].Link + " " + SpotifyLink);
				}
			}

			MainWindow.MW.btn_GetSpotifyLinks.Content = MainWindow.MW.btn_GetSpotifyLinks.Content.ToString().TrimEnd('.').TrimEnd('.').TrimEnd('.');
			MainWindow.MW.btn_GetSpotifyLinks.IsEnabled = true;
		}

		public static async void ScrapeYoutube()
		{
			MainWindow.MW.btn_GetYoutubeLinks.IsEnabled = false;
			MainWindow.MW.btn_GetYoutubeLinks.Content += "...";

			bool UseSlowScrape = false;

			// Yeah cant make this quicker, getting rate limited
			for (int i = 0; i <= MainWindow.MW.viewModel.Releases.Count - 1; i++)
			{
				if (!MainWindow.MW.viewModel.Releases[i].Link.ToLower().Contains("youtu") && MainWindow.MW.viewModel.Releases[i].ReleaseKind == Helper.ReleaseKinds.Single) // catches youtube.com and youtu.be
				{
					string YoutubeLink = "";

					if (UseSlowScrape)
					{
						YoutubeLink = await Scraping.YoutubeSearch.GetLinkFromSearch(MainWindow.MW.viewModel.Releases[i]);
					}
					else
					{
						try
						{
							YoutubeLink = await Scraping.YoutubeAPI.GetLinkFromSearch(MainWindow.MW.viewModel.Releases[i]);
						}
						catch
						{
							Helper.Logger.Log("Gonna use the slow youtube scrape instead of the API, because the API is rate limited");
							UseSlowScrape = true;
							if (!Globals.YoutubeSlowWarningThrown)
							{
								MainWindow.MW.SetUpAnnoucement("Will use slow(er) Youtube Scraping since the API RateLimit is reached for today.\nThis resets daily.\n");
								Globals.YoutubeSlowWarningThrown = true;
							}
							YoutubeLink = await Scraping.YoutubeSearch.GetLinkFromSearch(MainWindow.MW.viewModel.Releases[i]);
						}
					}

					if (!String.IsNullOrWhiteSpace(YoutubeLink))
					{
						MainWindow.MW.viewModel.Change(i, MainWindow.MW.viewModel.Releases[i].Link + " " + YoutubeLink);
					}
				}
			}

			MainWindow.MW.btn_GetYoutubeLinks.Content = MainWindow.MW.btn_GetYoutubeLinks.Content.ToString().TrimEnd('.').TrimEnd('.').TrimEnd('.');
			MainWindow.MW.btn_GetYoutubeLinks.IsEnabled = true;
		}

		public static void ExportForReddit()
		{
			DateTime myDT = (DateTime)MainWindow.MW.dp.SelectedDate;
			string CurrFriday = myDT.ToString("dd.MM.yyyy");
			string LastSaturday = myDT.AddDays(-6).ToString("dd.MM.yyyy");

			IList<Helper.Release> myList;
			myList = MainWindow.MW.viewModel.Releases;

			if (Settings.AlwaysExportAlphabetically)
			{
				myList = myList.OrderBy(p => p.Artist).ToList();
			}

			// Open Notepad with correct shit
			List<string> temp = new List<string>();
			temp.Add("**[Release-Friday] - Die Releases am " + CurrFriday + "**");
			temp.Add("");
			temp.Add("---");
			temp.Add("");
			temp.Add("Einen wunderschönen guten Tag zusammen, ich begrüße Sie.  ");
			temp.Add("Im Nachfolgenden sind die Releases der letzten Woche (" + LastSaturday + " - " + CurrFriday + ") aufgelistet.");
			temp.Add("");
			temp.Add("---");
			temp.Add("");
			temp.Add("**Singles**");
			temp.Add("");
			temp.Add("**Artist**|**Title**|**Info**");
			temp.Add(":--|:--|:--");
			foreach (Helper.Release myRelease in myList)
			{
				if (myRelease.ReleaseKind == Helper.ReleaseKinds.Single)
				{
					temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "|" + myRelease.GetInfoTabFromRelease());
				}
			}
			temp.Add("");
			temp.Add("---");
			temp.Add("");
			temp.Add("**Alben**");
			temp.Add("");
			temp.Add("**Artist**|**Title**|**Info**");
			temp.Add(":--|:--|:--");
			foreach (Helper.Release myRelease in myList)
			{
				if (myRelease.ReleaseKind == Helper.ReleaseKinds.Album)
				{
					temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "|" + myRelease.GetInfoTabFromRelease());
				}
			}
			temp.Add("");
			temp.Add("---");
			temp.Add("");
			temp.Add("Wie immer, sollte irgendwas nicht gelistet seien, lasst es mich in den Kommentaren wissen und habt nen schönen Tag!");

			Helper.FileHandling.WriteStringToFileOverwrite(Globals.TEMPFile, temp.ToArray());
			try
			{
				Process.Start("notepad.exe", Globals.TEMPFile);
			}
			catch { }
		}


		public static async void ScrapeReleases(bool OnlyShowSource = false, bool OwnUrl = false)
		{
			Helper.Logger.Log("Lets Scrape");

			DateTime myDT = (DateTime)MainWindow.MW.dp.SelectedDate;
			MainWindow.MW.btn_GetFromDate.Content += "...";

			List<string> ListOfLinks = new List<string>();

			Helper.Logger.Log(String.Format("Starting to scrape: OnlyShowSource=\"{0}\", OwnUrl=\"{1}\", myDT=\"{2}\"", OnlyShowSource, OwnUrl, myDT.ToString("yyyy_MM_dd")));

			if (OwnUrl)
			{
				Popups.PopupTextBox tmp = new Popups.PopupTextBox("Enter Link:", "Link here");
				tmp.ShowDialog();
				if (tmp.MyReturnString != "")
				{
					Helper.Logger.Log("Custom Link: \"" + tmp.MyReturnString + "\"");
					ListOfLinks.Add(tmp.MyReturnString);
				}
			}
			else
			{
				Helper.Logger.Log("Trying to scrape RSS");

				ListOfLinks = await Scraping.RSS_Scraper.GetLinksAsync(myDT);

				Helper.Logger.Log("Done scraping RSS");
			}



			if (ListOfLinks.Count == 0)
			{
				Helper.Logger.Log("Havent found a release Link. (" + myDT.ToString("yyyy - MM - dd") + ")");
				new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupOkError, "Havent found a release Post Link for that date. (" + myDT.ToString("dd.MM.yyyy") + ")").ShowDialog();
			}
			else
			{
				Helper.Logger.Log("Found " + ListOfLinks.Count + " Link(s). (" + myDT.ToString("yyyy - MM - dd") + ")");
				int counter = 0;
				foreach (string MyLink in ListOfLinks)
				{
					Helper.Logger.Log("ReleaseLink[" + counter + "]: " + MyLink);

					List<Helper.Release> ReleaseList = new List<Helper.Release>();

					try
					{
						string myWebSource = await Scraping.DeinUpdate_Scraper.GetWebsiteSource(MyLink);

						if (OnlyShowSource)
						{

							Helper.FileHandling.WriteStringToFileOverwrite(Globals.TEMPFile, new[] { myWebSource });
							try
							{
								Process.Start("notepad.exe", Globals.TEMPFile);
							}
							catch { }

							break;
						}
						Helper.Logger.Log("Trying to get all Releases from this DeinUpdate post: " + MyLink);
						ReleaseList = await Scraping.DeinUpdate_Scraper.GetReleasesFromLinkAsync(myWebSource);
					}
					catch (Exception ex)
					{
						Helper.Logger.Log(ex.Message);
						new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupOkError, ex.Message).ShowDialog();
					}


					MainWindow.MW.viewModel.MyAdd(ReleaseList);
				}
			}


			MainWindow.MW.btn_GetFromDate.Content = MainWindow.MW.btn_GetFromDate.Content.ToString().TrimEnd('.').TrimEnd('.').TrimEnd('.');
			Helper.Logger.Log("Done scraping");

		}
	}
}
