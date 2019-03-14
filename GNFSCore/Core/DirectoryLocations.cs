using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public class DirectoryLocations
	{
		private string _polynomial = null;
		private string _saveDirectory = null;
		private string _parameters = "GNFS.json";

		private static readonly int showDigits = 22;
		private static readonly string elipse = "[...]";
		private static readonly string saveRootDirectory = "GNFS";

		public string SaveDirectory { get { return _saveDirectory; } }
		public string GnfsParameters_SaveFile { get { return Path.Combine(SaveDirectory, _parameters); } }

		static DirectoryLocations()
		{
			if (!IsLinuxOS())
			{
				saveRootDirectory = "C:\\GNFS";
			}
		}

		public DirectoryLocations(string baseDirectory)
		{
			_saveDirectory = baseDirectory;
			SearchPolynomialPaths();
		}

		public DirectoryLocations(BigInteger n)
		{
			_saveDirectory = GetSaveLocation(n);
			SearchPolynomialPaths();
		}

		public DirectoryLocations(BigInteger n, BigInteger polynomialBase, BigInteger polynomialDegree)
		{
			_saveDirectory = GetSaveLocation(n);
			SetPolynomialPath(polynomialBase, polynomialDegree);
		}

		public static bool IsLinuxOS()
		{
			int p = (int)Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128); // 128 comes from mono run-times
		}

		private string SearchPolynomialPaths()
		{
			IEnumerable<string> polyDirectoriesrelationFiles = Directory.EnumerateDirectories(SaveDirectory, "Poly_B[*", SearchOption.TopDirectoryOnly);

			_polynomial = polyDirectoriesrelationFiles.FirstOrDefault() ?? SaveDirectory;
			return _polynomial;
		}

		public void SetPolynomialPath(BigInteger polynomialBase, BigInteger polynomialDegree)
		{
			_polynomial = Path.Combine(SaveDirectory, $"Poly_B[{polynomialBase}]_D[{polynomialDegree}]");
		}

		public static string GetSaveLocation(BigInteger n)
		{
			string directoryFilename = GetUniqueNameFromN(n);
			return Path.Combine(saveRootDirectory, directoryFilename);
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
