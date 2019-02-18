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
		public int A { get; private set; }
		public int B { get; private set; }
		public int Quantity { get; private set; }
		public int ValueRange { get; private set; }
		//private int IndexOfUnFactored;

		public CancellationToken CancelToken;

		public List<List<Relation>> FreeRelations { get { return Relations.FreeRelations; } }
		public List<Relation> SmoothRelations { get { return Relations.SmoothRelations; } }
		public List<Relation> UnFactored { get { return Relations.UnFactored; } }
		public List<RoughPair> RoughRelations { get { return Relations.RoughRelations; } }

		public RelationContainer Relations { get; set; }

		public FactorBase PrimeBase { get; private set; }

		private GNFS _gnfs;

		#region Internal Directory / FilePath  Information

		internal static string Polynomial_SaveDirectory { get; private set; }
		public static string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "Relations"); } }
		internal static string RelationProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Relations.Progress"); } }
		internal static string UnfactoredProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Unfactored.relations"); } }
		internal static string RoughRelations_Filename { get { return Path.Combine(Relations_SaveDirectory, "Rough.relations"); } }
		internal static string SmoothRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "SmoothRelations"); } }
		internal static string FreeRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "FreeRelations"); } }

		#endregion

		#region Constructors

		public PolyRelationsSieveProgress(GNFS gnfs, CancellationToken cancelToken, string polynomialSaveDirectory, int quantity, int valueRange)
		{
			using (CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken))
			{
				CancelToken = cancellationSource.Token;
			}

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

			A = 0;
			B = 3;
			Quantity = quantity;
			ValueRange = valueRange;
			//if (ValueRange > 400 && gnfs.N < 5000000)
			//{
			//	ValueRange = 400;
			//}

			Relations = new RelationContainer();
		}

		public PolyRelationsSieveProgress(GNFS gnfs, string polynomialSaveDirectory, int a, int b, int quantity, int valueRange, List<List<Relation>> free, List<Relation> smooth, List<RoughPair> rough, List<Relation> unfactored)
		{
			Polynomial_SaveDirectory = polynomialSaveDirectory;

			PrimeBase = new FactorBase();
			_gnfs = gnfs;

			A = a;
			B = b;
			Quantity = quantity;
			ValueRange = valueRange;

			Relations = new RelationContainer();
			Relations.FreeRelations = free;
			Relations.SmoothRelations = smooth;
			Relations.RoughRelations = rough;
			Relations.UnFactored = unfactored;

			PrimeBase = gnfs.PrimeFactorBase;

			using (CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(gnfs.CancelToken))
			{
				CancelToken = cancellationSource.Token;
			}
		}

		#endregion

		#region Processing / Computation

		public void GenerateRelations(CancellationToken cancelToken)
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

			if (A >= ValueRange)
			{
				ValueRange += 100;
			}

			int adjustedRange = ValueRange % 2 == 0 ? ValueRange + 1 : ValueRange;
			A = (A % 2 == 0) ? A + 1 : A;

			//var negativeRange = Enumerable.Range(-adjustedRange, adjustedRange - A).Reverse();
			//var positiveRange = Enumerable.Range(1 + A, adjustedRange - A);

			//IEnumerable<int> rangeA = negativeRange.Zip(positiveRange, (a, b) => new[] { a, b }).SelectMany(n => n); // Enumerable.Range(-adjustedRange, adjustedRange * 2);
			int maxB = Math.Min(adjustedRange * 2, Quantity);

			if (B >= maxB)
			{
				maxB += 100;
			}

			//IEnumerable<int> range = SieveRange.GetSieveRangeContinuation(A, ValueRange);

			while (Relations.SmoothRelations.Count < Quantity)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				if (B > maxB)
				{
					break;
				}

				foreach (int a in SieveRange.GetSieveRangeContinuation(A, ValueRange))
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}

					A = a;

					if (GCD.AreCoprime(A, B))
					{
						Relation rel = new Relation(_gnfs, A, B);

						rel.Sieve(_gnfs.CurrentRelationsProgress);

						bool smooth = rel.IsSmooth;
						if (smooth)
						{
							_gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Add(rel);
							rel.Save($"{SmoothRelations_SaveDirectory}\\{rel.A}_{rel.B}.relation");
						}
						else
						{
							_gnfs.CurrentRelationsProgress.Relations.RoughRelations.Add(new RoughPair(rel));
						}
					}
				}

				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				B += 2;
				A = 1;
			}

			SaveProgress();
		}


		#endregion

		#region Misc

		public void PurgePrimeRoughRelations()
		{
			List<RoughPair> roughRelations = Relations.RoughRelations.ToList();

			IEnumerable<RoughPair> toRemoveAlg = roughRelations
				.Where(r => r.AlgebraicQuotient != 1 && FactorizationFactory.IsProbablePrime(r.AlgebraicQuotient));

			roughRelations = roughRelations.Except(toRemoveAlg).ToList();

			Relations.RoughRelations = roughRelations;
			SaveRoughRelations();

			IEnumerable<RoughPair> toRemoveRational = roughRelations
				.Where(r => r.RationalQuotient != 1 && FactorizationFactory.IsProbablePrime(r.RationalQuotient));

			roughRelations = roughRelations.Except(toRemoveRational).ToList();

			Relations.RoughRelations = roughRelations;
			SaveRoughRelations();
		}

		public void AddFreeRelations(List<List<Relation>> freeRelations)
		{
			Relations.FreeRelations.AddRange(freeRelations);
			SaveFreeRelationSets(freeRelations, FreeRelations_SaveDirectory);
		}

		#endregion

		#region Save

		public void SaveProgress()
		{
			if (!Directory.Exists(Relations_SaveDirectory))
			{
				Directory.CreateDirectory(Relations_SaveDirectory);
			}

			new XDocument(
				new XElement("PolyRelationSieveProgress",
					new XElement("A", A.ToString()),
					new XElement("B", B.ToString()),
					new XElement("Quantity", Quantity.ToString()),
					new XElement("ValueRange", ValueRange.ToString())
				)
			).Save(RelationProgress_Filename);

			SaveFreeRelationSets(Relations.FreeRelations, FreeRelations_SaveDirectory);

			SaveSmoothRelations(Relations.SmoothRelations, SmoothRelations_SaveDirectory);

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

		private int FreeRelationsDirectoryCounter = 0;
		private void SaveFreeRelationSets(List<List<Relation>> relationsGroup, string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			foreach (List<Relation> set in relationsGroup)
			{
				string directoryName = $"{directory}\\{FreeRelationsDirectoryCounter}";

				if (Directory.Exists(directoryName))
				{
					directoryName = $"{directory}\\POTENTIALDUPLICATE___{FreeRelationsDirectoryCounter}";
					SaveSmoothRelations(set, directoryName);

					throw new Exception("Relations set directory counter somehow out of synch with number of actual directories. ");
				}
				else
				{
					SaveSmoothRelations(set, directoryName);
				}

				FreeRelationsDirectoryCounter += 1;
			}
		}

		private void SaveSmoothRelations(List<Relation> relations, string directory)
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

		#endregion

		#region Load

		public static PolyRelationsSieveProgress LoadProgress(GNFS gnfs, string polynomialSaveDirectory)
		{
			int a = 0;
			int b = 1;
			int quantity = 200;
			int valueRange = 200;

			Polynomial_SaveDirectory = polynomialSaveDirectory;

			if (Directory.Exists(Relations_SaveDirectory))
			{
				if (File.Exists(RelationProgress_Filename))
				{
					XElement xml = XElement.Load(RelationProgress_Filename);
					a = int.Parse(xml.Element("A").Value);
					b = int.Parse(xml.Element("B").Value);
					quantity = int.Parse(xml.Element("Quantity").Value);
					valueRange = int.Parse(xml.Element("ValueRange").Value);
				}

				List<List<Relation>> freeRelations = new List<List<Relation>>();
				List<Relation> smoothRelations = new List<Relation>();
				List<RoughPair> roughRelations = new List<RoughPair>();
				List<Relation> unFactored = new List<Relation>();

				freeRelations = LoadFreeRelationSets(FreeRelations_SaveDirectory);
				smoothRelations = LoadSmoothRelations(SmoothRelations_SaveDirectory);

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
				result = new PolyRelationsSieveProgress(gnfs, Polynomial_SaveDirectory, a, b, quantity, valueRange, freeRelations, smoothRelations, roughRelations, unFactored);

				result.FreeRelationsDirectoryCounter = freeRelations.Count;

				return result;
			}
			else
			{
				return new PolyRelationsSieveProgress(gnfs, gnfs.CancelToken, gnfs.SaveLocations.Polynomial_SaveDirectory, quantity, valueRange);
			}
		}

		private static List<List<Relation>> LoadFreeRelationSets(string directory)
		{
			List<List<Relation>> results = new List<List<Relation>>();
			if (Directory.Exists(directory))
			{
				IEnumerable<string> setDirectory = Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories);
				foreach (string dir in setDirectory)
				{
					List<Relation> relationSet = new List<Relation>();

					IEnumerable<string> relationFiles = Directory.EnumerateFiles(dir, "*.relation");
					foreach (string filename in relationFiles)
					{
						Relation rel = (Relation)Serializer.Deserialize(filename, typeof(Relation));
						relationSet.Add(rel);
					}

					results.Add(relationSet);
				}
			}
			return results;
		}

		private static List<Relation> LoadSmoothRelations(string directory)
		{
			List<Relation> results = new List<Relation>();
			if (Directory.Exists(directory))
			{
				IEnumerable<string> relationFiles = Directory.EnumerateFiles(directory, "*.relation");
				foreach (string filename in relationFiles)
				{
					Relation rel = (Relation)Serializer.Deserialize(filename, typeof(Relation));
					results.Add(rel);
				}
			}
			return results;
		}

		#endregion

		#region ToString

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
						BigInteger f = _gnfs.CurrentPolynomial.Evaluate((BigInteger)rel.A);
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

		#endregion

	}
}
