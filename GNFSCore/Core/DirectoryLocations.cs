using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public class DirectoryLocations
	{
		private string saveDirectory = null;
		private static string baseDirectory = "GNFS";

		private const int showDigits = 22;
		private const string elipse = "[...]";
		private const string parametersFilename = "GNFS.json";

		public string SaveDirectory { get { return saveDirectory; } }
		public string GnfsParameters_SaveFile { get { return Path.Combine(SaveDirectory, parametersFilename); } }

		public DirectoryLocations(string saveLocation)
		{
			saveDirectory = saveLocation;
		}

		public DirectoryLocations(BigInteger n)
		{
			saveDirectory = GetSaveLocation(n);
		}

		public DirectoryLocations(BigInteger n, BigInteger polynomialBase, BigInteger polynomialDegree)
		{
			saveDirectory = GetSaveLocation(n);
		}

		public static void SetBaseDirectory(string path)
		{
			baseDirectory = path;
		}

		public static string GetSaveLocation(BigInteger n)
		{
			string directoryName = GetUniqueNameFromN(n);
			return Path.Combine(baseDirectory, directoryName);
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
