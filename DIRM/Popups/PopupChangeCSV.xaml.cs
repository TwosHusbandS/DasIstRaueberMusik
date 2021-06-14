using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DIRM.Popups
{
	/// <summary>
	/// Interaction logic for PopupTextBox.xaml
	/// </summary>
	public partial class PopupChangeCSV : Window
	{

		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			if (MainWindow.MW.IsVisible)
			{
				this.Left = MainWindow.MW.Left + (MainWindow.MW.Width / 2) - (this.Width / 2);
				this.Top = MainWindow.MW.Top + (MainWindow.MW.Height / 2) - (this.Height / 2);
			}
		}



		public bool CopyOldFiles = true;

		public bool DeleteOldFiles = false;

		public string NewCSVFilePath = "";

		/// <summary>
		/// Constructor of PopupChangeCSV
		/// </summary>
		public PopupChangeCSV()
		{
			if (MainWindow.MW.IsVisible)
			{
				this.Owner = MainWindow.MW;
				this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			InitializeComponent();

			cb_ChangeCSV_CopyOldFiles.IsChecked = CopyOldFiles;
			cb_ChangeCSV_DeleteOldFiles.IsChecked = DeleteOldFiles;

			lbl_CurrCSVLocation.Content = Settings.CSVPath;
			lbl_CurrCSVLocation.ToolTip = Settings.CSVPath;

			btn_Yes.Focus();
		}


		/// <summary>
		/// Click on "Yes". Sets DialogResult to "Yes" and closes itself.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btn_Yes_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close();
		}


		/// <summary>
		/// Click on "No". Sets DialogResult to "No" and closes itself.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btn_No_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			this.Close();

		}


		/// <summary>
		/// Method which makes the Window draggable, which moves the whole window when holding down Mouse1 on the background
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove(); // Pre-Defined Method
		}

		private void cb_ChangeCSV_CopyOldFiles_Click(object sender, RoutedEventArgs e)
		{
			CopyOldFiles = (bool)cb_ChangeCSV_CopyOldFiles.IsChecked;
		}

		private void cb_ChangeCSV_DeleteOldFiles_Click(object sender, RoutedEventArgs e)
		{
			DeleteOldFiles = (bool)cb_ChangeCSV_DeleteOldFiles.IsChecked;
		}

		private void btn_Settings_SavePath_Click(object sender, RoutedEventArgs e)
		{
			// Grabbing the new Path from FolderDialogThingy
			string StartUpPathCSV = Settings.CSVPath;
			if (String.IsNullOrWhiteSpace(StartUpPathCSV))
			{
				StartUpPathCSV = @"C:\";
			}
			else
			{
				StartUpPathCSV = Helper.FileHandling.PathSplitUp(StartUpPathCSV.TrimEnd('\\'))[0];
			}
			NewCSVFilePath = Helper.FileHandling.OpenDialogExplorer(Helper.FileHandling.PathDialogType.Folder, "Pick the Folder where this Program will store its Data.", StartUpPathCSV);

			if (!String.IsNullOrEmpty(NewCSVFilePath))
			{
				btn_Settings_SavePath.Content = NewCSVFilePath;
				btn_Settings_SavePath.ToolTip = NewCSVFilePath;
			}
		}
	}
}
