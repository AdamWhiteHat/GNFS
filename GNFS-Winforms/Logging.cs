using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GNFS_Winforms
{
	public static class Logging
	{
		public static MainForm PrimaryForm;
		public static TextBox OutputTextbox;
		public static bool FirstFindRelations = false;
		public static string OutputFilename = Path.GetFullPath(Settings.Log_FileName ?? DefaultLoggingFilename);
		public static string ExceptionLogFilename = Path.GetFullPath(DefaultExceptionLogFilename);


		private static int MaxLines = 200;
		private const string DefaultLoggingFilename = "Output.log.txt";
		private const string DefaultExceptionLogFilename = "Exceptions.log.txt";

		public static bool IsDebugMode()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}

		public static void LogMessage()
		{
			LogMessage(string.Empty);
		}

		public static void LogMessage(string message, params object[] args)
		{
			LogMessage(args.Any() ? string.Format(message, args) : string.IsNullOrWhiteSpace(message) ? "(empty)" : message);
		}

		public static void LogMessage(string message)
		{
			string toLog = message + Environment.NewLine;
			CreateLogFileIfNotExists(OutputFilename);
			File.AppendAllText(OutputFilename, GetTimestamp() + toLog);
			LogTextbox(toLog);
		}

		public static void LogException(Exception ex, string message)
		{
			string toLog = (ex == null) ? Environment.NewLine + "Application encountered an error" : ex.ToString();

			if (!string.IsNullOrWhiteSpace(message))
				toLog += ": " + message;
			else
				toLog += "!";

			toLog += Environment.NewLine + Environment.NewLine;


			CreateLogFileIfNotExists(OutputFilename);
			File.AppendAllText(OutputFilename, GetTimestamp() + toLog);
			LogTextbox(toLog);
		}

		public static void LogTextbox(string message)
		{
			//if (GNFSCore.DirectoryLocations.IsLinuxOS())
			//{
			//	return;
			//}

			if (OutputTextbox.IsDisposed || !OutputTextbox.IsHandleCreated)
			{
				throw new Exception();
			}

			if (OutputTextbox.InvokeRequired /* && !GNFSCore.DirectoryLocations.IsLinuxOS()*/)
			{
				OutputTextbox.Invoke(new Action(() => { LogTextbox(message); }));
			}
			else
			{
				string toLog = message;
				if (PrimaryForm.IsWorking)
				{
					toLog = "\t" + message;
				}

				if (OutputTextbox.Lines.Length > MaxLines)
				{
					OutputTextbox.Clear();
				}
				OutputTextbox.AppendText(toLog);
			}
		}

		public static void CreateLogFileIfNotExists(string file)
		{
			string directory = Path.GetDirectoryName(file);
			if (!Directory.Exists(directory))
			{
				FirstFindRelations = true;
				Directory.CreateDirectory(directory);
			}
			if (!File.Exists(file))
			{
				string logHeader = $"Log created: {DateTime.Now}";
				string line = new string(Enumerable.Repeat('-', logHeader.Length).ToArray());

				File.WriteAllLines(file, new string[] { logHeader, line, Environment.NewLine });
			}
		}

		public static string GetTimestamp()
		{
			DateTime now = DateTime.Now;
			return $"[{now.DayOfYear}.{now.Year} @ {now.ToString("HH:mm:ss")}]  ";
		}
	}
}