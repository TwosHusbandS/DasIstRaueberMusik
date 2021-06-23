using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;

namespace DIRM.Helper
{
	/// <summary>
	/// Class of our own Logger
	/// </summary>
	public static class Logger
	{
		// We should probably use a logging libary / framework now that I think about it...whatevs
		// Actually implementing this probably took less time than googling "Logging class c#", and we have more control over it

		private static Mutex mut = new Mutex();

		/// <summary>
		/// Init Function which gets called once at the start.
		/// </summary>
		public static void Init()
		{
			// Since the createFile Method will override an existing file
			if (!FileHandling.doesFileExist(Globals.Logfile))
			{
				Helper.FileHandling.createFile(Globals.Logfile);
			}



			string MyCreationDate = Helper.FileHandling.GetCreationDate(Process.GetCurrentProcess().MainModule.FileName).ToString("yyyy-MM-ddTHH:mm:ss");

			Helper.Logger.Log("-", true, 0);
			Helper.Logger.Log("-", true, 0);
			Helper.Logger.Log("-", true, 0);
			Helper.Logger.Log(" === DIRM Started (Version: '" + Globals.ProjectVersion + "' BuildInfo: '" + Globals.BuildInfo + "' Built at: '" + MyCreationDate + "' Central European Time) ===", true, 0);
			Helper.Logger.Log("    Time Now: '" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "'", true, 0);
			Helper.Logger.Log("    Time Now UTC: '" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + "'", true, 0);
			Helper.Logger.Log("Logging initiated. Time to the left is local time and NOT UTC. See Debug for more Info", true, 0);
		}

		/// <summary>
		/// Main Method of Logging.cs which is called to log stuff.
		/// </summary>
		/// <param name="pLogMessage"></param>
		public static void Log(string pLogMessage, bool pSkipLogSetting, int pLogLevel)
		{
			mut.WaitOne();
			if (pSkipLogSetting && !String.IsNullOrWhiteSpace(pLogMessage))
			{
				string LogMessage = "[" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "] - ";

				// Yes this for loop is correct. If Log level 0, we dont add another "- "
				for (int i = 0; i <= pLogLevel - 1; i++)
				{
					LogMessage += "- ";
				}

				LogMessage += pLogMessage;
				Helper.FileHandling.AddToLog(Globals.Logfile, LogMessage);
			}
			mut.ReleaseMutex();
		}

		/// <summary>
		/// Overloaded / Underloaded Logging Method
		/// </summary>
		/// <param name="pLogMessage"></param>
		public static void Log(string pLogMessage)
		{
			Log(pLogMessage, true, 0);
		}

		/// <summary>
		/// Overloaded / Underloaded Logging Method
		/// </summary>
		/// <param name="pLogMessage"></param>
		public static void Log(Exception ex)
		{
			Log("!!! Exception found !!!");
			Log("-----------------------");
			Log("ex.ToString()");
			Log(ex.ToString());
			Log("-----------------------");
			Log("ex.Message.ToString()");
			Log(ex.Message.ToString());
			Log("-----------------------");
			try
			{
				StackTrace st = new StackTrace(ex, true);
				StackFrame frame = st.GetFrame(0);
				string fileName = frame.GetFileName();
				string methodName = frame.GetMethod().Name;
				int line = frame.GetFileLineNumber();
				int col = frame.GetFileColumnNumber();
				Log("Stacktrace Info");
				Log("FileName: \"" + fileName + "\"");
				Log("MethodName: \"" + methodName + "\"");
				Log("Line: \"" + line.ToString() + "\"");
				Log("Column: \"" + col.ToString() + "\"");
				Log("-----------------------");
			}
			catch
			{
				Log("Actually failed getting StackTrace Information. 110 of Logger");
				Log("-----------------------");
			}
		}

		/// <summary>
		/// Overloaded / Underloaded Logging Method
		/// </summary>
		/// <param name="pLogMessage"></param>
		/// <param name="pLogLevel"></param>
		public static void Log(string pLogMessage, int pLogLevel)
		{
			Log(pLogMessage, true, pLogLevel);
		}

		/// <summary>
		/// Rolling Log. Gets called on P127 start. Only keeps the latest 2500 lines, everything before that will get deleted
		/// </summary>
		public static void RollingLog()
		{
			string[] Logs = Helper.FileHandling.ReadFileEachLine(Globals.Logfile);
			if (Logs.Length > 2500)
			{
				List<string> myNewLog = new List<string>();
				int i = Logs.Length - 2490;
				while (i <= Logs.Length - 1)
				{
					myNewLog.Add(Logs[i]);
					i++;
				}
				string[] tmp = myNewLog.ToArray();
				Helper.FileHandling.WriteStringToFileOverwrite(Globals.Logfile, tmp);
			}
		}



	} // End of Class
} // End of NameSpace
