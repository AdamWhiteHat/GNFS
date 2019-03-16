using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace GNFSCore
{
	using Factors;
	using Interfaces;

	public static partial class Serialization
	{
		public static class Load
		{
			public static T Generic<T>(string filename)
			{
				string loadJson = File.ReadAllText(filename);
				return JsonConvert.DeserializeObject<T>(loadJson);
			}

			public static GNFS Gnfs(CancellationToken cancelToken, string filename)
			{
				string loadJson = File.ReadAllText(filename);
				GNFS gnfs = JsonConvert.DeserializeObject<GNFS>(loadJson);
				gnfs.CancelToken = cancelToken;

				gnfs.SaveLocations = new DirectoryLocations(Path.GetDirectoryName(filename));

				gnfs.CurrentPolynomial = gnfs.PolynomialCollection.Last();

				Load.FactorBase.Rational(ref gnfs);
				Load.FactorBase.Algebraic(ref gnfs);
				Load.FactorBase.Quadratic(ref gnfs);

				Load.FactorPair.Rational(ref gnfs);
				Load.FactorPair.Algebraic(ref gnfs);
				Load.FactorPair.Quadratic(ref gnfs);

				gnfs.CurrentRelationsProgress._gnfs = gnfs;

				Load.Relations.Smooth(ref gnfs);
				Load.Relations.Rough(ref gnfs);
				Load.Relations.Free(ref gnfs);

				return gnfs;
			}

			public static class FactorBase
			{
				public static void Rational(ref GNFS gnfs)
				{
					gnfs.PrimeFactorBase.RationalFactorBase = Load.Generic<List<BigInteger>>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.RationalFactorBase)}.json"));
				}

				public static void Algebraic(ref GNFS gnfs)
				{
					gnfs.PrimeFactorBase.AlgebraicFactorBase = Load.Generic<List<BigInteger>>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.AlgebraicFactorBase)}.json"));
				}

				public static void Quadratic(ref GNFS gnfs)
				{
					gnfs.PrimeFactorBase.QuadraticFactorBase = Load.Generic<List<BigInteger>>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.QuadraticFactorBase)}.json"));
				}
			}

			public static class FactorPair
			{
				public static void Rational(ref GNFS gnfs)
				{
					gnfs.RationalFactorPairCollection = Load.Generic<FactorPairCollection>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.RationalFactorPairCollection)}.json"));
				}

				public static void Algebraic(ref GNFS gnfs)
				{
					gnfs.AlgebraicFactorPairCollection = Load.Generic<FactorPairCollection>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.AlgebraicFactorPairCollection)}.json"));

				}

				public static void Quadratic(ref GNFS gnfs)
				{
					gnfs.QuadradicFactorPairCollection = Load.Generic<FactorPairCollection>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.QuadradicFactorPairCollection)}.json"));
				}
			}

			public static class Relations
			{
				public static void Smooth(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.SmoothRelations)}.json");
					if (File.Exists(filename))
					{
						gnfs.CurrentRelationsProgress.Relations.SmoothRelations = Load.Generic<List<Relation>>(filename);
					}
				}

				public static void Rough(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.RoughRelations)}.json");
					if (File.Exists(filename))
					{
						gnfs.CurrentRelationsProgress.Relations.RoughRelations = Load.Generic<List<Relation>>(filename);
					}
				}

				public static void Free(ref GNFS gnfs)
				{
					if (gnfs.CurrentRelationsProgress.Relations.FreeRelations.Any())
					{
						gnfs.CurrentRelationsProgress.Relations.FreeRelations.Clear();
					}

					IEnumerable<string> freeRelations = Directory.EnumerateFiles(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.FreeRelations)}_*.json");
					foreach (string solution in freeRelations)
					{
						gnfs.CurrentRelationsProgress.Relations.FreeRelations.Add(Load.Generic<List<Relation>>(solution));
					}
				}
			}
		}
	}
}
