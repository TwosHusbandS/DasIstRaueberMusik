using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;

namespace DIRM
{
	public partial class MainWindow : Window
	{
		public ViewModel viewModel;



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

				// Switch Value
				switch (value)
				{
					// In Case: Settings
					case PageStates.Main:
						MainWindow.MW.Grid_DG.Visibility = Visibility.Visible;
						MainWindow.MW.Grid_Page.Visibility = Visibility.Hidden;
						MainWindow.MW.btn_About.Style = Application.Current.FindResource("btn_hamburgeritem") as Style;
						break;
					case PageStates.AboutSettings:
						MainWindow.MW.Grid_Page.Visibility = Visibility.Visible;
						MainWindow.MW.Grid_DG.Visibility = Visibility.Hidden;
						MainWindow.MW.Frame_Main.Content = new Settings();
						MainWindow.MW.btn_About.Style = Application.Current.FindResource("btn_hamburgeritem_selected") as Style;
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

				if (value == HamburgerMenuStates.Visible)
				{
					MainWindow.MW.Grid_Main.ColumnDefinitions[0].Width = new GridLength(300);
				}
				// If is not visible
				else
				{
					MainWindow.MW.Grid_Main.ColumnDefinitions[0].Width = new GridLength(0);
				}
			}
		}







		public void InitUI()
		{
			this.viewModel = new ViewModel
			{
				Releases = new ObservableCollection<Helper.Release>()
				{

				}
			};

			this.DataContext = this.viewModel;

			InitDatePicker();

			SetSaveButtonVisibilityBasedOnSettings();
			SetButtonMouseOverMagic(btn_Exit);
			SetButtonMouseOverMagic(btn_Hamburger);
			SetWindowBackgroundImage();
			dg.RowBackground = MyColors.MyColorOffBlack70;
			dg.AlternatingRowBackground = MyColors.MyColorOffBlack50;
			dg.Foreground = MyColors.MyColorWhite;


			Grid_Message.Visibility = Visibility.Hidden;
		}



		public void SetSaveButtonVisibilityBasedOnSettings()
		{
			if (Settings.AutoSave)
			{
				btn_Save.Visibility = Visibility.Hidden;
			}
			else
			{
				btn_Save.Visibility = Visibility.Visible;
			}
		}


		private void btn_Okay_Click(object sender, RoutedEventArgs e)
		{
			Grid_Message.Visibility = Visibility.Hidden;
		}


		public void SetUpAnnoucement(string msg)
		{
			SetUpAnnoucement(new string[] { msg });
		}


		public void SetUpAnnoucement(string[] msg)
		{
			rtb_Message.Document.Blocks.Clear();
			AddParagraph(rtb_Message, "");
			rtb_Message.Document.Blocks.Remove(rtb_Message.Document.Blocks.FirstBlock);

			for (int i = 0; i <= msg.Length - 1; i++)
			{
				AddParagraph(rtb_Message, msg[i]);
			}

			AddParagraph(rtb_Message, "");
			Grid_Message.Visibility = Visibility.Visible;
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


		private void SetButtonMouseOverMagic(Button myBtn)
		{
			switch (myBtn.Name)
			{
				case "btn_Hamburger":
					if (myBtn.IsMouseOver)
					{
						SetControlBackground(myBtn, @"Artwork\hamburger_mo.png");
					}
					else
					{
						SetControlBackground(myBtn, @"Artwork\hamburger.png");
					}
					break;
				case "btn_Exit":
					if (myBtn.IsMouseOver)
					{
						SetControlBackground(myBtn, @"Artwork\exit_mo.png");
					}
					else
					{
						SetControlBackground(myBtn, @"Artwork\exit.png");
					}
					break;
			}
		}

		private void btn_MouseLeave(object sender, MouseEventArgs e)
		{
			SetButtonMouseOverMagic((Button)sender);
		}

		private void btn_MouseEnter(object sender, MouseEventArgs e)
		{
			SetButtonMouseOverMagic((Button)sender);
		}

		private void Frame_Main_Navigating(object sender, NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Forward)
			{
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Adding a Paragraph to a RichTextBox
		/// </summary>
		/// <param name="rtb"></param>
		/// <param name="Paragraph"></param>
		public static void AddParagraph(RichTextBox rtb, string MyParagraph)
		{
			Paragraph para = new Paragraph();
			para.Margin = new Thickness(10);
			para.Inlines.Add(MyParagraph);
			para.TextAlignment = TextAlignment.Center;
			rtb.Document.Blocks.Add(para);
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



		/// <summary>
		/// Set the Background to our WPF Window
		/// </summary>
		public void SetWindowBackgroundImage()
		{
			string URL_Path = @"Artwork\bg.png";
			Uri resourceUri = new Uri(URL_Path, UriKind.Relative);
			StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
			BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
			ImageBrush brush = new ImageBrush();
			brush.ImageSource = temp;
			MainWindow.MW.GridBackground.Background = brush;
		}

		/// <summary>
		/// Sets the Backgrund of a specific Button
		/// </summary>
		/// <param name="myCtrl"></param>
		/// <param name="pArtpath"></param>
		public void SetControlBackground(Control myCtrl, string pArtpath, bool FromFile = false)
		{
			try
			{
				Uri resourceUri;
				if (FromFile)
				{
					var bitmap = new BitmapImage();

					using (var stream = new FileStream(pArtpath, FileMode.Open))
					{
						bitmap.BeginInit();
						bitmap.CacheOption = BitmapCacheOption.OnLoad;
						bitmap.StreamSource = stream;
						bitmap.EndInit();
						bitmap.Freeze(); // optional
					}
					ImageBrush brush2 = new ImageBrush();
					brush2.ImageSource = bitmap;
					myCtrl.Background = brush2;
				}
				else
				{
					resourceUri = new Uri(pArtpath, UriKind.Relative);
					StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);
					BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
					ImageBrush brush = new ImageBrush();
					brush.ImageSource = temp;
					myCtrl.Background = brush;
				}
			}
			catch
			{
				Helper.Logger.Log("Failed to set Background Image for Button");
			}
		}



	}
}
