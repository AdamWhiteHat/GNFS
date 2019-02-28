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

		internal GNFS _gnfs;

		internal DirectoryLocations _directoryLocations;


		#region Constructors

		public PolyRelationsSieveProgress(GNFS gnfs)
		{
			_gnfs = gnfs;
			_directoryLocations = gnfs.SaveLocations;

			using (CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(_gnfs.CancelToken))
			{
				CancelToken = cancellationSource.Token;
			}

			if (!Directory.Exists(_directoryLocations.Relations_SaveDirectory))
			{
				Directory.CreateDirectory(_directoryLocations.Relations_SaveDirectory);
			}

			if (!Directory.Exists(_directoryLocations.SmoothRelations_SaveDirectory))
			{
				Directory.CreateDirectory(_directoryLocations.SmoothRelations_SaveDirectory);
			}

			PrimeBase = new FactorBase();
			Relations = new RelationContainer();

			PrimeBase = gnfs.PrimeFactorBase;
		}

		public PolyRelationsSieveProgress(GNFS gnfs, int quantity, int valueRange)
			: this(gnfs)
		{
			A = 0;
			B = 3;
			Quantity = quantity;
			ValueRange = valueRange;			
		}

		public PolyRelationsSieveProgress(GNFS gnfs, int a, int b, int quantity, int valueRange, List<List<Relation>> free, List<Relation> smooth, List<RoughPair> rough, List<Relation> unfactored)
			: this(gnfs, quantity, valueRange)
		{
			A = a;
			B = b;

			Relations.FreeRelations = free;
			Relations.SmoothRelations = smooth;
			Relations.RoughRelations = rough;
			Relations.UnFactored = unfactored;			
		}

		#endregion

		#region Processing / Computation

		public void GenerateRelations(CancellationToken cancelToken)
		{
			if (!Directory.Exists(_directoryLocations.Relations_SaveDirectory))
			{
				Directory.CreateDirectory(_directoryLocations.Relations_SaveDirectory);
			}

			if (!Directory.Exists(_directoryLocations.SmoothRelations_SaveDirectory))
			{
				Directory.CreateDirectory(_directoryLocations.SmoothRelations_SaveDirectory);
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
							rel.Save($"{_directoryLocations.SmoothRelations_SaveDirectory}\\{rel.A}_{rel.B}.relation");
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
			SaveFreeRelationSets(freeRelations, _directoryLocations.FreeRelations_SaveDirectory);
		}

		#endregion

		#region Save

		public void SaveProgress()
		{
			if (!Directory.Exists(_directoryLocations.Relations_SaveDirectory))
			{
				Directory.CreateDirectory(_directoryLocations.Relations_SaveDirectory);
			}

			new XDocument(
				new XElement("PolyRelationSieveProgress",
					new XElement("A", A.ToString()),
					new XElement("B", B.ToString()),
					new XElement("Quantity", Quantity.ToString()),
					new XElement("ValueRange", ValueRange.ToString())
				)
			).Save(_directoryLocations.RelationProgress_Filename);

			SaveFreeRelationSets(Relations.FreeRelations, _directoryLocations.FreeRelations_SaveDirectory);

			SaveSmoothRelations(Relations.SmoothRelations, _directoryLocations.SmoothRelations_SaveDirectory);

			SaveRoughRelations();

			if (Relations.UnFactored.Any())
			{
				if (File.Exists(_directoryLocations.UnfactoredProgress_Filename))
				{
					File.Delete(_directoryLocations.UnfactoredProgress_Filename);
				}

				Relation.SerializeUnfactoredToFile(_directoryLocations.UnfactoredProgress_Filename, Relations.UnFactored);
			}
		}

		private void SaveRoughRelations()
		{
			if (Relations.RoughRelations.Any())
			{
				if (File.Exists(_directoryLocations.RoughRelations_Filename))
				{
					File.Delete(_directoryLocations.RoughRelations_Filename);
				}

				// Write out RoughRelations file
				RoughPair.SaveToFile(_directoryLocations.RoughRelations_Filename, Relations.RoughRelations);
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

		public static PolyRelationsSieveProgress LoadProgress(GNFS gnfs)
		{
			int a = 0;
			int b = 1;
			int quantity = 200;
			int valueRange = 200;

			DirectoryLocations directoryLocations = gnfs.SaveLocations;

			if (Directory.Exists(directoryLocations.Relations_SaveDirectory))
			{
				if (File.Exists(directoryLocations.RelationProgress_Filename))
				{
					XElement xml = XElement.Load(directoryLocations.RelationProgress_Filename);
					a = int.Parse(xml.Element("A").Value);
					b = int.Parse(xml.Element("B").Value);
					quantity = int.Parse(xml.Element("Quantity").Value);
					valueRange = int.Parse(xml.Element("ValueRange").Value);
				}

				List<List<Relation>> freeRelations = new List<List<Relation>>();
				List<Relation> smoothRelations = new List<Relation>();
				List<RoughPair> roughRelations = new List<RoughPair>();
				List<Relation> unFactored = new List<Relation>();

				freeRelations = LoadFreeRelationSets(directoryLocations.FreeRelations_SaveDirectory);
				smoothRelations = LoadSmoothRelations(directoryLocations.SmoothRelations_SaveDirectory);

				if (File.Exists(directoryLocations.RoughRelations_Filename))
				{
					roughRelations = RoughPair.LoadFromFile(directoryLocations.RoughRelations_Filename);
				}
				else
				{
					roughRelations = new List<RoughPair>();
				}

				if (File.Exists(directoryLocations.UnfactoredProgress_Filename))
				{
					unFactored = Relation.LoadUnfactoredFile(gnfs, directoryLocations.UnfactoredProgress_Filename);
				}
				else
				{
					unFactored = new List<Relation>();
				}

				PolyRelationsSieveProgress result = null;
				result = new PolyRelationsSieveProgress(gnfs, a, b, quantity, valueRange, freeRelations, smoothRelations, roughRelations, unFactored);

				result.FreeRelationsDirectoryCounter = freeRelations.Count;

				return result;
			}
			else
			{
				return new PolyRelationsSieveProgress(gnfs, quantity, valueRange);
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
