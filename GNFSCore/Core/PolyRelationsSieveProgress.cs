using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using GNFSCore.IntegerMath;
using System.Xml.Linq;

namespace GNFSCore
{
	public class PolyRelationsSieveProgress
	{

		private int B;
		private int Quantity;
		private int ValueRange;
		//private int IndexOfUnFactored;
		public CancellationToken CancelToken;

		public List<Relation> SmoothRelations { get; private set; }
		public List<RoughPair> RoughRelations { get; private set; }
		public List<Relation> UnFactored { get; private set; }

		internal string Polynomial_SaveDirectory { get; private set; }
		public string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "Relations"); } }
		internal string RelationProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Relations.Progress"); } }
		internal string UnfactoredProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Unfactored.relations"); } }
		internal string RoughRelations_Filename { get { return Path.Combine(Relations_SaveDirectory, "Rough.relations"); } }
		internal string SmoothRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "SmoothRelations"); } }

		public PolyRelationsSieveProgress(CancellationToken cancelToken, string polynomialSaveDirectory)
		{
			CancelToken = cancelToken;
			Polynomial_SaveDirectory = polynomialSaveDirectory;

			B = 1;
			Quantity = 100;
			ValueRange = 100;
			SmoothRelations = new List<Relation>();
			RoughRelations = new List<RoughPair>();
			UnFactored = new List<Relation>();
		}

		public PolyRelationsSieveProgress(string polynomialSaveDirectory, int b, int quantity, int valueRange, List<Relation> smooth, List<RoughPair> rough, List<Relation> unfactored)
		{
			B = b;
			Quantity = quantity;
			ValueRange = valueRange;
			SmoothRelations = smooth;
			RoughRelations = rough;
			UnFactored = unfactored;
			Polynomial_SaveDirectory = polynomialSaveDirectory;
		}

		public List<Relation> GenerateRelations(GNFS gnfs, CancellationToken cancelToken)
		{
			if (Quantity == -1)
			{
				Quantity = gnfs.RFB.Count + gnfs.AFB.Count + gnfs.QFB.Count + 1;
			}
			else if (SmoothRelations.Count() >= Quantity)
			{
				Quantity += 200;
			}

			if (B >= ValueRange)
			{
				ValueRange += 200;
			}

			int adjustedRange = ValueRange % 2 == 0 ? ValueRange + 1 : ValueRange;
			IEnumerable<int> A = Enumerable.Range(-adjustedRange, adjustedRange * 2);
			int maxB = Math.Max(adjustedRange, Quantity) + 2;

			List<Relation> newestRelations = new List<Relation>();

			while (SmoothRelations.Count() < Quantity)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				IEnumerable<int> coprimes = A.Where(a => CoPrime.IsCoprime(a, B));
				IEnumerable<Relation> unfactored = coprimes.Select(a => new Relation(gnfs, a, B));

				newestRelations.AddRange(SieveRelations(gnfs.CurrentRelationsProgress, unfactored));

				if (B > maxB)
				{
					break;
				}

				B += 2;
			}

			SaveProgress();
			return newestRelations;
		}

		public static List<Relation> SieveRelations(PolyRelationsSieveProgress relationsProgress, IEnumerable<Relation> unfactored)
		{
			Tuple<List<Relation>, List<RoughPair>> result = SieveRelations2(relationsProgress, unfactored);

			if (result.Item1.Any())
			{
				relationsProgress.SmoothRelations.AddRange(result.Item1);
			}
			if (result.Item2.Any())
			{
				relationsProgress.RoughRelations.AddRange(result.Item2);
			}

			return result.Item1;
		}

		// Tuple<SmoothRelations, RoughRelations>
		public static Tuple<List<Relation>, List<RoughPair>> SieveRelations2(PolyRelationsSieveProgress relationsProgress, IEnumerable<Relation> unfactored)
		{
			//IndexOfUnFactored = 0;
			List<Relation> smoothRelations = new List<Relation>();
			List<RoughPair> roughRelations = new List<RoughPair>();
			foreach (Relation rel in unfactored)
			{
				if (relationsProgress.CancelToken.IsCancellationRequested)
				{
					return new Tuple<List<Relation>, List<RoughPair>>(smoothRelations, roughRelations);
				}

				rel.Sieve();
				bool smooth = rel.IsSmooth;
				if (smooth)
				{
					smoothRelations.Add(rel);
					rel.Save(relationsProgress.Relations_SaveDirectory);
				}
				else
				{
					roughRelations.Add(new RoughPair(rel));
				}

				//IndexOfUnFactored++;
			}
			return new Tuple<List<Relation>, List<RoughPair>>(smoothRelations, roughRelations);
		}

		public void SaveProgress()
		{
			//Serializer.Serialize(RelationProgress_Filename, this);

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

			if (SmoothRelations.Any())
			{
				// Create SmoothRelations directory
				if (!Directory.Exists(SmoothRelations_SaveDirectory))
				{
					Directory.CreateDirectory(SmoothRelations_SaveDirectory);
				}

				// Write out SmoothRelations
				foreach (Relation rel in SmoothRelations)
				{
					string filename = $"{SmoothRelations_SaveDirectory}\\{rel.A}_{rel.B}.relation";
					if (!File.Exists(filename))
					{
						rel.Save(filename);
					}
				}
			}

			if (RoughRelations.Any())
			{
				// Remove previous RoughRelations file
				if (File.Exists(RoughRelations_Filename))
				{
					File.Delete(RoughRelations_Filename);
				}

				// Write out RoughRelations file
				RoughPair.SaveToFile(RoughRelations_Filename, RoughRelations);

				if (File.Exists(UnfactoredProgress_Filename))
				{
					File.Delete(UnfactoredProgress_Filename);
				}
			}

			if (UnFactored.Any())
			{
				Relation.SerializeUnfactoredToFile(UnfactoredProgress_Filename, UnFactored);
			}
		}


		public static PolyRelationsSieveProgress LoadProgress(GNFS gnfs, string polynomialSaveDirectory)
		{
			string directoryRelations = Path.Combine(polynomialSaveDirectory, "Relations");
			string filenameRelationProgress = Path.Combine(directoryRelations, "Relations.Progress");
			string filenameUnfactoredProgress = Path.Combine(filenameRelationProgress, "Unfactored.relations");
			string filenameRoughRelations = Path.Combine(filenameUnfactoredProgress, "Rough.relations");

			int b = 1;
			int quantity = 100;
			int valueRange = 200;

			if (File.Exists(filenameRelationProgress))
			{
				XElement xml = XElement.Load(filenameRelationProgress);
				b = int.Parse(xml.Element("B").Value);
				quantity = int.Parse(xml.Element("Quantity").Value);
				valueRange = int.Parse(xml.Element("ValueRange").Value);
			}

			List<Relation> smoothRelations = new List<Relation>();
			List<RoughPair> roughRelations = new List<RoughPair>();
			List<Relation> unFactored = new List<Relation>();

			if (Directory.Exists(directoryRelations))
			{
				IEnumerable<string> relationFiles = Directory.EnumerateFiles(directoryRelations, "*.relation");
				smoothRelations = relationFiles.Select(fn => Relation.LoadFromFile(fn)).ToList();
			}
			else
			{
				smoothRelations = new List<Relation>();
			}

			if (File.Exists(filenameRoughRelations))
			{
				roughRelations = RoughPair.LoadFromFile(filenameRoughRelations);
			}
			else
			{
				roughRelations = new List<RoughPair>();
			}

			if (File.Exists(filenameUnfactoredProgress))
			{
				unFactored = Relation.LoadUnfactoredFile(gnfs, filenameUnfactoredProgress);
			}
			else
			{
				unFactored = new List<Relation>();
			}

			return new PolyRelationsSieveProgress(polynomialSaveDirectory, b, quantity, valueRange, smoothRelations, roughRelations, unFactored);
		}
	}
}
