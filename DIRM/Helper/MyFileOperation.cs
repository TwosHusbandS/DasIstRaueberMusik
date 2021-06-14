using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DIRM.Helper
{


	/// <summary>
	/// Class of MyFileOperation
	/// </summary>
	public class MyFileOperation
	{
		/// <summary>
		/// Enum FileOperations
		/// </summary>
		public enum FileOperations
		{
			Copy,
			Move,
			Hardlink,
			Delete,
			Create
		}

		/// <summary>
		/// Enum FileOrFolder
		/// </summary>
		public enum FileOrFolder
		{
			File,
			Folder
		}

		/// <summary>
		/// Property of Class Instance,  FileOrFolder Type
		/// </summary>
		public FileOrFolder MyFileOrFolder { get; private set; }


		/// <summary>
		/// Property of Class Instance, File Operation Type
		/// </summary>
		public FileOperations FileOperation { get; private set; }

		/// <summary>
		/// Property of Class Instance. Name of File
		/// </summary>
		public string OriginalFile { get; private set; }

		/// <summary>
		/// Property of Class Instance. New File Location (for Moving, Copying, Hardlinking)
		/// </summary>
		public string NewFile { get; private set; }

		/// <summary>
		/// Property of Class Instance. The Log message which should appear
		/// </summary>
		public string Log { get; private set; }

		/// <summary>
		/// The loglevel for the LogMessage
		/// </summary>
		public int LogLevel { get; private set; }

		/// <summary>
		/// Constructor, creates one MyFileOperation Object
		/// </summary>
		/// <param name="pFileOperation"></param>
		/// <param name="pOriginalFile"></param>
		/// <param name="pNewFile"></param>
		/// <param name="pLog"></param>
		/// <param name="pLogLevel"></param>
		public MyFileOperation(FileOperations pFileOperation, string pOriginalFile, string pNewFile, string pLog, int pLogLevel, FileOrFolder pFileOrFolder = FileOrFolder.File)
		{
			FileOperation = pFileOperation;
			OriginalFile = pOriginalFile;
			NewFile = pNewFile;
			Log = pLog;
			LogLevel = pLogLevel;
			MyFileOrFolder = pFileOrFolder;
		}

		/// <summary>
		/// Executes one MyFileOperationObject
		/// </summary>
		/// <param name="pMyFileOperation"></param>
		public static void Execute(MyFileOperation pMyFileOperation)
		{
			switch (pMyFileOperation.FileOperation)
			{
				case FileOperations.Copy:
					{
						Helper.Logger.Log(pMyFileOperation.Log, pMyFileOperation.LogLevel);
						if (pMyFileOperation.MyFileOrFolder == FileOrFolder.File)
						{
							Helper.FileHandling.copyFile(pMyFileOperation.OriginalFile, pMyFileOperation.NewFile);
						}
						else
						{
							Helper.FileHandling.CopyPath(pMyFileOperation.OriginalFile, pMyFileOperation.NewFile);
						}
						break;
					}
				case FileOperations.Create:
					{
						Helper.Logger.Log(pMyFileOperation.Log, pMyFileOperation.LogLevel);
						if (pMyFileOperation.MyFileOrFolder == FileOrFolder.File)
						{
							Helper.FileHandling.createFile(pMyFileOperation.OriginalFile);
						}
						else
						{
							Helper.FileHandling.createPath(pMyFileOperation.OriginalFile);
						}
						break;
					}
				case FileOperations.Move:
					{
						Helper.Logger.Log(pMyFileOperation.Log, pMyFileOperation.LogLevel);
						if (pMyFileOperation.MyFileOrFolder == FileOrFolder.File)
						{
							Helper.FileHandling.moveFile(pMyFileOperation.OriginalFile, pMyFileOperation.NewFile);
						}
						else
						{
							Helper.FileHandling.movePath(pMyFileOperation.OriginalFile, pMyFileOperation.NewFile);
						}
						break;
					}
				case FileOperations.Hardlink:
					{
						if (pMyFileOperation.MyFileOrFolder == FileOrFolder.File)
						{
							Helper.Logger.Log(pMyFileOperation.Log, pMyFileOperation.LogLevel);
							Helper.FileHandling.HardLinkFiles(pMyFileOperation.NewFile, pMyFileOperation.OriginalFile);
						}
						else
						{
							new Popups.Popup(Popups.Popup.PopupWindowTypes.PopupOkError, "No idea what happened here...MyFileOperation Execute Hardlink Folder");
						}
						break;
					}
				case FileOperations.Delete:
					{
						Helper.Logger.Log(pMyFileOperation.Log, pMyFileOperation.LogLevel);
						if (pMyFileOperation.MyFileOrFolder == FileOrFolder.File)
						{
							Helper.FileHandling.deleteFile(pMyFileOperation.OriginalFile);
						}
						else
						{
							Helper.FileHandling.DeleteFolder(pMyFileOperation.OriginalFile);
						}
						break;
					}
				default: break;
			}
		}

	}
}
