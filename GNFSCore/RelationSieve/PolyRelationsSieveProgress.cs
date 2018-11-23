using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Xml.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GNFSCore
{
	using IntegerMath;
	public class PolyRelationsSieveProgress
	{
		private int B;
		private int Quantity;
		private int ValueRange;
		//private int IndexOfUnFactored;

		public CancellationToken CancelToken;

		public List<List<Relation>> FreeRelations { get { return Relations.FreeRelations; } }
		public List<Relation> SmoothRelations { get { return Relations.SmoothRelations; } }
		public List<Relation> UnFactored { get { return Relations.UnFactored; } }
		public List<RoughPair> RoughRelations { get { return Relations.RoughRelations; } }

		public RelationContainer Relations { get; set; }

		public FactorBase PrimeBase { get; private set; }

		private GNFS _gnfs;

		internal static string Polynomial_SaveDirectory { get; private set; }
		public static string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "Relations"); } }
		internal static string RelationProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Relations.Progress"); } }
		internal static string UnfactoredProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Unfactored.relations"); } }
		internal static string RoughRelations_Filename { get { return Path.Combine(Relations_SaveDirectory, "Rough.relations"); } }
		internal static string SmoothRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "SmoothRelations"); } }
		internal static string FreeRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "FreeRelations"); } }

		public PolyRelationsSieveProgress(GNFS gnfs, CancellationToken cancelToken, string polynomialSaveDirectory, int quantity, int valueRange)
		{
			CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
			CancelToken = cancellationSource.Token;

			Polynomial_SaveDirectory = polynomialSaveDirectory;

			if (!Directory.Exists(Relations_SaveDirectory))
			{
				Directory.CreateDirectory(Relations_SaveDirectory);
			}

			if (!Directory.Exists(SmoothRelations_SaveDirectory))
			{
				Directory.CreateDirectory(SmoothRelations_SaveDirectory);
			}

			_gnfs = gnfs;
			PrimeBase = new FactorBase();
			PrimeBase = gnfs.PrimeFactorBase;

			B = 1;
			Quantity = quantity;
			ValueRange = valueRange;
			if (ValueRange > 400 && gnfs.N < 5000000)
			{
				ValueRange = 400;
			}

			Relations = new RelationContainer();
		}

		public PolyRelationsSieveProgress(GNFS gnfs, string polynomialSaveDirectory, int b, int quantity, int valueRange, List<List<Relation>> free, List<Relation> smooth, List<RoughPair> rough, List<Relation> unfactored)
		{
			Polynomial_SaveDirectory = polynomialSaveDirectory;

			PrimeBase = new FactorBase();
			_gnfs = gnfs;

			B = b;
			Quantity = quantity;
			ValueRange = valueRange;

			Relations = new RelationContainer();
			Relations.FreeRelations = free;
			Relations.SmoothRelations = smooth;
			Relations.RoughRelations = rough;
			Relations.UnFactored = unfactored;

			PrimeBase = gnfs.PrimeFactorBase;

			CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(gnfs.CancelToken);
			CancelToken = cancellationSource.Token;
		}

		public List<Relation> GenerateRelations(CancellationToken cancelToken)
		{
			if (!Directory.Exists(Relations_SaveDirectory))
			{
				Directory.CreateDirectory(Relations_SaveDirectory);
			}

			if (!Directory.Exists(SmoothRelations_SaveDirectory))
			{
				Directory.CreateDirectory(SmoothRelations_SaveDirectory);
			}



			if (Quantity == -1)
			{
				Quantity = _gnfs.RFB.Count + _gnfs.AFB.Count + _gnfs.QFB.Count + 1;
			}
			else if (Relations.SmoothRelations.Count >= Quantity)
			{
				Quantity += 2000;
			}

			if (B >= ValueRange)
			{
				ValueRange += 100;
			}

			int adjustedRange = ValueRange % 2 == 0 ? ValueRange + 1 : ValueRange;
			IEnumerable<int> A = Enumerable.Range(-adjustedRange, adjustedRange * 2);
			int maxB = Math.Min(adjustedRange * 2, Quantity);

			List<Relation> newestRelations = new List<Relation>();

			while (Relations.SmoothRelations.Count < Quantity && B < ValueRange)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				List<int> coprimes = A.Where(a => GCD.AreCoprime(a, B)).ToList();
				List<Relation> unfactored = coprimes.Select(a => new Relation(_gnfs, a, B)).ToList();

				newestRelations.AddRange(SieveRelations(cancelToken, _gnfs.CurrentRelationsProgress, unfactored));

				if (B > maxB)
				{
					break;
				}

				B += 2;
			}

			SaveProgress();
			return newestRelations;
		}

		public static List<Relation> SieveRelations(CancellationToken cancelToken, PolyRelationsSieveProgress relationsProgress, List<Relation> unfactored)
		{
			//IndexOfUnFactored = 0;
			List<Relation> smoothRelations = new List<Relation>();
			List<RoughPair> roughRelations = new List<RoughPair>();

			int index = 0;
			int max = unfactored.Count;

			while (index < max)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				Relation rel = unfactored[index];

				//BigInteger algResult = rel.Factor(relationsProgress.PrimeBase.AlgebraicFactorBase, rel.AlgebraicNorm, rel.AlgebraicQuotient, rel.AlgebraicFactorization);
				//BigInteger ratReslult = rel.Factor(relationsProgress.PrimeBase.RationalFactorBase, rel.RationalNorm, rel.RationalQuotient, rel.RationalFactorization);

				//AlgebraicQuotient = algResult;
				//RationalQuotient = ratReslult;

				rel.Sieve(relationsProgress);
				bool smooth = rel.IsSmooth;
				if (smooth)
				{
					smoothRelations.Add(rel);
					rel.Save($"{SmoothRelations_SaveDirectory}\\{rel.A}_{rel.B}.relation");
				}
				else
				{
					roughRelations.Add(new RoughPair(rel));
				}

				index++;
			}

			if (smoothRelations.Any())
			{
				relationsProgress.Relations.SmoothRelations.AddRange(smoothRelations);
			}
			if (roughRelations.Any())
			{
				relationsProgress.Relations.RoughRelations.AddRange(roughRelations);
			}

			return new List<Relation>(smoothRelations);
		}

		public void PurgePrimeRoughRelations()
		{
			List<RoughPair> roughRelations = Relations.RoughRelations.ToList();
			
			IEnumerable<RoughPair> toRemoveAlg = roughRelations
				.Where(r => r.AlgebraicQuotient != 1 && FactorizationFactory.IsProbablePrime(r.AlgebraicQuotient));
				
			roughRelations = roughRelations.Except(toRemoveAlg).ToList();

			Relations.RoughRelations = roughRelations;
			SaveRoughRelations();

			IEnumerable <RoughPair> toRemoveRational = roughRelations
				.Where(r => r.RationalQuotient != 1 && FactorizationFactory.IsProbablePrime(r.RationalQuotient));

			roughRelations = roughRelations.Except(toRemoveRational).ToList();

			Relations.RoughRelations = roughRelations;
			SaveRoughRelations();			
		}


		public string FormatRelations(IEnumerable<Relation> relations)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine($"Smooth relations:");
			result.AppendLine("\t_______________________________________________");
			result.AppendLine($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tQuantity: {Relations.SmoothRelations.Count} Target quantity: {(_gnfs.RFB.Count + _gnfs.AFB.Count + _gnfs.QFB.Count + 1).ToString()}");
			result.AppendLine("\t```````````````````````````````````````````````");
			foreach (Relation rel in relations.OrderByDescending(rel => rel.A * rel.B))
			{
				result.AppendLine(rel.ToString());
				result.AppendLine("Algebraic " + rel.AlgebraicFactorization.FormatStringAsFactorization());
				result.AppendLine("Rational  " + rel.RationalFactorization.FormatStringAsFactorization());
				result.AppendLine();
			}
			result.AppendLine();

			return result.ToString();
		}

		public override string ToString()
		{
			if (Relations.FreeRelations.Any())
			{
				StringBuilder result = new StringBuilder();

				List<Relation> relations = Relations.FreeRelations.First();

				result.AppendLine(FormatRelations(relations));

				BigInteger algebraic = relations.Select(rel => rel.AlgebraicNorm).Product();
				BigInteger rational = relations.Select(rel => rel.RationalNorm).Product();

				bool isAlgebraicSquare = algebraic.IsSquare();
				bool isRationalSquare = rational.IsSquare();

				CountDictionary algCountDict = new CountDictionary();
				foreach (var rel in relations)
				{
					algCountDict.Combine(rel.AlgebraicFactorization);
				}

				result.AppendLine("---");
				result.AppendLine($"Rational  ∏(a+mb): IsSquare? {isRationalSquare} : {rational}");
				result.AppendLine($"Algebraic ∏ƒ(a/b): IsSquare? {isAlgebraicSquare} : {algebraic}");
				result.AppendLine();
				result.AppendLine($"Algebraic factorization (as prime ideals): {algCountDict.FormatStringAsFactorization()}");
				result.AppendLine();

				result.AppendLine();
				result.AppendLine("");
				result.AppendLine(string.Join(Environment.NewLine,
					relations.Select(rel =>
					{
						BigInteger f = _gnfs.CurrentPolynomial.Evaluate(rel.A);
						if (rel.B == 0)
						{
							return "";
						}
						return $"ƒ({rel.A}) ≡ {f} ≡ {(f % rel.B)} (mod {rel.B})";
					}
					)));
				result.AppendLine();



				return result.ToString();
			}
			else
			{
				return FormatRelations(Relations.SmoothRelations);
			}
		}


		public void SetFreeRelations(List<List<Relation>> freeRelations)
		{
			Relations.FreeRelations.AddRange(freeRelations);
			SaveRelationSetsToDirectory(freeRelations, FreeRelations_SaveDirectory);
		}

		public void SaveProgress()
		{
			if (!Directory.Exists(Relations_SaveDirectory))
			{
				Directory.CreateDirectory(Relations_SaveDirectory);
			}

			new XDocument(
				new XElement("PolyRelationSieveProgress",
					new XElement("B", B.ToString()),
					new XElement("Quantity", Quantity.ToString()),
					new XElement("ValueRange", ValueRange.ToString())
				)
			).Save(RelationProgress_Filename);

			SaveRelationSetsToDirectory(Relations.FreeRelations, FreeRelations_SaveDirectory);

			SaveRelationsToDirectory(Relations.SmoothRelations, SmoothRelations_SaveDirectory);

			SaveRoughRelations();

			if (Relations.UnFactored.Any())
			{
				if (File.Exists(UnfactoredProgress_Filename))
				{
					File.Delete(UnfactoredProgress_Filename);
				}

				Relation.SerializeUnfactoredToFile(UnfactoredProgress_Filename, Relations.UnFactored);
			}
		}

		private void SaveRoughRelations()
		{
			if (Relations.RoughRelations.Any())
			{
				if (File.Exists(RoughRelations_Filename))
				{
					File.Delete(RoughRelations_Filename);
				}

				// Write out RoughRelations file
				RoughPair.SaveToFile(RoughRelations_Filename, Relations.RoughRelations);
			}
		}

		public static PolyRelationsSieveProgress LoadProgress(GNFS gnfs, string polynomialSaveDirectory)
		{
			int b = 1;
			int quantity = 200;
			int valueRange = 200;

			Polynomial_SaveDirectory = polynomialSaveDirectory;

			if (Directory.Exists(Relations_SaveDirectory))
			{
				if (File.Exists(RelationProgress_Filename))
				{
					XElement xml = XElement.Load(RelationProgress_Filename);
					b = int.Parse(xml.Element("B").Value);
					quantity = int.Parse(xml.Element("Quantity").Value);
					valueRange = int.Parse(xml.Element("ValueRange").Value);
				}

				List<List<Relation>> freeRelations = new List<List<Relation>>();
				List<Relation> smoothRelations = new List<Relation>();
				List<RoughPair> roughRelations = new List<RoughPair>();
				List<Relation> unFactored = new List<Relation>();



				freeRelations = LoadRelationsSetsFromDirectory(FreeRelations_SaveDirectory);

				smoothRelations = LoadRelationsFromDirectory(SmoothRelations_SaveDirectory);

				if (File.Exists(RoughRelations_Filename))
				{
					roughRelations = RoughPair.LoadFromFile(RoughRelations_Filename);
				}
				else
				{
					roughRelations = new List<RoughPair>();
				}

				if (File.Exists(UnfactoredProgress_Filename))
				{
					unFactored = Relation.LoadUnfactoredFile(gnfs, UnfactoredProgress_Filename);
				}
				else
				{
					unFactored = new List<Relation>();
				}

				PolyRelationsSieveProgress result = null;
				result = new PolyRelationsSieveProgress(gnfs, Polynomial_SaveDirectory, b, quantity, valueRange, freeRelations, smoothRelations, roughRelations, unFactored);

				result.RelationSetsDirectoryCounter = freeRelations.Count;

				return result;
			}
			else
			{
				return new PolyRelationsSieveProgress(gnfs, gnfs.CancelToken, gnfs.SaveLocations.Polynomial_SaveDirectory, quantity, valueRange);
			}
		}

		private int RelationSetsDirectoryCounter = 0;
		private void SaveRelationSetsToDirectory(List<List<Relation>> relationsGroup, string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			foreach (List<Relation> set in relationsGroup)
			{
				string directoryName = $"{directory}\\{RelationSetsDirectoryCounter}";

				if (Directory.Exists(directoryName))
				{
					directoryName = $"{directory}\\POTENTIALDUPLICATE___{RelationSetsDirectoryCounter}";
					SaveRelationsToDirectory(set, directoryName);

					throw new Exception("Relations set directory counter somehow out of synch with number of actual directories. ");
				}
				else
				{
					SaveRelationsToDirectory(set, directoryName);
				}

				RelationSetsDirectoryCounter += 1;
			}
		}

		private void SaveRelationsToDirectory(List<Relation> relations, string directory)
		{
			if (relations.Any())
			{
				// Create directory
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				// Write out relations
				foreach (Relation rel in relations)
				{
					string filename = $"{directory}\\{rel.A}_{rel.B}.relation";
					if (!File.Exists(filename))
					{
						rel.Save(filename);
					}
				}
			}
		}

		private static List<List<Relation>> LoadRelationsSetsFromDirectory(string directory)
		{
			List<List<Relation>> results = new List<List<Relation>>();
			if (Directory.Exists(directory))
			{
				IEnumerable<string> setDirectory = Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories);
				foreach (string dir in setDirectory)
				{
					IEnumerable<string> relationFiles = Directory.EnumerateFiles(dir, "*.relation");
					results.Add(relationFiles.Select(fn => Relation.LoadFromFile(fn)).ToList());
				}
			}
			return results;
		}

		private static List<Relation> LoadRelationsFromDirectory(string directory)
		{
			List<Relation> results = new List<Relation>();
			if (Directory.Exists(directory))
			{
				IEnumerable<string> relationFiles = Directory.EnumerateFiles(directory, "*.relation");
				results = relationFiles.Select(fn => Relation.LoadFromFile(fn)).ToList();
			}
			return results;
		}

	}
}
