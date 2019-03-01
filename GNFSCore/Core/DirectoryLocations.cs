using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Threading;

using GNFSCore.Factors;
using GNFSCore.Polynomials;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class DirectoryLocations
	{
		private string _polynomial = null;
		private string _saveDirectory = null;
		private string _parameters = "_GNFS.Parameters";

		private static readonly int showDigits = 22;
		private static readonly string elipse = "[...]";
		private static readonly string saveRootDirectory = "GNFS";

		public string SaveDirectory { get { return _saveDirectory; } }

		public string GnfsParameters_SaveFile { get { return Path.Combine(SaveDirectory, _parameters); } }

		public string Polynomial_Filename { get { return "_Polynomial.Parameters"; } }
		public string Polynomial_SaveDirectory { get { return _polynomial ?? SearchPolynomialPaths(); } }

		public string Polynomial_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, Polynomial_Filename); } }
		public string RationalFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Rational.FactorBase"); } }
		public string AlgebraicFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Algebraic.FactorBase"); } }
		public string QuadradicFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Quadradic.FactorBase"); } }

		public string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "Relations"); } }
		public string RelationProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Relations.Progress"); } }
		public string UnfactoredProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Unfactored.relations"); } }
		public string RoughRelations_Filename { get { return Path.Combine(Relations_SaveDirectory, "Rough.relations"); } }
		public string SmoothRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "SmoothRelations"); } }
		public string FreeRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "FreeRelations"); } }

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
			_saveDirectory = GenerateSaveDirectory(n);
			SearchPolynomialPaths();
		}

		public DirectoryLocations(BigInteger n, BigInteger polynomialBase, BigInteger polynomialDegree)
		{
			_saveDirectory = GenerateSaveDirectory(n);
			SetPolynomialPath(polynomialBase, polynomialDegree);
		}

		private static bool IsLinuxOS()
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

		public static string GenerateSaveDirectory(BigInteger n)
		{
			string directoryFilename = GenerateFileNameFromBigInteger(n);
			return Path.Combine(saveRootDirectory, directoryFilename);
		}

		public static string GenerateFileNameFromBigInteger(BigInteger n)
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
