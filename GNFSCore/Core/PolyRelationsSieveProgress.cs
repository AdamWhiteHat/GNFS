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

		public List<BigInteger> AlgebraicPrimeBase;
		public List<BigInteger> RationalPrimeBase;

		internal string Polynomial_SaveDirectory { get; private set; }
		public string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "Relations"); } }
		internal string RelationProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Relations.Progress"); } }
		internal string UnfactoredProgress_Filename { get { return Path.Combine(Relations_SaveDirectory, "Unfactored.relations"); } }
		internal string RoughRelations_Filename { get { return Path.Combine(Relations_SaveDirectory, "Rough.relations"); } }
		internal string SmoothRelations_SaveDirectory { get { return Path.Combine(Relations_SaveDirectory, "SmoothRelations"); } }

		public PolyRelationsSieveProgress(GNFS gnfs, CancellationToken cancelToken, string polynomialSaveDirectory)
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

			

			B = 1;
			Quantity = 200;
			ValueRange = 2000;
			SmoothRelations = new List<Relation>();
			RoughRelations = new List<RoughPair>();
			UnFactored = new List<Relation>();

			AlgebraicPrimeBase = gnfs.AlgebraicPrimeBase;
			RationalPrimeBase = gnfs.RationalPrimeBase;
		}

		public PolyRelationsSieveProgress(GNFS gnfs, string polynomialSaveDirectory, int b, int quantity, int valueRange, List<Relation> smooth, List<RoughPair> rough, List<Relation> unfactored)
		{
			B = b;
			Quantity = quantity;
			ValueRange = valueRange;
			SmoothRelations = smooth;
			RoughRelations = rough;
			UnFactored = unfactored;
			Polynomial_SaveDirectory = polynomialSaveDirectory;

			AlgebraicPrimeBase = gnfs.AlgebraicPrimeBase;
			RationalPrimeBase = gnfs.RationalPrimeBase;

			CancellationTokenSource cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(gnfs.CancelToken);
			CancelToken = cancellationSource.Token;
		}

		public List<Relation> GenerateRelations(GNFS gnfs, CancellationToken cancelToken)
		{
			if (Quantity == -1)
			{
				Quantity = gnfs.RFB.Count + gnfs.AFB.Count + gnfs.QFB.Count + 1;
			}
			else if (SmoothRelations.Count() >= Quantity)
			{
				Quantity += 2000;
			}

			if (B >= ValueRange)
			{
				return null;
				//ValueRange += 2000;
			}

			int adjustedRange = ValueRange % 2 == 0 ? ValueRange + 1 : ValueRange;
			IEnumerable<int> A = Enumerable.Range(-adjustedRange, adjustedRange * 2);
			int maxB = Math.Max(adjustedRange * 2, Quantity) + 2;

			List<Relation> newestRelations = new List<Relation>();

			while (SmoothRelations.Count() < Quantity)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				List<int> coprimes = A.Where(a => CoPrime.IsCoprime(a, B)).ToList();
				List<Relation> unfactored = coprimes.Select(a => new Relation(gnfs, a, B)).ToList();

				newestRelations.AddRange(SieveRelations(cancelToken, gnfs.CurrentRelationsProgress, unfactored));

				if (B > maxB)
				{
					break;
				}

				B += 2;
			}

			SaveProgress();
			return newestRelations;
		}

		// Tuple<SmoothRelations, RoughRelations>
		public static List<Relation> SieveRelations(CancellationToken cancelToken, PolyRelationsSieveProgress relationsProgress, IEnumerable<Relation> unfactored)
		{
			//IndexOfUnFactored = 0;
			List<Relation> smoothRelations = new List<Relation>();
			List<RoughPair> roughRelations = new List<RoughPair>();
			foreach (Relation rel in unfactored)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				rel.Sieve(relationsProgress);
				bool smooth = rel.IsSmooth;
				if (smooth)
				{
					smoothRelations.Add(rel);
					rel.Save($"{relationsProgress.SmoothRelations_SaveDirectory}\\{rel.A}_{rel.B}.relation");
				}
				else
				{
					roughRelations.Add(new RoughPair(rel));
				}
				//IndexOfUnFactored++;
			}

			if (smoothRelations.Any())
			{
				relationsProgress.SmoothRelations.AddRange(smoothRelations);
			}
			if (roughRelations.Any())
			{
				relationsProgress.RoughRelations.AddRange(roughRelations);
			}

			return new List<Relation>(smoothRelations);
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
			string filenameUnfactoredProgress = Path.Combine(directoryRelations, "Unfactored.relations");
			string filenameRoughRelations = Path.Combine(directoryRelations, "Rough.relations");

			int b = 1;
			int quantity = 100;
			int valueRange = 200;

			if (Directory.Exists(directoryRelations))
			{
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


				IEnumerable<string> relationFiles = Directory.EnumerateFiles(directoryRelations, "*.relation");
				smoothRelations = relationFiles.Select(fn => Relation.LoadFromFile(fn)).ToList();

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

				return new PolyRelationsSieveProgress(gnfs, polynomialSaveDirectory, b, quantity, valueRange, smoothRelations, roughRelations, unFactored);
			}
			else
			{
				return new PolyRelationsSieveProgress(gnfs, gnfs.CancelToken, gnfs.Polynomial_SaveDirectory);
			}




		}
	}
}
