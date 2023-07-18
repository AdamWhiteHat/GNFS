using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public class DirectoryLocations
	{
		private const int showDigits = 22;
		private const string elipse = "[...]";

		private static string baseDirectory = "GNFS";
		private static string parametersFilename = "GNFS.json";
		private static string rationalFactorPairFilename = $"{nameof(GNFS.RationalFactorPairCollection)}.json";
		private static string algebraicFactorPairFilename = $"{nameof(GNFS.AlgebraicFactorPairCollection)}.json";
		private static string quadraticFactorPairFilename = $"{nameof(GNFS.QuadraticFactorPairCollection)}.json";
		private static string smoothRelationsFilename = $"{nameof(RelationContainer.SmoothRelations)}.json";
		private static string roughRelationsFilename = $"{nameof(RelationContainer.RoughRelations)}.json";
		private static string freeRelationsFilenameSearchpattern = $"{nameof(RelationContainer.FreeRelations)}_*.json";

		private string _saveDirectory = null;
		private string _rationalFactorPairFilepath = null;
		private string _algebraicFactorPairFilepath = null;
		private string _quadraticFactorPairFilepath = null;
		private string _parametersFilepath = null;
		private string _smoothRelationsFilepath = null;
		private string _roughRelationsFilepath = null;

		public static string SaveFilename { get { return parametersFilename; } }
		public string SaveDirectory { get { return _saveDirectory; } }

		public string GnfsParameters_SaveFile
		{
			get
			{
				if (_parametersFilepath == null)
				{
					_parametersFilepath = Path.Combine(SaveDirectory, parametersFilename);
				}
				return _parametersFilepath;
			}
		}

		public string RationalFactorPair_SaveFile
		{
			get
			{
				if (_rationalFactorPairFilepath == null)
				{
					_rationalFactorPairFilepath = Path.Combine(SaveDirectory, rationalFactorPairFilename);
				}
				return _rationalFactorPairFilepath;
			}
		}

		public string AlgebraicFactorPair_SaveFile
		{
			get
			{
				if (_algebraicFactorPairFilepath == null)
				{
					_algebraicFactorPairFilepath = Path.Combine(SaveDirectory, algebraicFactorPairFilename);
				}
				return _algebraicFactorPairFilepath;
			}
		}

		public string QuadraticFactorPair_SaveFile
		{
			get
			{
				if (_quadraticFactorPairFilepath == null)
				{
					_quadraticFactorPairFilepath = Path.Combine(SaveDirectory, quadraticFactorPairFilename);
				}
				return _quadraticFactorPairFilepath;
			}
		}

		public string SmoothRelations_SaveFile
		{
			get
			{
				if (_smoothRelationsFilepath == null)
				{
					_smoothRelationsFilepath = Path.Combine(SaveDirectory, smoothRelationsFilename);
				}
				return _smoothRelationsFilepath;
			}
		}

		public string RoughRelations_SaveFile
		{
			get
			{
				if (_roughRelationsFilepath == null)
				{
					_roughRelationsFilepath = Path.Combine(SaveDirectory, roughRelationsFilename);
				}
				return _roughRelationsFilepath;
			}
		}

		public string FreeRelations_SearchPattern { get { return freeRelationsFilenameSearchpattern; } }

		public DirectoryLocations(string saveLocation)
		{
			_saveDirectory = saveLocation;
		}

		public DirectoryLocations(BigInteger n)
			: this(GetSaveLocation(n))
		{
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

		public IEnumerable<string> EnumerateFreeRelationFiles()
		{
			return Directory.EnumerateFiles(SaveDirectory, FreeRelations_SearchPattern);
		}
	}
}
