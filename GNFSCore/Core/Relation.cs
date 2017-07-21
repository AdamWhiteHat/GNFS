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
	using FactorBase;
	using PrimeSignature;

	public class Relation
	{
		public int A { get; set; }
		public int B { get; set; }
		public BigInteger C;
		public BigInteger AlgebraicNorm { get; set; }
		public BigInteger RationalNorm { get; set; }

		internal BigInteger AlgebraicQuotient { get; set; }
		internal BigInteger RationalQuotient { get; set; }

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
		}

		public Relation(int a, int b, BigInteger c, BigInteger algebraicNorm, BigInteger rationalNorm)
		{
			A = a;
			B = b;
			C = c;

			AlgebraicNorm = algebraicNorm;
			RationalNorm = rationalNorm;

			AlgebraicQuotient = 0;
			RationalQuotient = 0;
		}

		public void Sieve(PolyRelationsSieveProgress relationsSieve)
		{
			int i = 0;
			AlgebraicQuotient = Factor(relationsSieve.AlgebraicPrimeBase, AlgebraicNorm, AlgebraicQuotient);
			RationalQuotient = Factor(relationsSieve.RationalPrimeBase, RationalNorm, RationalQuotient);
		}

		private static BigInteger Factor(IEnumerable<BigInteger> factors, BigInteger norm, BigInteger quotient)
		{
			BigInteger sqrt = BigInteger.Abs(norm).SquareRoot();

			BigInteger result = quotient;
			foreach (BigInteger factor in factors)
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
						if (factors.Contains(absResult))
						{
							result = 1;
						}
					}
				}
			}
			return result;
		}

		public Tuple<BitVector, BitVector> GetMatrixRowVector(BigInteger rationalFactorBase, BigInteger algebraicFactorBase)
		{
			BitVector rationalBitVector = new BitVector(RationalNorm, rationalFactorBase);
			BitVector algebraicBitVector = new BitVector(AlgebraicNorm, algebraicFactorBase);
			//bool[] quadraticBitVector = QuadraticResidue.GetQuadraticCharacters(this, qudraticFactorBase.QFB);
			//List<bool> combinedVector = new List<bool>();
			//combinedVector.AddRange(rationalBitVector.Elements);
			//combinedVector.AddRange(algebraicBitVector.Elements);
			//combinedVector.AddRange(quadraticBitVector);
			//return new BitVector(RationalNorm, combinedVector.ToArray());
			return new Tuple<BitVector, BitVector>(rationalBitVector, algebraicBitVector);
		}

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t" +
				$"[ƒ(b) ≡ 0 mod a:{AlgebraicNorm.ToString().PadLeft(10)} ({AlgebraicNorm.IsSquare()}),\ta+b*m={RationalNorm.ToString().PadLeft(4)} ({RationalNorm.IsSquare()})]\t" +
				$"ƒ({RationalNorm}) =".PadRight(8) + $"{C.ToString().PadLeft(6)}";
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

		public void Save(string filename)
		{
			SerializeToFile(this, filename);// $"{directory}\\{relation.A}_{relation.B}.relation"
		}

		public static void SerializeToFile(Relation rel, string filename)
		{
			new XDocument(
				new XElement("Relation",
					new XElement("A", rel.A),
					new XElement("B", rel.B),
					new XElement("C", rel.C),
					new XElement("AlgebraicNorm", rel.AlgebraicNorm),
					new XElement("RationalNorm", rel.RationalNorm)
				)
			).Save(filename);
		}

		public static Relation LoadFromFile(string filename)
		{
			XElement rel = XElement.Load(filename);
			int a = int.Parse(rel.Element("A").Value);
			int b = int.Parse(rel.Element("B").Value);
			BigInteger c = BigInteger.Parse(rel.Element("C").Value);
			BigInteger an = BigInteger.Parse(rel.Element("AlgebraicNorm").Value);
			BigInteger rn = BigInteger.Parse(rel.Element("RationalNorm").Value);

			return new Relation(a, b, c, an, rn);
		}
	}
}
