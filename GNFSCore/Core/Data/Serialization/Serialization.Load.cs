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
using ExtendedArithmetic;

namespace GNFSCore
{
	using Factors;
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.RelationSieve;
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

			public static T GenericFixedArray<T>(string filename)
			{
				string loadJson = File.ReadAllText(filename).TrimStart(',');
				string fixedJson = FixAppendedJsonArrays(loadJson);
				return JsonConvert.DeserializeObject<T>(fixedJson);
			}

			private static string FixAppendedJsonArrays(string input)
			{
				//string inputJson = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()); // Remove all whitespace
				//inputJson = inputJson.Replace("[", "").Replace("]", ""); // Remove square brackets. There may be many, due to multiple calls to Serialization.Save.Relations.Smooth.Append()
				//inputJson = inputJson.Replace("}{", "},{"); // Insert commas between item instances
				string inputJson = input.Insert(input.Length, "]").Insert(0, "["); // Re-add square brackets.
				return inputJson;
			}

			public static GNFS All(string filename)
			{
				string loadJson = File.ReadAllText(filename);
				GNFS gnfs = JsonConvert.DeserializeObject<GNFS>(loadJson);

				string directoryName = Path.GetDirectoryName(filename);
				gnfs.SaveLocations = new DirectoryLocations(directoryName);

				int counter = 0;
				bool finished = false;
				string polyFilename = string.Empty;
				do
				{
					counter++;
					polyFilename = Path.GetFullPath(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"Polynomial.{counter:00}"));
					if (File.Exists(polyFilename))
					{
						Polynomial deserializedPoly = Load.Polynomial(polyFilename);
						gnfs.PolynomialCollection.Add(deserializedPoly);
					}
					else
					{
						finished = true;
					}
				}
				while (!finished);

				gnfs.CurrentPolynomial = gnfs.PolynomialCollection.First();
				gnfs.PolynomialDegree = gnfs.CurrentPolynomial.Degree;

				Load.FactorBase(ref gnfs);

				Load.FactorPair.Rational(ref gnfs);
				Load.FactorPair.Algebraic(ref gnfs);
				Load.FactorPair.Quadratic(ref gnfs);

				gnfs.CurrentRelationsProgress._gnfs = gnfs;

				Load.Relations.Smooth(ref gnfs);
				Load.Relations.Rough(ref gnfs);
				Load.Relations.Free(ref gnfs);

				return gnfs;
			}

			public static Polynomial Polynomial(string filename)
			{
				if (!File.Exists(filename))
				{
					throw new FileNotFoundException("Cannot find polynomial file!", filename);
				}
				string polyJson = File.ReadAllText(filename);
				Polynomial result = JsonConvert.DeserializeObject<Polynomial>(polyJson, new JsonConverter[] { new JsonTermConverter(), new JsonPolynomialConverter() });
				return result;
			}

			public static void FactorBase(ref GNFS gnfs)
			{
				gnfs.SetPrimeFactorBases();
			}

			/*
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
			*/

			public static class FactorPair
			{
				public static void Rational(ref GNFS gnfs)
				{
					if (File.Exists(gnfs.SaveLocations.RationalFactorPair_SaveFile))
					{
						gnfs.RationalFactorPairCollection = Load.Generic<FactorPairCollection>(gnfs.SaveLocations.RationalFactorPair_SaveFile);

					}
				}

				public static void Algebraic(ref GNFS gnfs)
				{
					if (File.Exists(gnfs.SaveLocations.AlgebraicFactorPair_SaveFile))
					{
						gnfs.AlgebraicFactorPairCollection = Load.Generic<FactorPairCollection>(gnfs.SaveLocations.AlgebraicFactorPair_SaveFile);
					}

				}

				public static void Quadratic(ref GNFS gnfs)
				{
					if (File.Exists(gnfs.SaveLocations.QuadraticFactorPair_SaveFile))
					{
						gnfs.QuadraticFactorPairCollection = Load.Generic<FactorPairCollection>(gnfs.SaveLocations.QuadraticFactorPair_SaveFile);
					}

				}
			}

			public static class Relations
			{
				public static void Smooth(ref GNFS gnfs)
				{
					if (File.Exists(gnfs.SaveLocations.SmoothRelations_SaveFile))
					{
						List<Relation> temp = Load.GenericFixedArray<List<Relation>>(gnfs.SaveLocations.SmoothRelations_SaveFile);
						var nullRels = temp.Where(x => x is null).ToList();
						if (nullRels.Any())
						{
							temp = temp.Where(x => !(x is null)).ToList();
						}
						temp.ForEach(rel => rel.IsPersisted = true);
						gnfs.CurrentRelationsProgress.SmoothRelationsCounter = temp.Count;
						gnfs.CurrentRelationsProgress.Relations.SmoothRelations = temp;
					}
				}

				public static void Rough(ref GNFS gnfs)
				{
					if (File.Exists(gnfs.SaveLocations.RoughRelations_SaveFile))
					{
						List<Relation> temp = Load.GenericFixedArray<List<Relation>>(gnfs.SaveLocations.RoughRelations_SaveFile);
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

					IEnumerable<string> freeRelations = gnfs.SaveLocations.EnumerateFreeRelationFiles();
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
