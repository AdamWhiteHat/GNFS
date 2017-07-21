using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Threading;

using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public partial class GNFS : IXmlSerializable
	{
		public BigInteger N { get; set; }
		public AlgebraicPolynomial CurrentPolynomial { get; private set; }
		public List<AlgebraicPolynomial> PolynomialCollection;
		public PolyRelationsSieveProgress CurrentRelationsProgress { get; set; }

		public CancellationToken CancelToken { get; set; }

		public BigInteger PrimeBound { get; set; }
		public BigInteger MaxPrimeBound { get { return BigInteger.Max(BigInteger.Max(RationalFactorBase, AlgebraicFactorBase), QuadraticFactorBaseMax); } }

		public BigInteger RationalFactorBase { get; set; }
		public BigInteger AlgebraicFactorBase { get; set; }
		public BigInteger QuadraticFactorBaseMin { get; set; }
		public BigInteger QuadraticFactorBaseMax { get; set; }

		public List<BigInteger> RationalPrimeBase;
		public List<BigInteger> AlgebraicPrimeBase;
		public List<BigInteger> QuadraticPrimeBase;

		public FactorCollection RFB { get; set; } = null;
		public FactorCollection AFB { get; set; } = null;
		public FactorCollection QFB { get; set; } = null;


		internal string GNFS_SaveDirectory { get { return Path.Combine("C:\\GNFS", GetNumberFilename()); } }
		internal string GnfsParameters_SaveFile { get { return Path.Combine(GNFS_SaveDirectory, "_GNFS.Parameters"); } }

		internal string Polynomial_Filename { get { return "_Polynomial.Parameters"; } }
		internal string Polynomial_SaveDirectory { get { return GetPolynomialPath(CurrentPolynomial); } }
		internal string Relations_SaveDirectory { get { return Path.Combine(Polynomial_SaveDirectory, "SmoothRelations"); } }

		internal string RationalFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Rational.FactorBase"); } }
		internal string AlgebraicFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Algebraic.FactorBase"); } }
		internal string QuadradicFactorBase_SaveFile { get { return Path.Combine(Polynomial_SaveDirectory, "Quadradic.FactorBase"); } }

		private BigInteger m;
		private int degree = 2;

		private IEnumerable<BigInteger> _primes;

		public GNFS()
		{
			CancelToken = CancellationToken.None;
		}

		public GNFS(CancellationToken cancelToken, BigInteger n, BigInteger polynomialBase, int polyDegree = -1)
			: this()
		{
			CancelToken = cancelToken;
			N = n;
			PolynomialCollection = new List<AlgebraicPolynomial>();

			if (!Directory.Exists(GNFS_SaveDirectory))
			{
				// New GNFS instance
				Directory.CreateDirectory(GNFS_SaveDirectory);

				if (polyDegree == -1)
				{
					this.degree = CalculateDegree(n);
				}
				else
				{
					this.degree = polyDegree;
				}


				CaclulatePrimeBounds();
				ConstructNewPolynomial(polynomialBase, this.degree);
				m = polynomialBase;

				CurrentRelationsProgress = new PolyRelationsSieveProgress(this, CancelToken, Polynomial_SaveDirectory);

				SaveGnfsProgress();

				LoadFactorBases();
			}
			else
			{
				GNFS gnfs = (GNFS)Serializer.Deserialize(GnfsParameters_SaveFile, typeof(GNFS));
				LoadGnfsProgress(gnfs);
			}
		}

		public bool IsFactor(BigInteger toCheck)
		{
			return ((N % toCheck) == 0);
		}

		public void SaveGnfsProgress()
		{
			Serializer.Serialize(GnfsParameters_SaveFile, this);
		}

		public void LoadGnfsProgress(GNFS input)
		{
			N = input.N;
			m = input.m;
			int polyDegree = input.degree;
			PrimeBound = input.PrimeBound;
			RationalFactorBase = input.RationalFactorBase;
			AlgebraicFactorBase = input.AlgebraicFactorBase;
			QuadraticFactorBaseMin = input.QuadraticFactorBaseMin;
			QuadraticFactorBaseMax = input.QuadraticFactorBaseMax;

			if (Directory.Exists(GNFS_SaveDirectory))
			{
				IEnumerable<string> polynomialFiles = Directory.EnumerateFiles(GNFS_SaveDirectory, Polynomial_Filename, SearchOption.AllDirectories);
				if (polynomialFiles.Any())
				{
					foreach (string file in polynomialFiles)
					{
						AlgebraicPolynomial poly = (AlgebraicPolynomial)Serializer.Deserialize(file, typeof(AlgebraicPolynomial));
						PolynomialCollection.Add(poly);
					}

					PolynomialCollection = PolynomialCollection.OrderByDescending(ply => ply.Degree).ToList();
					CurrentPolynomial = PolynomialCollection.First();
				}
			}

			int base10 = N.ToString().Count();
			int quadraticBaseSize = CalculateQuadraticBaseSize(polyDegree);

			_primes = PrimeFactory.GetPrimes((RationalFactorBase * 3) + 2 + base10);
			RationalPrimeBase = PrimeFactory.GetPrimesTo(RationalFactorBase).ToList();
			AlgebraicPrimeBase = PrimeFactory.GetPrimesTo(AlgebraicFactorBase).ToList();
			QuadraticPrimeBase = PrimeFactory.GetPrimesFrom(QuadraticFactorBaseMin).Take(quadraticBaseSize).ToList();

			// Load FactorBases
			LoadFactorBases();

			// Load Relations
			CurrentRelationsProgress = PolyRelationsSieveProgress.LoadProgress(this, Polynomial_SaveDirectory);
		}


		private int showDigits = 22;
		private string elipse = "[...]";
		public string GetNumberFilename()
		{
			string result = N.ToString();

			if (result.Length >= (showDigits * 2) + elipse.Length)
			{
				result = result.Substring(0, showDigits) + elipse + result.Substring(result.Length - showDigits, showDigits);
			}
			return result;
		}

		//public void SaveAllPolynomials() { foreach (AlgebraicPolynomial poly in PolynomialCollection) { SavePolynomial(poly); } }

		private string GetPolynomialPath(AlgebraicPolynomial poly)
		{
			return Path.Combine(GNFS_SaveDirectory, $"Poly_B[{ poly.Base}]_D[{ poly.Degree}]");
		}

		public void SavePolynomial(AlgebraicPolynomial poly)
		{
			string polynomialDirectory = GetPolynomialPath(poly);
			if (!Directory.Exists(polynomialDirectory))
			{
				Directory.CreateDirectory(polynomialDirectory);
			}
			Serializer.Serialize(Path.Combine(polynomialDirectory, Polynomial_Filename), poly);
		}

		// Values were obtained from the paper:
		// "Polynomial Selection for the Number Field Sieve Integer factorization Algorithm" - by Brian Antony Murphy
		// Table 3.1, page 44
		private static int CalculateDegree(BigInteger n)
		{
			int result = 2;
			int base10 = n.ToString().Count();

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
			//BigInteger remainder = new BigInteger();
			int base10 = N.ToString().Count(); //N.NthRoot(10, ref remainder);

			if (base10 <= 18)
			{
				PrimeBound = base10 * 1000;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 <= 100)
			{
				PrimeBound = 100000;
			}
			else if (base10 > 100 && base10 <= 150)
			{
				PrimeBound = 250000;
			}
			else if (base10 > 150 && base10 <= 200)
			{
				PrimeBound = 125000000;
			}
			else if (base10 > 200)
			{
				PrimeBound = 250000000;
			}

			PrimeBound *= 3;

			RationalFactorBase = PrimeBound;

			_primes = PrimeFactory.GetPrimes((RationalFactorBase * 3) + 2 + base10);

			RationalPrimeBase = PrimeFactory.GetPrimesTo(RationalFactorBase).ToList();

			//int algebraicQuantity = RationalPrimeBase.Count() * 3;

			AlgebraicFactorBase = RationalFactorBase * 3;//PrimeFactory.GetValueFromIndex(algebraicQuantity); //(int)(PrimeBound * 1.1);
			QuadraticFactorBaseMin = AlgebraicFactorBase + 2;
			QuadraticFactorBaseMax = QuadraticFactorBaseMin + base10;

			AlgebraicPrimeBase = PrimeFactory.GetPrimesTo(AlgebraicFactorBase).ToList();

			int quadraticBaseSize = CalculateQuadraticBaseSize(degree);

			QuadraticPrimeBase = PrimeFactory.GetPrimesFrom(QuadraticFactorBaseMin).Take(quadraticBaseSize).ToList();
		}

		private static int CalculateQuadraticBaseSize(int polyDegree)
		{
			int result = -1;

			if (polyDegree < 3)
			{
				result = 10;
			}
			else if (polyDegree == 3 || polyDegree == 4)
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
			CurrentPolynomial = new AlgebraicPolynomial(N, polynomialBase, polyDegree);
			PolynomialCollection.Add(CurrentPolynomial);
			SavePolynomial(CurrentPolynomial);
		}

		private void LoadFactorBases()
		{
			if (!File.Exists(RationalFactorBase_SaveFile))
			{
				RFB = FactorCollection.Factory.BuildRationalFactorBase(this);
				FactorCollection.Serialize(RationalFactorBase_SaveFile, RFB);
			}
			else
			{
				RFB = FactorCollection.Deserialize(RationalFactorBase_SaveFile);
			}

			if (CancelToken.IsCancellationRequested) { return; }

			if (!File.Exists(AlgebraicFactorBase_SaveFile))
			{
				AFB = FactorCollection.Factory.GetAlgebraicFactorBase(this);
				FactorCollection.Serialize(AlgebraicFactorBase_SaveFile, AFB);
			}
			else
			{
				AFB = FactorCollection.Deserialize(AlgebraicFactorBase_SaveFile);
			}

			if (CancelToken.IsCancellationRequested) { return; }

			if (!File.Exists(QuadradicFactorBase_SaveFile))
			{
				QFB = FactorCollection.Factory.GetQuadradicFactorBase(this);
				FactorCollection.Serialize(QuadradicFactorBase_SaveFile, QFB);
			}
			else
			{
				QFB = FactorCollection.Deserialize(QuadradicFactorBase_SaveFile);
			}

			if (CancelToken.IsCancellationRequested) { return; }
		}

		public static List<RoughPair[]> GroupRoughNumbers(List<RoughPair> roughNumbers)
		{
			IEnumerable<RoughPair> input1 = roughNumbers.OrderBy(rp => rp.AlgebraicQuotient).ThenBy(rp => rp.RationalQuotient);
			IEnumerable<RoughPair> input2 = roughNumbers.OrderBy(rp => rp.RationalQuotient).ThenBy(rp => rp.AlgebraicQuotient);

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

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("N", N.ToString());
			writer.WriteElementString("M", m.ToString());
			writer.WriteElementString("Degree", this.degree.ToString());
			writer.WriteElementString("PrimeBound", PrimeBound.ToString());
			writer.WriteElementString("RationalFactorBase", RationalFactorBase.ToString());
			writer.WriteElementString("AlgebraicFactorBase", AlgebraicFactorBase.ToString());
			writer.WriteElementString("QuadraticFactorBaseMin", QuadraticFactorBaseMin.ToString());
			writer.WriteElementString("QuadraticFactorBaseMax", QuadraticFactorBaseMax.ToString());
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();

			string nString = reader.ReadElementString("N");
			string mString = reader.ReadElementString("M");
			string degreeString = reader.ReadElementString("Degree");
			string primeBoundString = reader.ReadElementString("PrimeBound");
			string rationalFactorBaseString = reader.ReadElementString("RationalFactorBase");
			string algebraicFactorBaseString = reader.ReadElementString("AlgebraicFactorBase");
			string quadraticFactorBaseMinString = reader.ReadElementString("QuadraticFactorBaseMin");
			string quadraticFactorBaseMaxString = reader.ReadElementString("QuadraticFactorBaseMax");

			reader.ReadEndElement();

			N = BigInteger.Parse(nString);
			m = BigInteger.Parse(mString);
			this.degree = int.Parse(degreeString);
			PrimeBound = int.Parse(primeBoundString);
			RationalFactorBase = int.Parse(rationalFactorBaseString);
			AlgebraicFactorBase = int.Parse(algebraicFactorBaseString);
			QuadraticFactorBaseMin = int.Parse(quadraticFactorBaseMinString);
			QuadraticFactorBaseMax = int.Parse(quadraticFactorBaseMaxString);
		}

		public XmlSchema GetSchema() { return null; }
	}
}
