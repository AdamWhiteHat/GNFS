using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;

namespace GNFSCore
{
	using Factors;
	using IntegerMath;
	using Matrix;

	public enum FactorBaseType
	{
		Algebraic,
		Rational
	}

	public class Relation
	{
		public int A { get; private set; }
		public int B { get; private set; }
		public BigInteger C { get; private set; }

		public BigInteger AlgebraicNorm { get; private set; }
		public BigInteger RationalNorm { get; private set; }
		public BigInteger NormProduct { get { return BigInteger.Multiply(AlgebraicNorm, RationalNorm); } }

		internal BigInteger AlgebraicQuotient { get; private set; }
		internal BigInteger RationalQuotient { get; private set; }

		public bool IsSmooth
		{
			get
			{
				return BigInteger.Abs(AlgebraicQuotient) == 1 && BigInteger.Abs(RationalQuotient) == 1;
			}
		}

		public Tuple<BigInteger, BigInteger> GetRoughRemainders()
		{
			return new Tuple<BigInteger, BigInteger>(AlgebraicQuotient, RationalQuotient);
		}

		private GNFS _gnfs;

		public Relation(GNFS gnfs, int a, int b)
		{
			A = a;
			B = b;

			AlgebraicNorm = Normal.Algebraic(A, B, gnfs.CurrentPolynomial); // b^deg * f( a/b )
			RationalNorm = Normal.Rational(A, B, gnfs.CurrentPolynomial.Base); // a + bm

			AlgebraicQuotient = AlgebraicNorm;
			RationalQuotient = RationalNorm;

			BigInteger rationalEval = gnfs.CurrentPolynomial.Evaluate(RationalNorm);
			C = (rationalEval % gnfs.N);

			_gnfs = gnfs;
		}

		public Relation(GNFS gnfs, int a, int b, BigInteger c, BigInteger algebraicNorm, BigInteger rationalNorm)
		{
			A = a;
			B = b;
			C = c;

			AlgebraicNorm = algebraicNorm;
			RationalNorm = rationalNorm;

			AlgebraicQuotient = 0;
			RationalQuotient = 0;

			_gnfs = gnfs;
		}

		public BigInteger Apply(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void Sieve(PolyRelationsSieveProgress relationsSieve)
		{
			AlgebraicQuotient = Factor(relationsSieve.PrimeBase.AlgebraicFactorBase, AlgebraicNorm, AlgebraicQuotient);
			RationalQuotient = Factor(relationsSieve.PrimeBase.RationalFactorBase, RationalNorm, RationalQuotient);
		}

		private static BigInteger Factor(IEnumerable<BigInteger> primeFactors, BigInteger norm, BigInteger quotient)
		{
			BigInteger sqrt = BigInteger.Abs(norm).SquareRoot();

			BigInteger result = quotient;
			foreach (BigInteger factor in primeFactors)
			{
				if (result == 0 || result == -1 || result == 1 || factor > sqrt)
				{
					break;
				}
				while (result % factor == 0 && result != 1 && result != -1)
				{
					result /= factor;

					BigInteger absResult = BigInteger.Abs(result);
					if (absResult > 1 /*&& absResult < int.MaxValue - 1*/)
					{
						//int intValue = (int)absResult;
						if (primeFactors.Contains(absResult))
						{
							result = 1;
						}
					}
				}
			}
			return result;
		}

		public int RationalWeight { get { return _rationalFactorization.Any() ? _rationalFactorization.Count : 0; } }
		public int AlgebraicWeight { get { return _algebraicFactorization.Any() ? _algebraicFactorization.Count : 0; } }





		private PrimeFactorization _algebraicFactorization = new PrimeFactorization();
		private PrimeFactorization _rationalFactorization = new PrimeFactorization();

		public PrimeFactorization AlgebraicFactorization { get { checkMatrixInitialization(); return _algebraicFactorization; } }
		public PrimeFactorization RationalFactorization { get { checkMatrixInitialization(); return _rationalFactorization; } }

		public void MatrixInitialize()
		{
			_rationalFactorization = new PrimeFactorization(RationalNorm, _gnfs.PrimeFactorBase.MaxRationalFactorBase, true);
			_algebraicFactorization = new PrimeFactorization(AlgebraicNorm, _gnfs.PrimeFactorBase.MaxAlgebraicFactorBase, true);
		}

		private void checkMatrixInitialization()
		{
			if (!_algebraicFactorization.Any() && !_rationalFactorization.Any())
			{
				MatrixInitialize();
			}
		}

		public bool[] GetRationalMatrixRow()
		{
			checkMatrixInitialization();

			bool[] rational = GetVector(_rationalFactorization, _gnfs.PrimeFactorBase.MaxRationalFactorBase);

			bool sign = (RationalNorm.Sign == -1);
			return new bool[] { sign }.Concat(rational).ToArray();
		}

		public bool[] GetAlgebraicMatrixRow()
		{
			checkMatrixInitialization();

			bool[] algebraic = GetVector(_algebraicFactorization, _gnfs.PrimeFactorBase.MaxAlgebraicFactorBase);

			bool sign = (AlgebraicNorm.Sign == -1);
			return new bool[] { sign }.Concat(algebraic).ToArray();
		}

		private bool[] GetVector(PrimeFactorization primeFactorization, BigInteger maxValue)
		{
			int primeIndex = PrimeFactory.GetIndexFromValue(maxValue);

			bool[] result = new bool[primeIndex + 1];
			if (primeFactorization.Any())
			{
				foreach (Factor oddFactor in primeFactorization.Where(f => f.ExponentMod2 == 1))
				{
					if (oddFactor.Prime > maxValue)
					{
						throw new Exception();
					}
					int index = PrimeFactory.GetIndexFromValue(oddFactor.Prime);
					result[index] = true;
				}
			}

			return result.Take(primeIndex).ToArray();
		}

		
		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t"
				+ $"[ƒ(b) ≡ 0 mod a:{AlgebraicNorm.ToString().PadLeft(10)} (AlgebraicNorm) IsSquare: {AlgebraicNorm.IsSquare()},\ta+b*m={RationalNorm.ToString().PadLeft(4)} (RationalNorm) IsSquare: {RationalNorm.IsSquare()}]\t"
				//+ $"ƒ({RationalNorm}) =".PadRight(8) + $"{C.ToString().PadLeft(6)}"
				;
		}

		public static List<Relation> LoadUnfactoredFile(GNFS gnfs, string filename)
		{
			return File.ReadAllLines(filename).Select(str =>
			{
				string[] ab = str.Split(new char[] { ',' });
				return new Relation(gnfs, int.Parse(ab[0]), int.Parse(ab[1]));
			}).ToList();
		}

		public static void SerializeUnfactoredToFile(string filename, List<Relation> relations)
		{
			File.WriteAllLines(filename, relations.Select(rel => $"{rel.A},{rel.B}"));
		}

		public void Save(string filename, GNFS gnfs)
		{
			SerializeToFile(this, gnfs, filename);// $"{directory}\\{relation.A}_{relation.B}.relation"
		}

		public static void SerializeToFile(Relation rel, GNFS gnfs, string filename)
		{
			new XDocument(
				new XElement("Relation",
					new XElement("A", rel.A),
					new XElement("B", rel.B),
					new XElement("C", rel.C),
					new XElement("AlgebraicNorm", rel.AlgebraicNorm),
					new XElement("RationalNorm", rel.RationalNorm),
					new XElement("AlgebraicFactorization", FactorizationFactory.GetPrimeFactorization(rel.AlgebraicNorm, gnfs.PrimeFactorBase.MaxAlgebraicFactorBase).ToString()),
					new XElement("RationalFactorization", FactorizationFactory.GetPrimeFactorization(rel.RationalNorm, gnfs.PrimeFactorBase.MaxRationalFactorBase).ToString()),
					new XElement("AlgebraicMatrixRow", string.Join(",", rel.GetAlgebraicMatrixRow().Select(b => b ? '1' : '0')))
				)
			).Save(filename);
		}

		public static Relation LoadFromFile(GNFS gnfs, string filename)
		{
			XElement rel = XElement.Load(filename);
			int a = int.Parse(rel.Element("A").Value);
			int b = int.Parse(rel.Element("B").Value);
			BigInteger c = BigInteger.Parse(rel.Element("C").Value);
			BigInteger an = BigInteger.Parse(rel.Element("AlgebraicNorm").Value);
			BigInteger rn = BigInteger.Parse(rel.Element("RationalNorm").Value);

			return new Relation(gnfs, a, b, c, an, rn);
		}
	}
}
