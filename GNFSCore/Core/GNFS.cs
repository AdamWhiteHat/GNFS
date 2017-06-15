using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Threading;

namespace GNFSCore
{
	public partial class GNFS : IXmlSerializable
	{
		public BigInteger N { get; set; }
		public List<Relation> Relations { get; private set; }
		public AlgebraicPolynomial CurrentPolynomial { get; private set; }
		public List<AlgebraicPolynomial> PolynomialCollection;

		public CancellationToken CancelToken { get; set; }

		public int PrimeBound { get; set; }
		public int MaxPrimeBound { get { return Math.Max(Math.Max(RationalFactorBase, AlgebraicFactorBase), QuadraticFactorBaseMax); } }

		public int RationalFactorBase { get; set; }
		public int AlgebraicFactorBase { get; set; }
		public int QuadraticFactorBaseMin { get; set; }
		public int QuadraticFactorBaseMax { get; set; }

		public IEnumerable<int> RationalPrimeBase;
		public IEnumerable<int> AlgebraicPrimeBase;
		public IEnumerable<int> QuadraticPrimeBase;

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
		private int b = 1;
		private int valueRange = 200;
		private int quantity = 200;
		private int degree = 2;

		private int[] _primes;

		public GNFS()
		{
			CancelToken = CancellationToken.None;
		}

		public GNFS(CancellationToken cancelToken, BigInteger n, BigInteger polynomialBase, int polyDegree = -1)
			: this()
		{
			CancelToken = cancelToken;
			N = n;
			Relations = new List<Relation>();
			PolynomialCollection = new List<AlgebraicPolynomial>();

			if (!Directory.Exists(GNFS_SaveDirectory))
			{
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
				Directory.CreateDirectory(Relations_SaveDirectory);

				m = polynomialBase;

				// Save GNFS settings
				SaveGnfsProgress();

				LoadFactorBases();
			}
			else
			{
				// Load GNFS settings
				GNFS gnfs = (GNFS)Serializer.Deserialize(GnfsParameters_SaveFile, typeof(GNFS));

				N = gnfs.N;
				m = gnfs.m;
				b = gnfs.b;
				polyDegree = gnfs.degree;
				quantity = gnfs.quantity;
				valueRange = gnfs.valueRange;
				PrimeBound = gnfs.PrimeBound;
				RationalFactorBase = gnfs.RationalFactorBase;
				AlgebraicFactorBase = gnfs.AlgebraicFactorBase;
				QuadraticFactorBaseMin = gnfs.QuadraticFactorBaseMin;
				QuadraticFactorBaseMax = gnfs.QuadraticFactorBaseMax;

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
				RationalPrimeBase = PrimeFactory.GetPrimesTo(RationalFactorBase);
				AlgebraicPrimeBase = PrimeFactory.GetPrimesTo(AlgebraicFactorBase);
				QuadraticPrimeBase = PrimeFactory.GetPrimesFrom(QuadraticFactorBaseMin).Take(quadraticBaseSize);

				// Load FactorBases
				LoadFactorBases();

				// Load Relations
				Relations = LoadRelations(Relations_SaveDirectory);
			}
		}

		public bool IsFactor(BigInteger toCheck)
		{
			return (N % toCheck == 0);
		}

		public void SaveGnfsProgress()
		{
			Serializer.Serialize(GnfsParameters_SaveFile, this);
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
		// "Polynomial Selection for the Number Field Sieve Integer Factorisation Algorithm" - by Brian Antony Murphy
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
				PrimeBound = base10 * 10;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 < 100)
			{
				PrimeBound = 100000;
			}
			else if (base10 > 100 && base10 < 150)
			{
				PrimeBound = 250000;
			}
			else if (base10 > 150 && base10 < 200)
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

			RationalPrimeBase = PrimeFactory.GetPrimesTo(RationalFactorBase);

			//int algebraicQuantity = RationalPrimeBase.Count() * 3;

			AlgebraicFactorBase = RationalFactorBase * 3;//PrimeFactory.GetValueFromIndex(algebraicQuantity); //(int)(PrimeBound * 1.1);
			QuadraticFactorBaseMin = AlgebraicFactorBase + 2;
			QuadraticFactorBaseMax = QuadraticFactorBaseMin + base10;

			AlgebraicPrimeBase = PrimeFactory.GetPrimesTo(AlgebraicFactorBase);

			int quadraticBaseSize = CalculateQuadraticBaseSize(degree);

			QuadraticPrimeBase = PrimeFactory.GetPrimesFrom(QuadraticFactorBaseMin).Take(quadraticBaseSize);
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

		private static List<Relation> LoadRelations(string saveDirectory)
		{
			List<Relation> result = new List<Relation>();
			// Load Relations
			if (Directory.Exists(saveDirectory))
			{
				IEnumerable<string> relationFiles = Directory.EnumerateFiles(saveDirectory, "*.relation");
				if (relationFiles.Any())
				{
					foreach (string file in relationFiles)
					{
						Relation relation = (Relation)Serializer.Deserialize(file, typeof(Relation));
						result.Add(relation);
					}
				}
			}
			return result;
		}

		public List<Relation> GenerateRelations()
		{
			CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
			return GenerateRelations(cancelTokenSource.Token);
		}

		public List<Relation> GenerateRelations(CancellationToken cancelToken)
		{
			if (quantity == -1)
			{
				quantity = RFB.Count + AFB.Count + QFB.Count + 1;
			}
			else if (Relations.Count() >= quantity)
			{
				quantity += 200;
			}

			if (b >= valueRange)
			{
				valueRange += 200;
			}

			int adjustedRange = valueRange % 2 == 0 ? valueRange + 1 : valueRange;
			IEnumerable<int> A = Enumerable.Range(-adjustedRange, adjustedRange * 2);
			int maxB = Math.Max(adjustedRange, quantity) + 2;

			while (Relations.Count() < quantity)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				IEnumerable<int> coprimes = A.Where(a => CoPrime.IsCoprime(a, b));
				IEnumerable<Relation> unfactored = coprimes.Select(a => new Relation(a, b, this));

				SieveRelations(unfactored);

				if (b > maxB)
				{
					break;
				}

				b += 2;
			}

			SaveGnfsProgress();
			return Relations;
		}

		private void SieveRelations(IEnumerable<Relation> unfactored)
		{
			foreach (Relation rel in unfactored)
			{
				rel.Sieve();
				bool smooth = rel.IsSmooth;
				if (smooth)
				{
					Relations.Add(rel);
					Relation.Serialize(Relations_SaveDirectory, rel);
				}
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("N", N.ToString());
			writer.WriteElementString("M", m.ToString());
			writer.WriteElementString("B", b.ToString());
			writer.WriteElementString("Degree", this.degree.ToString());
			writer.WriteElementString("Quantity", quantity.ToString());
			writer.WriteElementString("ValueRange", valueRange.ToString());
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
			string bString = reader.ReadElementString("B");
			string degreeString = reader.ReadElementString("Degree");
			string quantityString = reader.ReadElementString("Quantity");
			string valueRangeString = reader.ReadElementString("ValueRange");
			string primeBoundString = reader.ReadElementString("PrimeBound");
			string rationalFactorBaseString = reader.ReadElementString("RationalFactorBase");
			string algebraicFactorBaseString = reader.ReadElementString("AlgebraicFactorBase");
			string quadraticFactorBaseMinString = reader.ReadElementString("QuadraticFactorBaseMin");
			string quadraticFactorBaseMaxString = reader.ReadElementString("QuadraticFactorBaseMax");

			reader.ReadEndElement();

			N = BigInteger.Parse(nString);
			m = BigInteger.Parse(mString);
			b = int.Parse(bString);
			this.degree = int.Parse(degreeString);
			quantity = int.Parse(quantityString);
			valueRange = int.Parse(valueRangeString);
			PrimeBound = int.Parse(primeBoundString);
			RationalFactorBase = int.Parse(rationalFactorBaseString);
			AlgebraicFactorBase = int.Parse(algebraicFactorBaseString);
			QuadraticFactorBaseMin = int.Parse(quadraticFactorBaseMinString);
			QuadraticFactorBaseMax = int.Parse(quadraticFactorBaseMaxString);
		}

		public XmlSchema GetSchema() { return null; }
	}
}
