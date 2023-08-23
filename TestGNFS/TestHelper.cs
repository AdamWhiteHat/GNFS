using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGNFS.Integration
{
	public static class TestHelper
	{
		private static string _saveLocation = null;

		public static string GetTestSaveLocation()
		{
			if (_saveLocation == null)
			{
				_saveLocation = Path.Combine(TestContext.CurrentContext.WorkDirectory, "UnitTestData");
				//var abs = Path.GetFullPath(@".\UnitTestData");
				if (!Directory.Exists(_saveLocation))
				{
					Directory.CreateDirectory(_saveLocation);
				}
			}
			return _saveLocation;
		}

		public static void RecursiveDelete(string path)
		{
			string fullPath = Path.GetFullPath(path);

			if (Path.GetPathRoot(fullPath) == fullPath
				|| !fullPath.Contains("GNFS")
				|| fullPath.Count(c => c == Path.DirectorySeparatorChar) < 3)
			{
				return; // Prevent deleting C:\ or some other shallow path accidentally
			}

			IEnumerable<string> files = Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				File.Delete(file);
			}

			IEnumerable<string> directories = Directory.EnumerateDirectories(fullPath, "*", SearchOption.AllDirectories);
			foreach (string directory in directories)
			{
				RecursiveDelete(directory);
			}

			Directory.Delete(fullPath);
		}
	}
}
