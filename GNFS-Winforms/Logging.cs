using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GNFS_Winforms
{
	public static class Logging
	{
		public static string OutputFilename;
		public static MainForm PrimaryForm;
		public static TextBox OutputTextbox;
		public static bool FirstFindRelations;

		private static int MaxLines = 200;
		private static readonly string DefaultLoggingFilename = "Output.log.txt";

		static Logging()
		{
			FirstFindRelations = false;
			OutputFilename = Settings.Log_FileName ?? DefaultLoggingFilename;
			OutputFilename = Path.GetFullPath(OutputFilename);
		}

		public static bool IsDebugMode()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}

		public static void LogException(Exception ex, string message, params object[] args)
		{
			string msg = args.Any() ? string.Format(message, args) : string.IsNullOrWhiteSpace(message) ? "(empty)" : message;
			LogMessage("{0} : {1}", msg, ex == null ? "(null)" : ex.ToString());
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
			CreateLogFileIfNotExists();
			File.AppendAllText(OutputFilename, toLog);
			LogTextbox(toLog);
		}

		public static void LogTextbox(string message)
		{
			//if (GNFSCore.DirectoryLocations.IsLinuxOS())
			//{
			//	return;
			//}

			if (!OutputTextbox.IsHandleCreated || OutputTextbox.IsDisposed)
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

		public static void CreateLogFileIfNotExists()
		{
			string directory = Path.GetDirectoryName(OutputFilename);
			if (!Directory.Exists(directory))
			{
				FirstFindRelations = true;
				Directory.CreateDirectory(directory);
			}
			if (!File.Exists(OutputFilename))
			{
				string logHeader = $"Log created: {DateTime.Now}";
				string line = new string(Enumerable.Repeat('-', logHeader.Length).ToArray());

				File.WriteAllLines(OutputFilename, new string[] { logHeader, line, Environment.NewLine });
			}
		}
	}
}