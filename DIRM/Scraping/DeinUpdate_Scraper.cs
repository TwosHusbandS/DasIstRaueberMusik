using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DIRM.Scraping
{
	class DeinUpdate_Scraper
	{
		public async static Task<string> GetWebsiteSource(string pLink)
		{
			Helper.Logger.Log("Trying to get the source of website: \"" + pLink + "\"");
			string webSource = "";
			HttpClient myHttpClient = new HttpClient();
			try
			{
				// no matter what or how we do it, if they want to fuck us, they are capable of doing this.
				// if any of deinupdate staff read this, contact me at discord @thS#0305 if you want me to stop scraping. have a nice day : )
				myHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (iPhone; CPU iPhone OS 12_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/12.1 Mobile/15E148 Safari/604.1");
				webSource = await myHttpClient.GetStringAsync(pLink);
			}
			catch (Exception ex)
			{
				Helper.Logger.Log("Failed to get the soruce of website: \"" + pLink + "\", Exception: " + ex.ToString());
				throw new NullReferenceException("Failed to get source of Website (" + pLink + ").");
			}
			return webSource;
		}


		public async static Task<List<Helper.Release>> GetReleasesFromLinkAsync(string pWebSource)
		{
			List<Helper.Release> myReleaseList = new List<Helper.Release>();

			string webSource = pWebSource;

			webSource.Replace("\n", "");
			webSource.Replace("\r", "");
			webSource = Regex.Replace(webSource, @"\r\n?|\n", "");

			// Regex Pattern to get the big block of usefull data. 
			string RegexPattern = @"<div class=""rhonda-full-entry"">.{1,}<h4 style=""text-align: center;""><strong>.{1,}<\/strong><\/h4>";

			// Main regex match to get the big block of data.
			Regex MyRegex = new Regex(RegexPattern);
			Match MyMatch = MyRegex.Match(webSource);
			if (MyMatch.Success && MyMatch.Groups.Count > 0)
			{
				Helper.Logger.Log("Big Regex DOES match. MyMatchUndSo (post .Replace Actions) coming right up.");

				string MyMatchUndSo = MyMatch.Groups[0].ToString();


				// Clean up some of the data we later need
				MyMatchUndSo = MyMatchUndSo.Replace("&amp;", "&");
				MyMatchUndSo = MyMatchUndSo.Replace("&#8211;", "");
				MyMatchUndSo = MyMatchUndSo.Replace("&#8217;", "'");
				MyMatchUndSo = MyMatchUndSo.Replace("&#8222;", "");
				MyMatchUndSo = MyMatchUndSo.Replace("&#8220;", "");
				MyMatchUndSo = MyMatchUndSo.Replace(" <em>", "<em>");
				MyMatchUndSo = MyMatchUndSo.Replace("<em>", ",");
				MyMatchUndSo = MyMatchUndSo.Replace(" ,", ",");
				MyMatchUndSo = MyMatchUndSo.Replace("</em>", "|");

				Helper.Logger.Log(MyMatchUndSo);


				// Stripping off the start of the string (up until and including the "<h4>Albums</h4>" part
				try
				{
					MyMatchUndSo = MyMatchUndSo.Substring(MyMatchUndSo.ToLower().IndexOf("</h4>".ToLower()) + "</h4>".Length);
				}
				catch
				{
					throw new NullReferenceException("Failed to strip to the start of the big match.");
				}


				// html which separtes the albums from singles 
				string seperatingHTML = @"<h4 style=""text-align: center;""><strong>SINGLE</strong></h4>";
				if (MyMatchUndSo.ToLower().IndexOf(seperatingHTML.ToLower()) < 1)
				{
					seperatingHTML = @"<h4 style=""text-align: center;""><strong>SINGLES</strong></h4>";
				}


				string Albums = "";
				string Singles = "";
				// Stripping off the start of the string (up until and including the "<h4>Albums</h4>" part
				try
				{
					// Have an all-albums and an all-singles string. Those still need some work each (mainly splitting into each single / album)
					Albums = MyMatchUndSo.Substring(0, MyMatchUndSo.ToLower().IndexOf(seperatingHTML.ToLower()));
					Singles = MyMatchUndSo.Substring(MyMatchUndSo.ToLower().IndexOf(seperatingHTML.ToLower()) + seperatingHTML.Length);
					// Those are one string: "Fler,NDW|Bushido,CCN|Dude,Albumtitel
				}
				catch
				{
					throw new NullReferenceException("Failed to separate into Albums and Singles.");
				}


				try
				{
					// Trimming the end of Singles. Some h4 headers which are still caught in big regex match
					Singles = Singles.Substring(0, Singles.IndexOf(@"<h4"));
				}
				catch
				{
					throw new NullReferenceException("Failed to trim the end of singles");
				}

				// Putting it together, adding an A or an S, depending on what
				string AllReleases = Albums.Replace(",", ",A,") + Singles.Replace(",", ",S,");
				// This is: "Fler,A,NDW|Bushido,S,Sonnenbank flavour|Eko Fresh,S,700 Bars

				// split those into ReleaseLines
				string[] MyReleaseLines = AllReleases.Split('|');

				// Those are string arrays of strings like: "Fler,A,NDW"
				foreach (string MyReleaseLine in MyReleaseLines)
				{
					string[] Content = MyReleaseLine.Split(',');

					// Hopefully: Content[0]="Fler" and Content[1]="NDW", lets check that really quick	
					if (Content.Length > 2)
					{
						// Trim some shit at start and end again...
						string MyArtist = Content[0].TrimStart(' ').TrimEnd(' ');
						string MyTitle = Content[2].TrimStart(' ').TrimEnd(' ');

						if (Content[1] == "A")
						{
							myReleaseList.Add(new Helper.Release { Artist = MyArtist, Title = MyTitle, ReleaseKind = Helper.ReleaseKinds.Album });
						}
						else if (Content[1] == "S")
						{
							myReleaseList.Add(new Helper.Release { Artist = MyArtist, Title = MyTitle, ReleaseKind = Helper.ReleaseKinds.Single });
							//myReleaseList.Add(new Release(MyArtist, MyTitle, ReleaseKinds.Single));
						}
					}
				}
			}
			else
			{
				Helper.Logger.Log("Big Regex doesnt match. Websource coming right up.");
				Helper.Logger.Log(webSource);

				throw new NullReferenceException("Big Regex doesnt match");
			}

			return myReleaseList;
		}
	}
}
