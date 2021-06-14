using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIRM.Helper
{
	class CSVHelper
	{
		static string HeadingLine = "Artist" + CharSeperator + "Title" + CharSeperator + "ReleaseKind" + CharSeperator + "Info" + CharSeperator + "Link";

		static char CharSeperator = '=';

		public static IList<Helper.Release> Read(string FilePath)
		{
			MainWindow.MW.viewModel.Releases.Clear();

			IList<Helper.Release> myReleaseList = new List<Helper.Release>();

			string[] filecontent = Helper.FileHandling.ReadFileEachLine(FilePath);
			if (filecontent.Length > 1)
			{
				if (filecontent[0] == HeadingLine)
				{
					for (int i = 1; i <= filecontent.Length - 1; i++)
					{
						string[] Line = filecontent[i].Split(CharSeperator);
						if (Line.Length >= 5)
						{
							ReleaseKinds tmp;

							if (Line[2].ToLower() == "album")
							{
								tmp = ReleaseKinds.Album;
							}
							else
							{
								tmp = ReleaseKinds.Single;
							}

							myReleaseList.Add(new Helper.Release
							{
								Artist = Line[0],
								Title = Line[1],
								ReleaseKind = tmp,
								Info = Line[3],
								Link = Line[4]
							});
						}
					}
				}
			}
			return myReleaseList;

		}


		public static void Save(bool pManualSafe = false)
		{
			Save(Globals.CurrCSVFile, MainWindow.MW.viewModel.Releases, pManualSafe);
		}

		public static void Save(string FilePath, IList<Release> ListOfReleases, bool pManualSave = false)
		{
			if (pManualSave || Settings.AutoSave)
			{
				List<string> temp = new List<string>();
				temp.Add(HeadingLine);
				foreach (Release myRelease in ListOfReleases)
				{
					temp.Add(myRelease.Artist + CharSeperator + myRelease.Title + CharSeperator + myRelease.ReleaseKind.ToString() + CharSeperator + myRelease.Info + CharSeperator + myRelease.Link);
				}
				Helper.FileHandling.WriteStringToFileOverwrite(FilePath, temp.ToArray());
			}
		}
	}
}
