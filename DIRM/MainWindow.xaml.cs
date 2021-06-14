/*

DIRM = Das ist Räuber Musik

Basically gets all the Titles and Artists from a release from the RSS of: deinupdate

https://www.deinupdate.de/?feed=rss2&cat=4
https://www.deinupdate.de/?feed=rss2&cat=4&paged=2
https://www.deinupdate.de/?feed=rss2&cat=4&s=Alle%20Rap-Songs%2C%20die%20heute%20erschienen%20sind%21
https://www.feed-reader.net/index.php
https://validator.w3.org/feed/check.cgi?url=https%3A%2F%2Fwww.deinupdate.de%2F%3Ffeed%3Drss2%26cat%3D4
https://www.deinupdate.de/?feed=rss2&orderby=pubdate&order=DESC
https://www.deinupdate.de/?feed=rss2&cat=4&m=20191010&paged=5
https://www.reddit.com/r/GermanRap/submit?selftext=true


ToDo:

git, github, Installer etc....

Colors, Styles etc...for MainWindow and popups.
Clean up code, add logging, etc etc...Maybe. If i feel like it. Devs be lazy.

*/


using CodeHollow.FeedReader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ViewModel viewModel;

		// Lets get dirty here...
		public static MainWindow MW;

		/// <summary>
		/// Constructor of our Main Window
		/// </summary>
		public MainWindow()
		{
			// Initializes WPF Shit
			InitializeComponent();

			MW = this;

			Globals.Init();

			this.viewModel = new ViewModel
			{
				Releases = new ObservableCollection<Helper.Release>()
				{

				}
			};

			this.DataContext = this.viewModel;

			InitDatePicker();

			MainWindow.MW.SaveButtonVisibilityRefresh();
		}

		/// <summary>
		/// 
		/// </summary>
		public void InitDatePicker()
		{
			DateTime LatestFridayRelease;

			// Sets DisplayDateEnd to today or tomorrow (if tomorrow is Friday)
			if (DateTime.Today.DayOfWeek == DayOfWeek.Friday)
			{
				LatestFridayRelease = DateTime.Today;
			}
			else if (DateTime.Today.DayOfWeek == DayOfWeek.Thursday)
			{
				LatestFridayRelease = DateTime.Today.AddDays(1);
			}
			else
			{
				LatestFridayRelease = DateTime.Today.AddDays(-7 + ((int)DayOfWeek.Friday - (int)DateTime.Today.DayOfWeek));
			}



			// If tomorrow is Friday
			if (DateTime.Today.AddDays(1).DayOfWeek == DayOfWeek.Friday)
			{
				// Set DisplayDateEnd of DatePicker to Tomorrow
				dp.DisplayDateEnd = DateTime.Today.AddDays(1);

				// Set LatestFridayRelease to Tomorrow
				LatestFridayRelease = DateTime.Today.AddDays(1);
			}
			// If tomorrow is not Friday
			else
			{
				// Set DisplayDateEnd of DatePicker to Today
				dp.DisplayDateEnd = DateTime.Today;

				// Set LatestFridayRelease to last Friday (or Today)
				// Can probably improve this with Math instead of while loop
				LatestFridayRelease = DateTime.Today;
				while (LatestFridayRelease.DayOfWeek != DayOfWeek.Friday)
				{
					LatestFridayRelease = LatestFridayRelease.AddDays(-1);
				}
			}

			// Set DisplayStartDate of DatePicker to the day where DeinUpdate started Posting
			dp.DisplayDateStart = new DateTime(2018, 03, 12);
			dp.DisplayDateEnd = LatestFridayRelease.AddYears(1);

			// Make it select the LatestFridayRelease
			dp.SelectedDate = LatestFridayRelease;


			// Make all non-fridays non selectable. Can probably improve this in terms of CPU
			// by using Ranges from Saturday to Thursday, instead of single Days
			// If I do that, gotta make sure I cover start and end edge-cases

			DateTime minDate = dp.DisplayDateStart ?? DateTime.MinValue;
			DateTime maxDate = dp.DisplayDateEnd ?? DateTime.MaxValue;

			for (DateTime dt = minDate; dt <= maxDate; dt = dt.AddDays(1))
			{
				if (dt.DayOfWeek != DayOfWeek.Friday)
				{
					dp.BlackoutDates.Add(new CalendarDateRange(dt));
				}
			}
		}


		private void btn_GetFromDate_Click(object sender, RoutedEventArgs e)
		{
			Scrape();
		}


		private async void Scrape(bool OnlyShowSource = false, bool OwnUrl = false)
		{
			DateTime myDT = (DateTime)dp.SelectedDate;
			btn_GetFromDate.Content += "...";

			List<string> ListOfLinks = new List<string>();

			if (OwnUrl)
			{
				Popups.PopupTextBox tmp = new Popups.PopupTextBox("Enter Link:", "Link here");
				tmp.ShowDialog();
				if (tmp.MyReturnString != "")
				{
					ListOfLinks.Add(tmp.MyReturnString);
				}
				else
				{
					btn_GetFromDate.Content = btn_GetFromDate.Content.ToString().TrimEnd('.').TrimEnd('.').TrimEnd('.');
					return;
				}
			}
			else
			{
				ListOfLinks = await Scraping.RSS_Scraper.GetLinksAsync(myDT);
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


					this.viewModel.MyAdd(ReleaseList);
				}
			}


			btn_GetFromDate.Content = btn_GetFromDate.Content.ToString().TrimEnd('.').TrimEnd('.').TrimEnd('.');
		}



		private void btn_GetFromDate_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			ContextMenu cm = new ContextMenu();

			MenuItem mi = new MenuItem();
			mi.Header = "Open Website Source in NotePad";
			mi.Click += MI_WebsiteSource_Click;
			cm.Items.Add(mi);

			MenuItem mi2 = new MenuItem();
			mi2.Header = "Custom URL to parse from";
			mi2.Click += MI_CustomURL_Click;
			cm.Items.Add(mi2);

			cm.IsOpen = true;
		}


		private void MI_WebsiteSource_Click(object sender, RoutedEventArgs e)
		{
			Scrape(true, false);
		}

		private void MI_CustomURL_Click(object sender, RoutedEventArgs e)
		{
			Scrape(false, true);
		}



		private void btn_Export_Click(object sender, RoutedEventArgs e)
		{
			DateTime myDT = (DateTime)dp.SelectedDate;
			string CurrFriday = myDT.ToString("dd.MM.yyyy");
			string LastSaturday = myDT.AddDays(-6).ToString("dd.MM.yyyy");

			IList<Helper.Release> myList;
			myList = this.viewModel.Releases;

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
					if (String.IsNullOrWhiteSpace(myRelease.Link))
					{
						if (String.IsNullOrWhiteSpace(myRelease.Info))
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| ");
						}
						else
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| " + myRelease.Info);
						}
					}
					else
					{
						if (String.IsNullOrWhiteSpace(myRelease.Info))
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| [Link](" + myRelease.Link + ")");
						}
						else
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| [Link](" + myRelease.Link + ") " + myRelease.Info);
						}
					}
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
					if (String.IsNullOrWhiteSpace(myRelease.Link))
					{
						if (String.IsNullOrWhiteSpace(myRelease.Info))
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| ");
						}
						else
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| " + myRelease.Info);
						}
					}
					else
					{
						if (String.IsNullOrWhiteSpace(myRelease.Info))
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| [Link](" + myRelease.Link + ")");
						}
						else
						{
							temp.Add(myRelease.Artist + "|" + myRelease.Title.Replace("[", @"\[").Replace("]", @"\]") + "| [Link](" + myRelease.Link + ") " + myRelease.Info);
						}
					}
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

		private void btn_About_Click(object sender, RoutedEventArgs e)
		{
			new Settings().ShowDialog();
		}

		private void btn_Exit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Button_LostFocus(object sender, RoutedEventArgs e)
		{
			MainWindow.MW.dg.CanUserAddRows = true;
		}

		private void Button_GotFocus(object sender, RoutedEventArgs e)
		{
			MainWindow.MW.dg.CanUserAddRows = false;
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove(); // Pre-Defined Method
		}

		private void dg_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3);
		}


		private void dp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			DateTime myDT = (DateTime)dp.SelectedDate;
			lbl_dg_Header.Content = "Die Releases vom: " + myDT.ToString("dd.MM.yyyy");
			MainWindow.MW.viewModel.Releases.Clear(); 
			MainWindow.MW.viewModel.MyAdd(Helper.CSVHelper.Read(Globals.CurrCSVFile));

		}

		private void btn_Clear_Click(object sender, RoutedEventArgs e)
		{
			Popups.Popup yesno = new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupYesNo, "Are you sure?");
			yesno.ShowDialog();
			if ((bool)yesno.DialogResult)
			{
				this.viewModel.Releases.Clear();
				Helper.CSVHelper.Save();
				Helper.FileHandling.deleteFile(Globals.CurrCSVFile);
			}
		}

		private void dg_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
		{
			Helper.CSVHelper.Save();
		}

		private void dg_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			Helper.CSVHelper.Save();
		}

		private void dg_CurrentCellChanged(object sender, EventArgs e)
		{
			Helper.CSVHelper.Save();
		}

		private void dg_AddingNewItem(object sender, AddingNewItemEventArgs e)
		{
			Helper.CSVHelper.Save();
		}

		private void dg_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
		{
			Helper.CSVHelper.Save();
		}

		private void dg_Sorting(object sender, DataGridSortingEventArgs e)
		{
			Helper.CSVHelper.Save();
		}

		private void btn_Save_Click(object sender, RoutedEventArgs e)
		{
			Helper.CSVHelper.Save(true);
		}

		public void SaveButtonVisibilityRefresh()
		{
			if (Settings.AutoSave)
			{
				MainWindow.MW.btn_Save.Visibility = Visibility.Hidden;
				MainWindow.MW.btn_Export.Margin = new Thickness(10, 10, 10, 10);
			}
			else
			{
				MainWindow.MW.btn_Save.Visibility = Visibility.Visible;
				MainWindow.MW.btn_Export.Margin = new Thickness(140, 10, 10, 10);
			}
		}
	}
}
