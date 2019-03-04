using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace GNFSCore
{
	using Factors;
	using Polynomials;
	using IntegerMath;

	public partial class GNFS : IXmlSerializable
	{
		public BigInteger N { get; set; }
		public List<IPolynomial> PolynomialCollection;
		public IPolynomial CurrentPolynomial { get; private set; }
		public PolyRelationsSieveProgress CurrentRelationsProgress { get; set; }
		public CancellationToken CancelToken { get; set; }

		public FactorBase PrimeFactorBase { get; set; }

		/// <summary>
		/// Array of (p, m % p)
		/// </summary>
		public FactorCollection RFB { get; set; } = null;

		/// <summary>
		/// Array of (p, r) where ƒ(r) % p == 0
		/// </summary>
		public FactorCollection AFB { get; set; } = null;
		public FactorCollection QFB { get; set; } = null;

		public DirectoryLocations SaveLocations { get; private set; }

		public BigInteger PolynomialBase { get; private set; }
		public int PolynomialDegree { get; private set; }

		public GNFS()
		{
			PrimeFactorBase = new FactorBase();
			PolynomialCollection = new List<IPolynomial>();
		}

		public GNFS(CancellationToken cancelToken, string openDirectory)
			: this()
		{
			CancelToken = cancelToken;
			SaveLocations = new DirectoryLocations(openDirectory);

			if (!Directory.Exists(SaveLocations.SaveDirectory))
			{
				throw new ArgumentException($"Parameter {nameof(openDirectory)} must be a directory, that directory must exist and must contain the file {SaveLocations.GnfsParameters_SaveFile}");
			}

			if (!File.Exists(SaveLocations.GnfsParameters_SaveFile))
			{
				throw new FileNotFoundException($"File does not exist: \"{SaveLocations.GnfsParameters_SaveFile}\"!");
			}

			GNFS gnfs = (GNFS)Serializer.Deserialize(SaveLocations.GnfsParameters_SaveFile, typeof(GNFS));
			LoadGnfsProgress(gnfs);
		}

		public GNFS(CancellationToken cancelToken, BigInteger n, BigInteger polynomialBase, int polyDegree, BigInteger primeBound, int relationQuantity, int relationValueRange)
			: this()
		{
			CancelToken = cancelToken;
			N = n;

			SaveLocations = new DirectoryLocations(N, polynomialBase, polyDegree);

			if (!Directory.Exists(SaveLocations.SaveDirectory))
			{
				// New GNFS instance
				Directory.CreateDirectory(SaveLocations.SaveDirectory);

				if (polyDegree == -1)
				{
					this.PolynomialDegree = CalculateDegree(n);
				}
				else
				{
					this.PolynomialDegree = polyDegree;
				}

				this.PolynomialBase = polynomialBase;

				CaclulatePrimeBounds(primeBound);
				ConstructNewPolynomial(this.PolynomialBase, this.PolynomialDegree);				

				CurrentRelationsProgress = new PolyRelationsSieveProgress(this, relationQuantity, relationValueRange);

				SaveGnfsProgress();

				LoadFactorBases();
			}
			else
			{
				GNFS gnfs = (GNFS)Serializer.Deserialize(SaveLocations.GnfsParameters_SaveFile, typeof(GNFS));
				LoadGnfsProgress(gnfs);
			}
		}

		public GNFS(CancellationToken cancelToken, BigInteger n)
			: this()
		{
			CancelToken = cancelToken;
			N = n;

			SaveLocations = new DirectoryLocations(N);

			if (Directory.Exists(SaveLocations.SaveDirectory))
			{
				GNFS gnfs = (GNFS)Serializer.Deserialize(SaveLocations.GnfsParameters_SaveFile, typeof(GNFS));
				LoadGnfsProgress(gnfs);
			}
			else
			{
				throw new Exception($"Directory does not exists: \"{SaveLocations.SaveDirectory}\". Only call this constructor to load previously saved factoring job.");
			}
		}

		public bool IsFactor(BigInteger toCheck)
		{
			return ((N % toCheck) == 0);
		}

		public void SaveGnfsProgress()
		{
			Serializer.Serialize(SaveLocations.GnfsParameters_SaveFile, this);
			SavePolynomial(CurrentPolynomial, PolynomialBase);
			CurrentRelationsProgress.SaveProgress();
		}

		public void LoadGnfsProgress(GNFS input)
		{
			N = input.N;
			PolynomialBase = input.PolynomialBase;
			PolynomialDegree = input.PolynomialDegree;

			PrimeFactorBase.MaxRationalFactorBase = input.PrimeFactorBase.MaxRationalFactorBase;
			PrimeFactorBase.MaxAlgebraicFactorBase = input.PrimeFactorBase.MaxAlgebraicFactorBase;
			PrimeFactorBase.MinQuadraticFactorBase = input.PrimeFactorBase.MinQuadraticFactorBase;
			PrimeFactorBase.MaxQuadraticFactorBase = input.PrimeFactorBase.MaxQuadraticFactorBase;
			PrimeFactorBase.QuadraticBaseSize = input.PrimeFactorBase.QuadraticBaseSize;
			
			PrimeFactory.IncreaseMaxValue(PrimeFactorBase.MaxQuadraticFactorBase);
			PrimeFactorBase.RationalFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxRationalFactorBase).ToList();
			PrimeFactorBase.AlgebraicFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxAlgebraicFactorBase).ToList();
			PrimeFactorBase.QuadraticFactorBase = PrimeFactory.GetPrimesFrom(PrimeFactorBase.MinQuadraticFactorBase).Take(PrimeFactorBase.QuadraticBaseSize).ToList();

			// Load Polynomial
			if (Directory.Exists(SaveLocations.SaveDirectory))
			{
				IEnumerable<string> polynomialFiles = Directory.EnumerateFiles(SaveLocations.SaveDirectory, SaveLocations.Polynomial_Filename, SearchOption.AllDirectories);
				if (polynomialFiles.Any())
				{
					foreach (string file in polynomialFiles)
					{
						Polynomial poly = (Polynomial)Serializer.Deserialize(file, typeof(Polynomial));
						PolynomialCollection.Add(poly);
					}

					PolynomialCollection = PolynomialCollection.OrderByDescending(ply => ply.Degree).ToList();
					CurrentPolynomial = PolynomialCollection.First();
					PolynomialDegree = CurrentPolynomial.Degree;
				}
			}

			// Load FactorBases
			LoadFactorBases();

			// Load Relations
			CurrentRelationsProgress = PolyRelationsSieveProgress.LoadProgress(this);
		}

		public void SavePolynomial(IPolynomial poly, BigInteger polynomialBase)
		{
			SaveLocations.SetPolynomialPath(polynomialBase, poly.Degree);

			if (!Directory.Exists(SaveLocations.Polynomial_SaveDirectory))
			{
				Directory.CreateDirectory(SaveLocations.Polynomial_SaveDirectory);
			}
			Serializer.Serialize(SaveLocations.Polynomial_SaveFile, poly);
		}

		// Values were obtained from the paper:
		// "Polynomial Selection for the Number Field Sieve Integer factorization Algorithm" - by Brian Antony Murphy
		// Table 3.1, page 44
		private static int CalculateDegree(BigInteger n)
		{
			int result = 2;
			int base10 = n.ToString().Length;

			if (base10 < 65)
			{
				result = 3;
			}
			else if (base10 >= 65 && base10 < 125)
			{
				result = 4;
			}
			else if (base10 >= 125 && base10 < 225)
			{
				result = 5;
			}
			else if (base10 >= 225 && base10 < 315)
			{
				result = 6;
			}
			else if (base10 >= 315)
			{
				result = 7;
			}

			return result;
		}

		private void CaclulatePrimeBounds()
		{
			BigInteger bound = new BigInteger();

			int base10 = N.ToString().Length; //N.NthRoot(10, ref remainder);
			if (base10 <= 10)
			{
				bound = 100;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 <= 18)
			{
				bound = base10 * 1000;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 <= 100)
			{
				bound = 100000;
			}
			else if (base10 > 100 && base10 <= 150)
			{
				bound = 250000;
			}
			else if (base10 > 150 && base10 <= 200)
			{
				bound = 125000000;
			}
			else if (base10 > 200)
			{
				bound = 250000000;
			}

			CaclulatePrimeBounds(bound);
		}

		private void CaclulatePrimeBounds(BigInteger bound)
		{
			PrimeFactorBase = new FactorBase();

			PrimeFactorBase.MaxRationalFactorBase = bound;
			PrimeFactorBase.MaxAlgebraicFactorBase = (PrimeFactorBase.MaxRationalFactorBase) * 3;

			PrimeFactorBase.QuadraticBaseSize = CalculateQuadraticBaseSize(PolynomialDegree);

			PrimeFactorBase.MinQuadraticFactorBase = PrimeFactorBase.MaxAlgebraicFactorBase + 20;
			PrimeFactorBase.MaxQuadraticFactorBase = PrimeFactory.GetApproximateValueFromIndex((UInt64)(PrimeFactorBase.MinQuadraticFactorBase + PrimeFactorBase.QuadraticBaseSize));
			
			PrimeFactory.IncreaseMaxValue(PrimeFactorBase.MaxQuadraticFactorBase);
			
			PrimeFactorBase.RationalFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxRationalFactorBase).ToList();
			PrimeFactorBase.AlgebraicFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxAlgebraicFactorBase).ToList();
			PrimeFactorBase.QuadraticFactorBase = PrimeFactory.GetPrimesFrom(PrimeFactorBase.MinQuadraticFactorBase).Take(PrimeFactorBase.QuadraticBaseSize).ToList();
		}

		private static int CalculateQuadraticBaseSize(int polyDegree)
		{
			int result = -1;

			if (polyDegree <= 3)
			{
				result = 10;
			}
			else if (polyDegree == 4)
			{
				result = 20;
			}
			else if (polyDegree == 5 || polyDegree == 6)
			{
				result = 40;
			}
			else if (polyDegree == 7)
			{
				result = 80;
			}
			else if (polyDegree >= 8)
			{
				result = 100;
			}

			return result;
		}

		private void ConstructNewPolynomial(BigInteger polynomialBase, int polyDegree)
		{
			CurrentPolynomial = new Polynomial(N, polynomialBase, polyDegree);
			PolynomialCollection.Add(CurrentPolynomial);
			SavePolynomial(CurrentPolynomial, polynomialBase);
		}

		private void LoadFactorBases()
		{
			if (!File.Exists(SaveLocations.RationalFactorBase_SaveFile))
			{
				RFB = FactorCollection.Factory.BuildRationalFactorBase(this);
				FactorCollection.Serialize(SaveLocations.RationalFactorBase_SaveFile, RFB);
			}
			else
			{
				RFB = FactorCollection.Deserialize(SaveLocations.RationalFactorBase_SaveFile);
			}

			if (CancelToken.IsCancellationRequested) { return; }

			if (!File.Exists(SaveLocations.AlgebraicFactorBase_SaveFile))
			{
				AFB = FactorCollection.Factory.BuildAlgebraicFactorBase(this);
				FactorCollection.Serialize(SaveLocations.AlgebraicFactorBase_SaveFile, AFB);
			}
			else
			{
				AFB = FactorCollection.Deserialize(SaveLocations.AlgebraicFactorBase_SaveFile);
			}

			if (CancelToken.IsCancellationRequested) { return; }

			if (!File.Exists(SaveLocations.QuadradicFactorBase_SaveFile))
			{
				QFB = FactorCollection.Factory.BuildQuadradicFactorBase(this);
				FactorCollection.Serialize(SaveLocations.QuadradicFactorBase_SaveFile, QFB);
			}
			else
			{
				QFB = FactorCollection.Deserialize(SaveLocations.QuadradicFactorBase_SaveFile);
			}

			if (CancelToken.IsCancellationRequested) { return; }
		}

		public static List<RoughPair[]> GroupRoughNumbers(List<RoughPair> roughNumbers)
		{
			IEnumerable<RoughPair> input1 = roughNumbers.OrderBy(rp => rp.AlgebraicQuotient).ThenBy(rp => rp.RationalQuotient);
			//IEnumerable<RoughPair> input2 = roughNumbers.OrderBy(rp => rp.RationalQuotient).ThenBy(rp => rp.AlgebraicQuotient);

			RoughPair lastItem = null;
			List<RoughPair[]> results = new List<RoughPair[]>();
			foreach (RoughPair pair in input1)
			{
				if (lastItem == null)
				{
					lastItem = pair;
				}
				else if (pair.AlgebraicQuotient == lastItem.AlgebraicQuotient && pair.RationalQuotient == lastItem.RationalQuotient)
				{
					results.Add(new RoughPair[] { pair, lastItem });
					lastItem = null;
				}
				else
				{
					lastItem = pair;
				}
			}

			return results;
		}

		public static List<Relation> MultiplyLikeRoughNumbers(GNFS gnfs, List<RoughPair[]> likeRoughNumbersGroups)
		{
			List<Relation> result = new List<Relation>();

			foreach (RoughPair[] likePair in likeRoughNumbersGroups)
			{
				var As = likePair.Select(lp => lp.A).ToList();
				var Bs = likePair.Select(lp => lp.B).ToList();

				int a = (As[0] + Bs[0]) * (As[0] - Bs[0]);//(int)Math.Round(Math.Sqrt(As.Sum()));
				int b = (As[1] + Bs[1]) * (As[1] - Bs[1]);//(int)Math.Round(Math.Sqrt(Bs.Sum()));

				if (a > 0 && b > 0)
				{
					result.Add(new Relation(gnfs, a, b));
				}
			}

			return result;
		}

		#region ToString

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine($"N = {N}");
			result.AppendLine();
			result.AppendLine($"Polynomial(degree: {PolynomialDegree}, base: {PolynomialBase}):");
			result.AppendLine("ƒ(m) = " + CurrentPolynomial.ToString());
			result.AppendLine();
			result.AppendLine("Prime Factor Base Bounds:");
			result.AppendLine($"RationalFactorBase : {PrimeFactorBase.MaxRationalFactorBase}");
			result.AppendLine($"AlgebraicFactorBase: {PrimeFactorBase.MaxAlgebraicFactorBase}");
			result.AppendLine($"QuadraticPrimeBase Range: {PrimeFactorBase.MinQuadraticFactorBase} - {PrimeFactorBase.MaxQuadraticFactorBase}");
			result.AppendLine($"QuadraticPrimeBase Count: {PrimeFactorBase.QuadraticFactorBase.Count}");
			result.AppendLine();
			result.AppendLine($"RFB - Rational Factor Base - Count: {RFB.Count} - Array of (p, m % p) with prime p");
			result.AppendLine(RFB.ToString(200));
			result.AppendLine();
			result.AppendLine($"AFB - Algebraic Factor Base - Count: {AFB.Count} - Array of (p, r) such that ƒ(r) ≡ 0 (mod p) and p is prime");
			result.AppendLine(AFB.ToString(200));
			result.AppendLine();
			result.AppendLine($"QFB - Quadratic Factor Base - Count: {QFB.Count} - Array of (p, r) such that ƒ(r) ≡ 0 (mod p) and p is prime");
			result.AppendLine(QFB.ToString());
			result.AppendLine();

			result.AppendLine();
			result.AppendLine();

			return result.ToString();
		}

		#endregion

		#region IXmlSerializable

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("N", N.ToString());
			writer.WriteElementString("M", PolynomialBase.ToString());
			writer.WriteElementString("Degree", this.PolynomialDegree.ToString());
			writer.WriteElementString("RationalFactorBase", PrimeFactorBase.MaxRationalFactorBase.ToString());
			writer.WriteElementString("AlgebraicFactorBase", PrimeFactorBase.MaxAlgebraicFactorBase.ToString());			
			writer.WriteElementString("QuadraticFactorBaseMin", PrimeFactorBase.MinQuadraticFactorBase.ToString());
			writer.WriteElementString("QuadraticFactorBaseMax", PrimeFactorBase.MaxQuadraticFactorBase.ToString());
			writer.WriteElementString("QuadraticBaseSize", PrimeFactorBase.QuadraticBaseSize.ToString());
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();

			string nString = reader.ReadElementString("N");
			string mString = reader.ReadElementString("M");
			string degreeString = reader.ReadElementString("Degree");
			string rationalFactorBaseString = reader.ReadElementString("RationalFactorBase");
			string algebraicFactorBaseString = reader.ReadElementString("AlgebraicFactorBase");
			string quadraticFactorBaseMinString = reader.ReadElementString("QuadraticFactorBaseMin");
			string quadraticFactorBaseMaxString = reader.ReadElementString("QuadraticFactorBaseMax");
			string quadraticBaseSizeString = reader.ReadElementString("QuadraticBaseSize");

			reader.ReadEndElement();

			N = BigInteger.Parse(nString);
			PolynomialBase = BigInteger.Parse(mString);
			this.PolynomialDegree = int.Parse(degreeString);
			PrimeFactorBase.MaxRationalFactorBase = int.Parse(rationalFactorBaseString);
			PrimeFactorBase.MaxAlgebraicFactorBase = int.Parse(algebraicFactorBaseString);
			PrimeFactorBase.MinQuadraticFactorBase = int.Parse(quadraticFactorBaseMinString);
			PrimeFactorBase.MaxQuadraticFactorBase = int.Parse(quadraticFactorBaseMaxString);
			PrimeFactorBase.QuadraticBaseSize = int.Parse(quadraticBaseSizeString);
		}

		public XmlSchema GetSchema() { return null; }

		#endregion

	}
}
