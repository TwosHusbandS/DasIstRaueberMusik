﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Resources;
using System.Windows.Media.Imaging;
using System.IO;
using System.Timers;

namespace DIRM
{
	static class Globals
	{

		/// <summary>
		/// Enum for potential Loaded Pages
		/// </summary>
		public enum PageStates
		{
			Main,
			AboutSettings
		}

		/// <summary>
		/// Internal Value for PageState
		/// </summary>
		private static PageStates _PageState = PageStates.Main;


		/// <summary>
		/// Value we use for PageState. Setter is Gucci :*
		/// </summary>
		public static PageStates PageState
		{
			get
			{
				return _PageState;
			}
			set
			{
				// Setting actual Enum to the correct Value
				_PageState = value;
				MainWindow.MW.SetWindowBackgroundBlur();

				// Switch Value
				switch (value)
				{
					// In Case: Settings
					case PageStates.Main:

						// Set actual Frame_Main Content to the correct Page
						//MainWindow.MW.Frame_Main.Content = new Settings();
						//MainWindow.MW.btn_Settings.Style = Application.Current.FindResource("btn_hamburgeritem_selected") as Style;
						//
						//// Call Mouse_Over false on other Buttons where a page is behind
						//MainWindow.MW.btn_Auth.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
						//MainWindow.MW.btn_SaveFiles.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
						break;
					case PageStates.AboutSettings:

						// Set actual Frame_Main Content to the correct Page
						//MainWindow.MW.Frame_Main.Content = new Settings();
						//MainWindow.MW.btn_Settings.Style = Application.Current.FindResource("btn_hamburgeritem_selected") as Style;
						//
						//// Call Mouse_Over false on other Buttons where a page is behind
						//MainWindow.MW.btn_Auth.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
						//MainWindow.MW.btn_SaveFiles.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
						break;
				}
			}
		}



		/// <summary>
		/// Enum for all HamburgerMenuStates
		/// </summary>
		public enum HamburgerMenuStates
		{
			Visible,
			Hidden
		}

		/// <summary>
		/// Internal Value for HamburgerMenuState
		/// </summary>
		private static HamburgerMenuStates _HamburgerMenuState = HamburgerMenuStates.Visible;

		/// <summary>
		/// Value we use for HamburgerMenuState. Setter is Gucci :*
		/// </summary>
		public static HamburgerMenuStates HamburgerMenuState
		{
			get
			{
				return _HamburgerMenuState;
			}
			set
			{
				_HamburgerMenuState = value;
				MainWindow.MW.SetWindowBackgroundBlur();

				if (value == HamburgerMenuStates.Visible)
				{
					// Make invisible
					MainWindow.MW.GridHamburgerOuter.Visibility = Visibility.Visible;
					MainWindow.MW.Grid_Main.ColumnDefinitions[0].Width = new GridLength(300);
					//MainWindow.MW.GridHamburgerOuterSeperator.Visibility = Visibility.Visible;
				}
				// If is not visible
				else
				{
					// Make visible
					MainWindow.MW.GridHamburgerOuter.Visibility = Visibility.Hidden;
					MainWindow.MW.Grid_Main.ColumnDefinitions[0].Width = new GridLength(0);
					//MainWindow.MW.GridHamburgerOuterSeperator.Visibility = Visibility.Hidden;
				}
			}
		}


		public static void Init()
		{
			// Initiates Logging
			// This is also responsible for the intial first few messages on startup.
			Helper.Logger.Init();

			// Initiates the Settings
			// Writes Settings Dictionary [copy of default settings at this point] in the registry if the value doesnt already exist
			// then reads the Regedit Values in the Settings Dictionary
			Settings.Init();

			// Rolling Log stuff
			Helper.Logger.RollingLog();

			CheckForUpdate();

			CheckIfCSVPathExistsOrCanBeCreated();
		}


		public static void CheckIfCSVPathExistsOrCanBeCreated()
		{
			if (!Helper.FileHandling.doesPathExist(Globals.CSVSubfolderPath))
			{
				Helper.FileHandling.createPath(Globals.CSVSubfolderPath);
				if (!Helper.FileHandling.doesPathExist(Globals.CSVSubfolderPath))
				{
					new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupOkError, "Cant create the Path for saving our ReleaseLists. Will Reset Settigns.").ShowDialog();
					Settings.CSVPath = Globals.MyDefaultSettings["CSVPath"];
					Helper.FileHandling.createPath(Globals.CSVSubfolderPath);
				}
			}
		}


		/// <summary>
		/// XML for AutoUpdaterFile
		/// </summary>
		public static string XML_AutoUpdate
		{
			get
			{
				string masterURL = "https://raw.githubusercontent.com/TwosHusbandS/DasIstRaueberMusik/master/DIRM/Installer/update.xml";
				return Helper.FileHandling.GetStringFromURL(masterURL);
			}
		}




		/// <summary>
		/// Method which does the UpdateCheck on Startup
		/// </summary>
		public static void CheckForUpdate(string XML_Autoupdate_Temp = "")
		{
			if (XML_Autoupdate_Temp == "")
			{
				XML_Autoupdate_Temp = XML_AutoUpdate;
			}

			// Check online File for Version.
			string MyVersionOnlineString = Helper.FileHandling.GetXMLTagContent(XML_Autoupdate_Temp, "version");

			// Just so we have one big code snippet we can exit at any point we want.
			while (true)
			{
				// If this is empty,  github returned ""
				if (!(String.IsNullOrEmpty(MyVersionOnlineString)))
				{
					// Building a Version out of the String
					Version MyVersionOnline = new Version(MyVersionOnlineString);

					// Logging some stuff
					Helper.Logger.Log("Checking for DIRM Update during start up procedure");
					Helper.Logger.Log("MyVersionOnline = '" + MyVersionOnline.ToString() + "', Globals.ProjectVersion = '" + Globals.ProjectVersion + "'", 1);

					// If Online Version is "bigger" than our own local Version
					if (MyVersionOnline > Globals.ProjectVersion)
					{
						// Update Found.
						Helper.Logger.Log("Update found (Version Check returning true).", 1);
						Helper.Logger.Log("Checking if URL is reachable.", 1);

						string DLPath = Helper.FileHandling.GetXMLTagContent(XML_Autoupdate_Temp, "url");
						string DLFilename = DLPath.Substring(DLPath.LastIndexOf('/') + 1);
						string LocalFileName = Globals.ProjectInstallationPath.TrimEnd('\\') + @"\" + DLFilename;


						Popups.Popup yesno = new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupYesNo, "Version: '" + MyVersionOnline.ToString() + "' found on the Server.\nVersion: '" + Globals.ProjectVersion.ToString() + "' found installed.\nDo you want to upgrade?");
						yesno.ShowDialog();
						// Asking User if he wants update.
						if (yesno.DialogResult == true)
						{
							// User wants Update
							Helper.Logger.Log("Presented Update Choice to User. User wants it", 1);

							Helper.FileHandling.deleteFile(LocalFileName);
							new Popups.PopupDownload(DLPath, LocalFileName, "Installer").ShowDialog();

							if (Helper.FileHandling.GetSizeOfFile(LocalFileName) > 1000)
							{
								Helper.Logger.Log("Installer on Disk looks good, Starting Installer, Exits this now.", 1);
								Process.Start(LocalFileName);
								Environment.Exit(0);
							}
							else
							{
								Helper.Logger.Log("Installer on Disk does not look good. FileSize: '" + Helper.FileHandling.GetSizeOfFile(LocalFileName) + "'. Will not start it. Will not exit this.", 1);
							}
						}
						else
						{
							// User doesnt want update
							Helper.Logger.Log("User does not wants update", 1);
						}
					}
					else
					{
						// No update found
						Helper.Logger.Log("No Update Found");
					}
				}
				else
				{
					// String return is fucked
					Helper.Logger.Log("Did not get most up to date DIRM Version from Github. Github offline or your PC offline. Probably. Lets hope so.");
				}
				break;
			}
		}




		/// <summary>
		/// Property of the Registry Key we use for our Settings
		/// </summary>													
		public static RegistryKey MySettingsKey { get { return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).CreateSubKey("SOFTWARE").CreateSubKey("DasIstRaueberMusik"); } }

		public static string CSVSubfolderPath
		{
			get
			{
				return Settings.CSVPath.TrimEnd('\\') + @"\DasIstRaueberMusik";
			}
		}
		
		public static string CSVFileName(DateTime myDT)
		{
			return myDT.ToString("yyyy_MM_dd") + ".csv";
		}

		public static string CurrCSVFile
		{
			get
			{
				DateTime myDT = (DateTime)MainWindow.MW.dp.SelectedDate;
				return CSVSubfolderPath.TrimEnd('\\') + @"\" + myDT.ToString("yyyy_MM_dd") + ".csv";
			}
		}

		/// <summary>
		/// Property of our default Settings
		/// </summary>
		public static Dictionary<string, string> MyDefaultSettings { get; private set; } = new Dictionary<string, string>()
		{
			{"CSVPath", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)},
			{"AutoSave", "True" },
			{"AlwaysExportAlphabetically", "False" }
		};

		/// <summary>
		/// Property of our Settings (Dictionary). Gets the default values on initiating the program. Our Settings will get read from registry on the Init functions.
		/// </summary>
		public static Dictionary<string, string> MySettings { get; private set; } = MyDefaultSettings.ToDictionary(entry => entry.Key, entry => entry.Value); // https://stackoverflow.com/a/139626


		public static string Logfile
		{
			get
			{
				return ProjectInstallationPath.TrimEnd('\\') + @"\AAA-Logfile.log";
			}
		}

		public static string TEMPFile
		{
			get
			{
				return ProjectInstallationPath.TrimEnd('\\') + @"\temp.txt";
			}
		}

		public static string ProjectInstallationPath
		{
			get
			{
				return Process.GetCurrentProcess().MainModule.FileName.Substring(0, Process.GetCurrentProcess().MainModule.FileName.LastIndexOf('\\'));
			}
		}


		public static string BuildInfo
		{
			get
			{
				return "Version 0.1.0.2 - Build 1";
			}
		}



		public static bool OfflineErrorThrown = false;

		/// <summary>
		/// Property of our own Project Version
		/// </summary>
		public static Version ProjectVersion = Assembly.GetExecutingAssembly().GetName().Version;

		public static void DebugPopup(string pMsg)
		{
			System.Windows.Forms.MessageBox.Show(pMsg);
		}

	}
}
