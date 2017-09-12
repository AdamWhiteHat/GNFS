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

using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class DirectoryLocations
	{
		public DirectoryLocations(string baseDirectory)
		{
			_saveDirectory = baseDirectory;
		}

		private string _polynomial = null;
		private string _saveDirectory= null;
		private string _parameters = "_GNFS.Parameters";

		public string SaveDirectory { get { return _saveDirectory; } /*set { _saveDirectory = value; }*/ }

		public string GnfsParameters_SaveFile { get { return Path.Combine(SaveDirectory, _parameters); } }

		public string Polynomial_Filename { get { return "_Polynomial.Parameters"; } }
		public string Polynomial_SaveDirectory { get { return _polynomial ?? GetPolynomialPath(); } }
		public string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "SmoothRelations"); } }

		public string RationalFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Rational.FactorBase"); } }
		public string AlgebraicFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Algebraic.FactorBase"); } }
		public string QuadradicFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Quadradic.FactorBase"); } }

		public string GetPolynomialPath()
		{
			IEnumerable<string> polyDirectoriesrelationFiles = Directory.EnumerateDirectories(SaveDirectory, "Poly_B[*", SearchOption.TopDirectoryOnly);
			
			_polynomial = polyDirectoriesrelationFiles.FirstOrDefault() ?? SaveDirectory;
			return _polynomial;
		}

		public void SetPolynomialPath(IPolynomial poly)
		{
			_polynomial = Path.Combine(SaveDirectory, $"Poly_B[{ poly.Base}]_D[{ poly.Degree}]");
		}
	}

	public static class GNFSFactory
	{
		public static class Create
		{
			public static GNFS FromFile(CancellationToken cancelToken, string openDirectory)
			{
				GNFS result = new GNFS();

				result.CancelToken = cancelToken;
				result.PolynomialCollection = new List<IPolynomial>();

				DirectoryLocations directories = new DirectoryLocations(openDirectory);
				result.SaveLocations = directories;

				if (!Directory.Exists(directories.SaveDirectory))
				{
					throw new ArgumentException($"Parameter {nameof(openDirectory)} must be a directory, that directory must exist and must contain the file {directories.GnfsParameters_SaveFile}");
				}

				if (!File.Exists(directories.GnfsParameters_SaveFile))
				{
					throw new FileNotFoundException($"File does not exist: \"{directories.GnfsParameters_SaveFile}\"!");
				}

				return result;//LoadGnfsProgress();
			}

			public static GNFS New(CancellationToken cancelToken, BigInteger n, BigInteger polynomialBase, int polyDegree = -1)
			{
				GNFS result = new GNFS();

				result.CancelToken = cancelToken;
				result.PolynomialCollection = new List<IPolynomial>();
				result.N = n;
				/*
				DirectoryLocations directories = new DirectoryLocations(GenerateSaveDirectory(N));
				result.SaveLocations = directories;

				if (!Directory.Exists(directories.SaveDirectory))
				{
					// New GNFS instance
					Directory.CreateDirectory(directories.SaveDirectory);

					int degree = -1;
					if (polyDegree == -1)
					{
						degree = result.CalculateDegree(n);
					}
					else
					{
						degree = polyDegree;
					}
					
					result.CaclulatePrimeBounds();
					result.ConstructNewPolynomial(polynomialBase,degree);
					result.m = polynomialBase;

					result.CurrentRelationsProgress = new PolyRelationsSieveProgress(result, cancelToken, directories.Polynomial_SaveDirectory);

					result.SaveGnfsProgress();

					result.LoadFactorBases();
				}
				else
				{
					GNFS gnfs = (GNFS)Serializer.Deserialize(directories.GnfsParameters_SaveFile, typeof(GNFS));
					gnfs.LoadGnfsProgress(gnfs);
				}
				*/
				return result;
			}
			
			public static void SaveGnfsProgress(GNFS gnfs)
			{
				Serializer.Serialize(gnfs.SaveLocations.GnfsParameters_SaveFile, gnfs);
			}
			
		}
	}
}
