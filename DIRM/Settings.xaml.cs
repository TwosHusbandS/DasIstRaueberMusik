using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DIRM
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : Page
	{
		public static SettingsStates LastSettingsState = SettingsStates.Settings;

		///*
		public Settings()
		{
			// WPF Shit
			InitializeComponent();

			// Setting ReadMeState to the LastReadMeState
			SettingsState = Settings.LastSettingsState;
		}

		/// <summary>
		/// Enum for all ReadMeStates
		/// </summary>
		public enum SettingsStates
		{
			About,
			Settings
		}

		/// <summary>
		/// Internal Value
		/// </summary>
		private SettingsStates _SettingsState = SettingsStates.Settings;

		/// <summary>
		/// Value we get and set. Setters are gucci. 
		/// </summary>
		public SettingsStates SettingsState
		{
			get
			{
				return _SettingsState;
			}
			set
			{
				_SettingsState = value;

				// Saving it in LastReadMeState
				Settings.LastSettingsState = value;

				if (value == SettingsStates.About)
				{
					MyFrame.Content = new SettingsPages.Settings_About();
					btn_Hamburger_About.Style = Application.Current.FindResource("btn_hamburgeritem_selected") as Style;
					btn_Hamburger_Settings.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
				}
				else if (value == SettingsStates.Settings)
				{
					MyFrame.Content = new SettingsPages.Settings_SettingsGUI();
					btn_Hamburger_Settings.Style = Application.Current.FindResource("btn_hamburgeritem_selected") as Style;
					btn_Hamburger_About.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
				}
			}
		}





		/// <summary>
		/// Initial Function. Gets Called from Globals.Init which gets called from the Contructor of MainWindow
		/// </summary>
		public static void Init()
		{

			Helper.Logger.Log("Initiating Settings", true, 0);
			Helper.Logger.Log("Initiating Regedit Setup Part of Settings", true, 1);

			// Writes our Settings (Copy of DefaultSettings) to Registry if the Value does not exist.
			Helper.Logger.Log("Writing MySettings (at this point a copy of MyDefaultSettings to the Regedit if the Value doesnt exist", true, 1);
			foreach (KeyValuePair<string, string> KVP in Globals.MySettings)
			{
				if (!(Helper.RegistryHandler.DoesValueExists(KVP.Key)))
				{
					Helper.Logger.Log("Writing '" + KVP.Key.ToString() + "' to the Registry (Value: '" + KVP.Value.ToString() + "') on Startup of P127, because it doesnt exist or is empty", true, 2);
					Helper.RegistryHandler.SetValue(KVP.Key, KVP.Value);
				}
			}

			Helper.Logger.Log("Done Initiating Regedit Part of Settings", true, 1);

			// Read the Registry Values in the Settings Dictionary
			LoadSettings();
		}





		/// <summary>
		/// Gets a specific Setting from the Dictionary. Does not query from Registry.
		/// </summary>
		/// <param name="pKey"></param>
		/// <returns></returns>
		public static string GetSetting(string pKey)
		{
			return Globals.MySettings[pKey];
		}

		/// <summary>
		/// Sets a specific Setting both in the Dictionary and in the Registry
		/// </summary>
		/// <param name="pKey"></param>
		/// <param name="pValue"></param>
		public static void SetSetting(string pKey, string pValue)
		{
			Helper.Logger.Log("Changing Setting '" + pKey + "' to '" + pValue + "'");
			try
			{
				Helper.RegistryHandler.SetValue(pKey, pValue);
				Globals.MySettings[pKey] = pValue;
			}
			catch
			{
				Helper.Logger.Log("Failed to Settings.cs SetSetting(" + pKey + ", " + pValue + ")");
			}
		}

		/// <summary>
		/// Loads all the Settings from the Registry into the Dictionary
		/// </summary>
		public static void LoadSettings()
		{
			Helper.Logger.Log("Loading Settings from Regedit", true, 1);
			foreach (KeyValuePair<string, string> SingleSetting in Globals.MyDefaultSettings)
			{
				Globals.MySettings[SingleSetting.Key] = Helper.RegistryHandler.GetValue(SingleSetting.Key);
			}
			Helper.Logger.Log("Loaded Settings from Regedit", true, 1);

		}

		/// <summary>
		/// Resets all Settings to Default Settings 
		/// </summary>
		private static void ResetSettings()
		{
			Helper.Logger.Log("Resetting Settings from Regedit", true, 1);
			foreach (KeyValuePair<string, string> SingleDefaultSetting in Globals.MyDefaultSettings)
			{
				SetSetting(SingleDefaultSetting.Key, SingleDefaultSetting.Value);
			}
			Helper.Logger.Log("Resetted Settings from Regedit", true, 1);
		}

		/// <summary>
		/// Gets Bool from String
		/// </summary>
		/// <param name="pString"></param>
		/// <returns></returns>
		public static bool GetBoolFromString(string pString)
		{
			bool tmpBool;
			bool.TryParse(pString, out tmpBool);
			return tmpBool;
		}


		// Below are Properties for all Settings, which is easier to Interact with than the Dictionary


		/// <summary>
		/// Settings AutoSave. Gets from the Dictionary.
		/// </summary>
		public static bool AutoSave
		{
			get
			{
				return GetBoolFromString(GetSetting("AutoSave"));
			}
			set
			{
				SetSetting("AutoSave", value.ToString());
				MainWindow.MW.SetSaveButtonVisibilityBasedOnSettings();
			}
		}


		/// <summary>
		/// Settings AlwaysExportAlphabetically. Gets from the Dictionary.
		/// </summary>
		public static bool AlwaysExportAlphabetically
		{
			get
			{
				return GetBoolFromString(GetSetting("AlwaysExportAlphabetically"));
			}
			set
			{
				SetSetting("AlwaysExportAlphabetically", value.ToString());
			}
		}

		/// <summary>
		/// Settings CSVPath. Gets from the Dictionary.
		/// </summary>
		public static string CSVPath
		{
			get
			{
				return GetSetting("CSVPath");
			}
			set
			{
				SetSetting("CSVPath", value.ToString());
			}
		}

		private void MyFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Forward)
			{
				e.Cancel = true;
			}
		}

		private void btn_Hamburger_About_Click(object sender, RoutedEventArgs e)
		{
			this.SettingsState = SettingsStates.About;
		}

		private void btn_Hamburger_Settings_Click(object sender, RoutedEventArgs e)
		{
			this.SettingsState = SettingsStates.Settings;
		}
	} // End of partial Class
} // End of Namespace
