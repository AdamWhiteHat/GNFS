using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GNFS_Winforms
{
	public static class Logging
	{
		public static string OutputFilename;
		public static TextBox OutputTextbox;
		public static bool FirstFindRelations;
		private static readonly string DefaultLoggingFilename = "Output.log.txt";
		
		static Logging()
		{
			FirstFindRelations = false;
			OutputFilename = Settings.Log_FileName ?? DefaultLoggingFilename;
			CreateLogFileIfNotExists();
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
			CreateLogFileIfNotExists();
			File.AppendAllText(OutputFilename, Environment.NewLine);
			LogTextbox(Environment.NewLine);
		}

		public static void LogMessage(string message, params object[] args)
		{
			string msg = args.Any() ? string.Format(message, args) : string.IsNullOrWhiteSpace(message) ? "(empty)" : message;
			if (!string.IsNullOrWhiteSpace(msg))
			{
				CreateLogFileIfNotExists();
				File.AppendAllText(OutputFilename, msg + Environment.NewLine);
				LogTextbox(msg);
			}
		}

		public static void LogTextbox(string message)
		{
			if (GNFSCore.DirectoryLocations.IsLinuxOS())
			{
				return;
			}
			if (OutputTextbox.InvokeRequired)
			{
				OutputTextbox.Invoke(new MethodInvoker(() => LogTextbox(message)));
			}
			else
			{
				OutputTextbox.AppendText(message);
			}
		}

		public static void CreateLogFileIfNotExists()
		{
			string directory = Path.GetDirectoryName(OutputFilename);

			if (!Directory.Exists(directory))
			{
				FirstFindRelations = true;
				if (File.Exists(OutputFilename))
				{
					File.Delete(OutputFilename);
				}
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