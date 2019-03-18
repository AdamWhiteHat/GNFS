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
				loadJson = loadJson.Replace("][", ",").Trim();
				if (loadJson.First() != '[')
				{
					loadJson = "[\r\n" + loadJson;
				}
				if (loadJson.Last() != ']')
				{
					if (loadJson.Last() == ',')
					{
						loadJson = loadJson.Substring(0, loadJson.Length - 1);
					}
					loadJson = loadJson + "]";
				}
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
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.RationalFactorBase)}.json");
					if (File.Exists(filename))
					{
						gnfs.PrimeFactorBase.RationalFactorBase = Load.Generic<List<BigInteger>>(filename);
					}
				}

				public static void Algebraic(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.AlgebraicFactorBase)}.json");
					if (File.Exists(filename))
					{
						gnfs.PrimeFactorBase.AlgebraicFactorBase = Load.Generic<List<BigInteger>>(filename);
					}
				}

				public static void Quadratic(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFSCore.FactorBase.QuadraticFactorBase)}.json");
					if (File.Exists(filename))
					{
						gnfs.PrimeFactorBase.QuadraticFactorBase = Load.Generic<List<BigInteger>>(filename);
					}
				}
			}

			public static class FactorPair
			{
				public static void Rational(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.RationalFactorPairCollection)}.json");
					if (File.Exists(filename))
					{
						gnfs.RationalFactorPairCollection = Load.Generic<FactorPairCollection>(filename);

					}
				}

				public static void Algebraic(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.AlgebraicFactorPairCollection)}.json");
					if (File.Exists(filename))
					{


						gnfs.AlgebraicFactorPairCollection = Load.Generic<FactorPairCollection>(filename);
					}

				}

				public static void Quadratic(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(GNFS.QuadradicFactorPairCollection)}.json");
					if (File.Exists(filename))
					{
						gnfs.QuadradicFactorPairCollection = Load.Generic<FactorPairCollection>(filename);
					}

				}
			}

			public static class Relations
			{
				public static void Smooth(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.SmoothRelations)}.json");
					if (File.Exists(filename))
					{
						List<Relation> temp = Load.Generic<List<Relation>>(filename);
						temp.ForEach(rel => rel.IsPersisted = true);
						gnfs.CurrentRelationsProgress.SmoothRelationsCounter = temp.Count;
						gnfs.CurrentRelationsProgress.Relations.SmoothRelations = temp;
					}
				}

				public static void Rough(ref GNFS gnfs)
				{
					string filename = Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.RoughRelations)}.json");
					if (File.Exists(filename))
					{
						List<Relation> temp = Load.Generic<List<Relation>>(filename);
						temp.ForEach(rel => rel.IsPersisted = true);
						gnfs.CurrentRelationsProgress.Relations.RoughRelations = temp;
					}
				}

				public static void Free(ref GNFS gnfs)
				{
					if (gnfs.CurrentRelationsProgress.Relations.FreeRelations.Any(lst => lst.Any(rel => !rel.IsPersisted)))
					{
						List<List<Relation>> unsaved = gnfs.CurrentRelationsProgress.Relations.FreeRelations.Where(lst => lst.Any(rel => !rel.IsPersisted)).ToList();
						foreach (List<Relation> solution in unsaved)
						{
							Serialization.Save.Object(solution, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"!!UNSAVED__{nameof(RelationContainer.FreeRelations)}.json"));
						}
					}

					gnfs.CurrentRelationsProgress.Relations.FreeRelations.Clear();
					gnfs.CurrentRelationsProgress.FreeRelationsCounter = 0;

					IEnumerable<string> freeRelations = Directory.EnumerateFiles(gnfs.SaveLocations.SaveDirectory, $"{nameof(RelationContainer.FreeRelations)}_*.json");
					foreach (string solution in freeRelations)
					{
						List<Relation> temp = Load.Generic<List<Relation>>(solution);
						temp.ForEach(rel => rel.IsPersisted = true);
						gnfs.CurrentRelationsProgress.Relations.FreeRelations.Add(temp);
						gnfs.CurrentRelationsProgress.FreeRelationsCounter += 1;
					}
				}
			}
		}
	}
}
