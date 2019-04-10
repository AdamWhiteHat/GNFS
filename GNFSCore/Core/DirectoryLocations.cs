using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public class DirectoryLocations
	{
		private string _saveDirectory = null;
		private const int showDigits = 22;
		private const string elipse = "[...]";
		private const string _parameters = "GNFS.json";
		private static readonly string saveRootDirectory = IsLinuxOS() ? "GNFS" : "C:\\GNFS";

		public string SaveDirectory { get { return _saveDirectory; } }
		public string GnfsParameters_SaveFile { get { return Path.Combine(SaveDirectory, _parameters); } }

		public DirectoryLocations(string baseDirectory)
		{
			_saveDirectory = baseDirectory;
		}

		public DirectoryLocations(BigInteger n)
		{
			_saveDirectory = GetSaveLocation(n);
		}

		public DirectoryLocations(BigInteger n, BigInteger polynomialBase, BigInteger polynomialDegree)
		{
			_saveDirectory = GetSaveLocation(n);
		}

		public static bool IsLinuxOS()
		{
			int p = (int)Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128); // 128 comes from mono run-times
		}

		public static string GetSaveLocation(BigInteger n)
		{
			string directoryName = GetUniqueNameFromN(n);
			return Path.Combine(saveRootDirectory, directoryName);
		}

		public static string GetUniqueNameFromN(BigInteger n)
		{
			string result = n.ToString();

			if (result.Length >= (showDigits * 2) + elipse.Length)
			{
				result = result.Substring(0, showDigits) + elipse + result.Substring(result.Length - showDigits, showDigits);
			}

			return result;
		}
	}
}
