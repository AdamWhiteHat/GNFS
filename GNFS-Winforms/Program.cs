using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GNFS_Winforms
{
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.ThreadException += Application_ThreadException;
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			Application.Run(new MainForm());
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Logging.LogException((Exception)e.ExceptionObject, "CAUGHT UNHANDLED _APPLICATION_ EXCEPTION");
			}
			catch
			{
			}
		}

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			try
			{
				Logging.LogException(e.Exception, "CAUGHT UNHANDLED _THREAD_ EXCEPTION");
			}
			catch
			{
			}
		}

		public static bool IsDebug()
		{
			bool result = false;

#if DEBUG
			result = true;
#endif

			return result;
		}
	}
}
