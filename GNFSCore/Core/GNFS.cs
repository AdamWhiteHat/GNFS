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
	using Polynomial;
	using IntegerMath;

	public partial class GNFS : IXmlSerializable
	{
		public BigInteger N { get; set; }
		public List<IPolynomial> PolynomialCollection;
		public IPolynomial CurrentPolynomial { get; private set; }
		public PolyRelationsSieveProgress CurrentRelationsProgress { get; set; }
		public CancellationToken CancelToken { get; set; }

		public FactorBase PrimeFactorBase { get; set; }

		public FactorCollection RFB { get; set; } = null;
		public FactorCollection AFB { get; set; } = null;
		public FactorCollection QFB { get; set; } = null;

		public DirectoryLocations SaveLocations { get; set; }

		private BigInteger m;
		private int degree = 2;

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

			SaveLocations = new DirectoryLocations(N);

			if (!Directory.Exists(SaveLocations.SaveDirectory))
			{
				// New GNFS instance
				Directory.CreateDirectory(SaveLocations.SaveDirectory);

				if (polyDegree == -1)
				{
					this.degree = CalculateDegree(n);
				}
				else
				{
					this.degree = polyDegree;
				}



				CaclulatePrimeBounds(primeBound);
				ConstructNewPolynomial(polynomialBase, this.degree);
				m = polynomialBase;

				CurrentRelationsProgress = new PolyRelationsSieveProgress(this, CancelToken, SaveLocations.Polynomial_SaveDirectory, relationQuantity, relationValueRange);

				SaveGnfsProgress();

				LoadFactorBases();
			}
			else
			{
				GNFS gnfs = (GNFS)Serializer.Deserialize(SaveLocations.GnfsParameters_SaveFile, typeof(GNFS));
				LoadGnfsProgress(gnfs);
			}
		}

		public bool IsFactor(BigInteger toCheck)
		{
			return ((N % toCheck) == 0);
		}

		public void SaveGnfsProgress()
		{
			Serializer.Serialize(SaveLocations.GnfsParameters_SaveFile, this);
		}

		public void LoadGnfsProgress(GNFS input)
		{
			N = input.N;
			m = input.m;
			int polyDegree = input.degree;
			

			PrimeFactorBase.MaxRationalFactorBase = input.PrimeFactorBase.MaxRationalFactorBase;
			PrimeFactorBase.MaxAlgebraicFactorBase = input.PrimeFactorBase.MaxAlgebraicFactorBase;
			PrimeFactorBase.MinQuadraticFactorBase = input.PrimeFactorBase.MinQuadraticFactorBase;
			PrimeFactorBase.MaxQuadraticFactorBase = input.PrimeFactorBase.MaxQuadraticFactorBase;

			int base10 = N.ToString().Length;
			int quadraticBaseSize = CalculateQuadraticBaseSize(polyDegree);

			PrimeFactorBase.RationalFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxRationalFactorBase).ToList();
			PrimeFactorBase.AlgebraicFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxAlgebraicFactorBase).ToList();
			PrimeFactorBase.QuadraticFactorBase = PrimeFactory.GetPrimesFrom(PrimeFactorBase.MinQuadraticFactorBase).Take(quadraticBaseSize).ToList();

			// Load Polynomial
			if (Directory.Exists(SaveLocations.SaveDirectory))
			{
				IEnumerable<string> polynomialFiles = Directory.EnumerateFiles(SaveLocations.SaveDirectory, SaveLocations.Polynomial_Filename, SearchOption.AllDirectories);
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

			// Load FactorBases
			LoadFactorBases();

			// Load Relations
			CurrentRelationsProgress = PolyRelationsSieveProgress.LoadProgress(this, SaveLocations.Polynomial_SaveDirectory);
		}

		public void SavePolynomial(IPolynomial poly)
		{
			SaveLocations.SetPolynomialPath(poly);

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
			PrimeFactorBase.RationalFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxRationalFactorBase).ToList();

			PrimeFactorBase.MaxAlgebraicFactorBase = (PrimeFactorBase.MaxRationalFactorBase) * 3;
			PrimeFactorBase.AlgebraicFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.MaxAlgebraicFactorBase).ToList();

			int quadraticBaseSize = 0;

			if (degree <= 3)
			{
				int tempQ = (PrimeFactorBase.RationalFactorBase.Count + PrimeFactorBase.AlgebraicFactorBase.Count + 1);
				tempQ = tempQ / 10;

				quadraticBaseSize = Math.Min(tempQ, 100);
			}
			else
			{
				quadraticBaseSize = CalculateQuadraticBaseSize(degree);
			}

			PrimeFactorBase.MinQuadraticFactorBase = BigInteger.Multiply(bound, 3) + BigInteger.Divide(bound, 2);
			PrimeFactorBase.QuadraticFactorBase = PrimeFactory.GetPrimesFrom(PrimeFactorBase.MinQuadraticFactorBase).Take((quadraticBaseSize*2)+1).ToList();

			PrimeFactorBase.MaxQuadraticFactorBase = PrimeFactorBase.QuadraticFactorBase.Last();

		}

		private static int CalculateQuadraticBaseSize(int polyDegree)
		{
			int result = -1;

			if (polyDegree == 4)
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

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine($"N = {N}");
			result.AppendLine();
			result.AppendLine($"Polynomial(degree: {degree}, base: {CurrentPolynomial.Base}):");
			result.AppendLine("ƒ(m) = " + CurrentPolynomial.ToString());
			result.AppendLine();
			result.AppendLine("Prime Factor Base Bounds:");
			result.AppendLine($"RationalFactorBase : {PrimeFactorBase.MaxRationalFactorBase}");
			result.AppendLine($"AlgebraicFactorBase: {PrimeFactorBase.MaxAlgebraicFactorBase}");
			result.AppendLine($"QuadraticPrimeBase Range: {PrimeFactorBase.MinQuadraticFactorBase} - {PrimeFactorBase.MaxQuadraticFactorBase}");
			result.AppendLine($"QuadraticPrimeBase Count: {PrimeFactorBase.QuadraticFactorBase.Count}");
			result.AppendLine();
			result.AppendLine($"RFB - Rational Factor Base (Count: {RFB.Count}):");
			result.AppendLine(RFB.ToString(200));
			result.AppendLine();
			result.AppendLine($"AFB - Algebraic Factor Base (Count: {AFB.Count}):");
			result.AppendLine(AFB.ToString(200));
			result.AppendLine();
			result.AppendLine($"QFB - Quadratic Factor Base (Count: {QFB.Count}):");
			result.AppendLine(QFB.ToString());
			result.AppendLine();

			List<int> prms = new List<int>();
			prms.AddRange(RFB.Primes);
			prms.AddRange(AFB.Primes);
			prms.AddRange(QFB.Primes);
			prms = prms.Distinct().ToList();

			BigInteger maxPrime = prms.Max();

			result.AppendLine();
			result.AppendLine("Distinct primes (from factor bases):");
			result.AppendLine(prms.FormatString(false));
			result.AppendLine();

			return result.ToString();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("N", N.ToString());
			writer.WriteElementString("M", m.ToString());
			writer.WriteElementString("Degree", this.degree.ToString());
			writer.WriteElementString("RationalFactorBase", PrimeFactorBase.MaxRationalFactorBase.ToString());
			writer.WriteElementString("AlgebraicFactorBase", PrimeFactorBase.MaxAlgebraicFactorBase.ToString());
			writer.WriteElementString("QuadraticFactorBaseMin", PrimeFactorBase.MinQuadraticFactorBase.ToString());
			writer.WriteElementString("QuadraticFactorBaseMax", PrimeFactorBase.MaxQuadraticFactorBase.ToString());
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

			reader.ReadEndElement();

			N = BigInteger.Parse(nString);
			m = BigInteger.Parse(mString);
			this.degree = int.Parse(degreeString);
			PrimeFactorBase.MaxRationalFactorBase = int.Parse(rationalFactorBaseString);
			PrimeFactorBase.MaxAlgebraicFactorBase = int.Parse(algebraicFactorBaseString);
			PrimeFactorBase.MinQuadraticFactorBase = int.Parse(quadraticFactorBaseMinString);
			PrimeFactorBase.MaxQuadraticFactorBase = int.Parse(quadraticFactorBaseMaxString);
		}

		public XmlSchema GetSchema() { return null; }
	}
}
