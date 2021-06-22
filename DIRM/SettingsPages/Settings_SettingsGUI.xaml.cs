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

namespace DIRM.SettingsPages
{
	/// <summary>
	/// Interaction logic for Settings_SettingsGUI.xaml
	/// </summary>
	public partial class Settings_SettingsGUI : Page
	{
		public Settings_SettingsGUI()
		{
			InitializeComponent();
			RefreshUI();
		}



		void RefreshUI()
		{
			cb_Settings_AlwaysExportAlphabetically.IsChecked = Settings.AlwaysExportAlphabetically;
			cb_Settings_AutoSave.IsChecked = Settings.AutoSave;
			btn_Settings_SavePath.Content = Settings.CSVPath;
			btn_Settings_SavePath.ToolTip = Settings.CSVPath;
		}



		private void btn_Settings_SavePath_Click(object sender, RoutedEventArgs e)
		{
			bool DoWeCopyOldFiles = true;
			bool DoWeDeleteOldFiles = false;
			string PickedCSVFolder = "";

			Popups.PopupChangeCSV PCSV = new Popups.PopupChangeCSV();
			PCSV.ShowDialog();
			if (PCSV.DialogResult == true)
			{
				DoWeCopyOldFiles = PCSV.CopyOldFiles;
				DoWeDeleteOldFiles = PCSV.DeleteOldFiles;
				PickedCSVFolder = PCSV.NewCSVFilePath;
			}
			else
			{
				return;
			}


			// If its a valid Path (no "") and if its a new Path
			if (ChangeCSVSavePath(PickedCSVFolder, DoWeCopyOldFiles, DoWeDeleteOldFiles))
			{
				Helper.Logger.Log("Changing CSV Path worked");
			}
			else
			{
				Helper.Logger.Log("Changing CSV Path did not work. Probably non existing Path or same Path as before");
				new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupOk, "Changing CSV Path did not work. Probably non existing Path or same Path as before");
			}

			RefreshUI();
		}



		/// <summary>
		/// Method which gets called when changing the Path of the ZIP Extraction
		/// </summary>
		/// <param name="pNewCSVSavePath"></param>
		/// <returns></returns>
		public static bool ChangeCSVSavePath(string pNewCSVSavePath, bool CopyOldFiles, bool DeleteOldFiles)
		{
			Helper.Logger.Log("Called Method ChangeCSVSavePath");

			// THROW UI AROUND

			if (Helper.FileHandling.doesPathExist(pNewCSVSavePath) && pNewCSVSavePath.TrimEnd('\\') != Settings.CSVPath.TrimEnd('\\'))
			{
				Helper.Logger.Log("Potential New CSVSavePath exists and is new, lets continue");

				string OldCSVSubFolderPath = Globals.CSVSubfolderPath;

				if (CopyOldFiles)
				{
					Helper.Logger.Log("We want to Copy Old Files.");

					// List of File Operations for the ZIP Move progress
					List<Helper.MyFileOperation> MyFileOperations = new List<Helper.MyFileOperation>();

					// List of FileNames
					string[] FilesInOldCSVPath = Helper.FileHandling.GetFilesFromFolderAndSubFolder(OldCSVSubFolderPath);
					string[] FilesInNewCSVPath = new string[FilesInOldCSVPath.Length];

					// Loop through all Files there
					for (int i = 0; i <= FilesInOldCSVPath.Length - 1; i++)
					{
						// Build new Path of each File
						FilesInNewCSVPath[i] = pNewCSVSavePath.TrimEnd('\\') + @"\DasIstRaueberMusik" + @"\" + FilesInOldCSVPath[i].Substring((OldCSVSubFolderPath.TrimEnd('\\') + @"\").Length);

						// Add File Operation for that new File
						MyFileOperations.Add(new Helper.MyFileOperation(Helper.MyFileOperation.FileOperations.Copy, FilesInOldCSVPath[i], FilesInNewCSVPath[i], "Copying File '" + FilesInOldCSVPath[i] + "' to Location '" + FilesInNewCSVPath[i] + "' while moving ZIP Files", 0));
					}

					// Execute all File Operations
					Helper.Logger.Log("About to copy all relevant Files (" + MyFileOperations.Count + ")");
					new Popups.PopupProgress(Popups.PopupProgress.ProgressTypes.FileOperation, "Copying CSV File Location", MyFileOperations).ShowDialog();
					Helper.Logger.Log("Done with copying all relevant Files");
				}
				else
				{
					Helper.Logger.Log("We dont want to Copy Old Files.");
				}


				// Actually changing the Settings here
				Settings.CSVPath = pNewCSVSavePath;
				Globals.CheckIfCSVPathExistsOrCanBeCreated();

				if (DeleteOldFiles)
				{
					Helper.Logger.Log("We want to Delete Old Files.");
					Helper.FileHandling.DeleteFolder(OldCSVSubFolderPath);
				}
				else
				{
					Helper.Logger.Log("We dont want to Delete Old Files.");

				}

				return true;
			}
			else
			{
				Helper.Logger.Log("Potential New CSVSavePath does not exist or is the same as the old");
				return false;
			}
		}




		private void cb_Settings_AutoSave_Click(object sender, RoutedEventArgs e)
		{
			Settings.AutoSave = (bool)cb_Settings_AutoSave.IsChecked;
			RefreshUI();
		}

		private void cb_Settings_AlwaysExportAlphabetically_Click(object sender, RoutedEventArgs e)
		{
			Settings.AlwaysExportAlphabetically = (bool)cb_Settings_AlwaysExportAlphabetically.IsChecked;
			RefreshUI();
		}

		private void btn_Settings_SavePath_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			Process.Start("explorer.exe", Globals.CSVSubfolderPath);
		}

		private void btn_Settings_ExportCSV_Click(object sender, RoutedEventArgs e)
		{
			FileInfo file = new FileInfo("DasIstRaueberMusikExport.zip");

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "ZIP Files|*.zip*";
			saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			saveFileDialog.FileName = file.Name;
			saveFileDialog.DefaultExt = file.Extension;
			saveFileDialog.AddExtension = true;
			saveFileDialog.Title = "Save all Release Dates as big ZIP File";

			if (saveFileDialog.ShowDialog() == true)
			{
				if (Helper.FileHandling.doesFileExist(saveFileDialog.FileName))
				{
					Helper.FileHandling.deleteFile(saveFileDialog.FileName);
				}
				ZipFile.CreateFromDirectory(Globals.CSVSubfolderPath, saveFileDialog.FileName);
			}
		}

		private void btn_Settings_ImportCSV_Click(object sender, RoutedEventArgs e)
		{
			string ZIPFilePathChosenByUserToImport = Helper.FileHandling.OpenDialogExplorer(Helper.FileHandling.PathDialogType.File, "Select the ZIP File you want to import", @"C:\", false, "ZIP Files|*.zip*");

			if (Helper.FileHandling.doesFileExist(ZIPFilePathChosenByUserToImport))
			{
				string tempFolderPath = Globals.CSVSubfolderPath.TrimEnd('\\') + @"\temp";
				Helper.FileHandling.DeleteFolder(tempFolderPath);
				Helper.FileHandling.createPath(tempFolderPath);

				Popups.PopupProgress Pop = new Popups.PopupProgress(Popups.PopupProgress.ProgressTypes.ZIPFile, ZIPFilePathChosenByUserToImport, null, tempFolderPath);
				Pop.ShowDialog();

				foreach (string myFilePath in Helper.FileHandling.GetFilesFromFolder(tempFolderPath))
				{
					string fileName = Helper.FileHandling.PathSplitUp(myFilePath)[1];
					string thatFilePathInOriginalCSVSubFolder = Helper.FileHandling.PathCombine(Globals.CSVSubfolderPath, fileName);

					IList<Helper.Release> NewImported = Helper.CSVHelper.Read(myFilePath);

					if (Helper.FileHandling.doesFileExist(thatFilePathInOriginalCSVSubFolder))
					{
						IList<Helper.Release> AlreadyExisting = Helper.CSVHelper.Read(thatFilePathInOriginalCSVSubFolder);
						NewImported = MainWindow.MW.viewModel.MyAdd(AlreadyExisting, NewImported);
					}

					Helper.CSVHelper.Save(thatFilePathInOriginalCSVSubFolder, NewImported, true);
				}
				Helper.FileHandling.DeleteFolder(tempFolderPath);
			}
		}

		private void btn_OpenLog_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("notepad.exe", Globals.Logfile);
		}


	}
}
