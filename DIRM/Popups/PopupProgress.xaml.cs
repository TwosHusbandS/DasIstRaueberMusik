
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.IO.Compression;
using System.Diagnostics;

namespace DIRM.Popups
{
	/// <summary>
	/// Interaction logic for CopyFileProgress.xaml
	/// This is responsible for ProgressBar on File copying (etc.) and Extracting the ZIP File
	/// </summary>
	public partial class PopupProgress : Window
	{


		private void Window_SourceInitialized(object sender, EventArgs e)
		{
			if (MainWindow.MW.IsVisible)
			{
				this.Left = MainWindow.MW.Left + (MainWindow.MW.Width / 2) - (this.Width / 2);
				this.Top = MainWindow.MW.Top + (MainWindow.MW.Height / 2) - (this.Height / 2);
			}
		}


		public enum ProgressTypes
		{
			ZIPFile,
			FileOperation,
		}

		/// <summary>
		/// ProgressType of Instance
		/// </summary>
		ProgressTypes ProgressType;

		/// <summary>
		/// Path of ZIP File (on disk) we are extracting
		/// </summary>
		string ZipFileWeWannaExtract;

		/// <summary>
		/// Path where ZIP File is extracted
		/// </summary>
		string myZipExtractionPath;




		/// <summary>
		/// List of File Operations
		/// </summary>
		List<Helper.MyFileOperation> MyFileOperations;

		/// <summary>
		/// List of File Operations
		/// </summary>
		public List<Helper.MyFileOperation> RtrnMyFileOperations;

		/// <summary>
		/// Using this to return a Bool
		/// </summary>
		public bool RtrnBool = false;

		/// <summary>
		/// Name of Operation for GUI
		/// </summary>
		string Operation;



		/// <summary>
		/// Constructor of PopupProgress.
		/// String pParam1 is either the GUI Text for File Operations or the ZIP File physical location
		/// </summary>
		/// <param name="UIText"></param>
		/// <param name="pMyFileOperations"></param>
		public PopupProgress(ProgressTypes pProgressType, string pParam1, List<Helper.MyFileOperation> pMyFileOperations = null, string ZipExtractionPath = "")
		{
			// Sorry you have to look at this spaghetti
			// Basically, based on the pProgressType the other params have different meanings or are not used etc. Kinda messy...really sucks

			if (MainWindow.MW.IsVisible)
			{
				this.Owner = MainWindow.MW;
				this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			if (MainWindow.MW.IsVisible)
			{
				this.Owner = MainWindow.MW;
				this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			}

			InitializeComponent();


			// Setting all Properties needed later
			ProgressType = pProgressType;

			if (ProgressType == ProgressTypes.FileOperation)
			{
				Operation = pParam1;
				MyFileOperations = pMyFileOperations;
				myLBL.Content = Operation + "...(0%)";
			}
			else if (ProgressType == ProgressTypes.ZIPFile)
			{
				ZipFileWeWannaExtract = pParam1;
				myLBL.Content = "Extracting ZIP...(0%)";
				myZipExtractionPath = ZipExtractionPath;
			}


			// Lets do some shit
			StartWork();
		}


		/// <summary>
		/// Starting the Task 
		/// </summary>
		[STAThread]
		public async void StartWork()
		{
			// Awaiting the Task of the Actual Work
			await Task.Run(new Action(ActualWork));

			// Close this
			this.Close();
		}

		/// <summary>
		/// Task of the actual work being done
		/// </summary>
		[STAThread]
		public void ActualWork()
		{

			//Basically just executing a list of MyFileOperations
			if (ProgressType == ProgressTypes.FileOperation)
			{
				double count = MyFileOperations.Count;
				double j = 0;

				Helper.Logger.Log("Lets do some File Operation Stuff");
				for (int i = 0; i <= MyFileOperations.Count - 1; i++)
				{
					Helper.MyFileOperation.Execute(MyFileOperations[i]);

					j++;
					this.Dispatcher.Invoke(() =>
					{
						myPB.Value = (int)(j / count * 100);
						myLBL.Content = Operation + "...(" + myPB.Value + "%)";
					});
				}
				Helper.Logger.Log("Done with File Operation Stuff");
			}
			else if (ProgressType == ProgressTypes.ZIPFile)
			{
				Helper.Logger.Log("ZipFileWeWannaExtract: '" + ZipFileWeWannaExtract + "'");
				Helper.Logger.Log("ZIPExtractPath: '" + myZipExtractionPath + "'");

				List<System.IO.Compression.ZipArchiveEntry> fileList = new List<System.IO.Compression.ZipArchiveEntry>();
				var totalFiles = 0;
				var filesExtracted = 0;

				if (!Helper.FileHandling.doesFileExist(ZipFileWeWannaExtract))
				{
					Helper.Logger.Log("ERROR. ZIP File we are extracting in Popup window doesnt exist..", true, 0);
					new Popup(Popup.PopupWindowTypes.PopupOk, "ERROR. ZIP File we are extracting in Popup window doesnt exist..").ShowDialog();
					return;
				}

				try
				{
					using (var archive = ZipFile.OpenRead(ZipFileWeWannaExtract))
					{
						totalFiles = archive.Entries.Count();

						// Looping through all Files in the ZIPFile
						foreach (var file in archive.Entries)
						{
							// If the File exists and is not a folder
							if (!string.IsNullOrEmpty(file.Name))
							{
								bool doExtract = true;
								string PathOnDisk = myZipExtractionPath.TrimEnd('\\') + @"\" + file.FullName.Replace(@"/", @"\");
								Helper.FileHandling.createPathOfFile(PathOnDisk); // 99% Chance I fixed this with the createZipPaths Method. Lets keep this to make sure...
								if (Helper.FileHandling.doesFileExist(PathOnDisk))
								{
									Helper.FileHandling.deleteFile(PathOnDisk);
								}

								if (doExtract)
								{
									file.ExtractToFile(PathOnDisk);
								}
							}

							// Update GUI
							Application.Current.Dispatcher.Invoke((Action)delegate
							{
								filesExtracted++;
								long progress = (100 * filesExtracted / totalFiles);
								myPB.Value = progress;
								myLBL.Content = "Extracting ZIP...(" + progress + "%)";
							});
						}
					}
				}
				catch (Exception e)
				{
					Helper.Logger.Log("TryCatch failed while extracting ZIP with progressbar." + e.ToString());
					new Popup(Popup.PopupWindowTypes.PopupOkError, "trycatch failed while extracting zip with progressbar\n" + e.ToString());
				}
			}
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

	}
}

