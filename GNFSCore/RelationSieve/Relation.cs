using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GNFSCore
{
	using Factors;
	using IntegerMath;
	using Matrix;
	using Polynomial;

	public class Relation
	{
		public int A { get; private set; }

		/// <summary>
		/// Root of f(x) in algebraic field
		/// </summary>
		public int B { get; private set; }

		/// <summary> ƒ(b) ≡ 0 (mod a); Calculated as: ƒ(-a/b) * -b^deg </summary>
		public BigInteger AlgebraicNorm { get; private set; }
		/// <summary>  a + bm </summary>
		public BigInteger RationalNorm { get; private set; }

		/// <summary> (a + bi) </summary>
		public Complex Complex { get { return new Complex(A, B); } }
		/// <summary> (a + bi)*(a - bi) </summary>
		public Complex ComplexNorm { get { return Complex.Multiply(this.Complex, Complex.Conjugate(this.Complex)); } }

		public CountDictionary AlgebraicFactorization { get; private set; }
		public CountDictionary RationalFactorization { get; private set; }

		internal BigInteger AlgebraicQuotient { get; private set; }
		internal BigInteger RationalQuotient { get; private set; }

		public bool IsSmooth
		{
			get
			{
				return (AlgebraicQuotient == 1 || AlgebraicQuotient == 0) && (RationalQuotient == 1 || RationalQuotient == 0);
			}
		}

		public Tuple<BigInteger, BigInteger> GetRoughRemainders()
		{
			return new Tuple<BigInteger, BigInteger>(AlgebraicQuotient, RationalQuotient);
		}

		public Relation(GNFS gnfs, int a, int b)
		{
			AlgebraicFactorization = new CountDictionary();
			RationalFactorization = new CountDictionary();

			A = a;
			B = b;

			AlgebraicNorm = Normal.Algebraic(A, B, gnfs.CurrentPolynomial); // b^deg * f( a/b )
			RationalNorm = Normal.Rational(A, B, gnfs.PolynomialBase); // a + bm

			AlgebraicQuotient = BigInteger.Abs(AlgebraicNorm);
			RationalQuotient = BigInteger.Abs(RationalNorm);

			if (AlgebraicNorm.Sign == -1)
			{
				AlgebraicFactorization.Add(BigInteger.MinusOne);
			}

			if (RationalNorm.Sign == -1)
			{
				RationalFactorization.Add(BigInteger.MinusOne);
			}
		}

		public Relation(int a, int b, BigInteger algebraicNorm, BigInteger rationalNorm, CountDictionary algebraicFactorization, CountDictionary rationalFactorization)
		{
			A = a;
			B = b;

			AlgebraicNorm = algebraicNorm;
			RationalNorm = rationalNorm;

			AlgebraicQuotient = 1;
			RationalQuotient = 1;

			AlgebraicFactorization = algebraicFactorization;
			RationalFactorization = rationalFactorization;
		}

		public BigInteger Apply(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void Sieve(PolyRelationsSieveProgress relationsSieve)
		{
			BigInteger algResult = Factor(relationsSieve.PrimeBase.AlgebraicFactorBase, AlgebraicQuotient, AlgebraicFactorization);
			BigInteger ratReslult = Factor(relationsSieve.PrimeBase.RationalFactorBase, RationalQuotient, RationalFactorization);

			AlgebraicQuotient = algResult;
			RationalQuotient = ratReslult;
		}

		public static BigInteger Factor(IEnumerable<BigInteger> primeFactors,BigInteger quotientValue, CountDictionary dictionary)
		{
			if (quotientValue.Sign == -1 || primeFactors.Any(f => f.Sign == -1))
			{
				throw new Exception("There shouldn't be any negative values either in the quotient or the factors");
			}
						
			BigInteger result = quotientValue;
			foreach (BigInteger factor in primeFactors)
			{
				if (result == 0 || result == 1)
				{
					return result;
				}

				if ((factor*factor) > result)
				{
					if (primeFactors.Contains(result))
					{
						dictionary.Add(result);
						return 1;
					}
					return result;
				}
							
				while (result != 1 && result % factor == 0)
				{
					result = BigInteger.Divide(result, factor);
					dictionary.Add(factor);					
				}
			}

			return result;
		}


		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t"
				+ $"[ƒ(b) ≡ 0 (mod a):{AlgebraicNorm.ToString().PadLeft(10)} (AlgebraicNorm) IsSquare: {AlgebraicNorm.IsSquare()},\ta+b*m={RationalNorm.ToString().PadLeft(4)} (RationalNorm) IsSquare: {RationalNorm.IsSquare()}]\t";
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
					new XElement("AlgebraicNorm", rel.AlgebraicNorm),
					new XElement("RationalNorm", rel.RationalNorm),
					rel.AlgebraicFactorization.SerializeToXElement("AlgebraicFactorization"),
					rel.RationalFactorization.SerializeToXElement("RationalFactorization")
				)
			).Save(filename);
		}

		public static Relation LoadFromFile(string filename)
		{
			XElement rel = XElement.Load(filename);
			int a = int.Parse(rel.Element("A").Value);
			int b = int.Parse(rel.Element("B").Value);
			BigInteger an = BigInteger.Parse(rel.Element("AlgebraicNorm").Value);
			BigInteger rn = BigInteger.Parse(rel.Element("RationalNorm").Value);
			CountDictionary algebraicFactorization = CountDictionary.DeserializeFromXElement(rel.Element("AlgebraicFactorization"));
			CountDictionary rationalFactorization = CountDictionary.DeserializeFromXElement(rel.Element("RationalFactorization"));

			return new Relation(a, b, an, rn, algebraicFactorization, rationalFactorization);
		}

		public override bool Equals(object obj)
		{
			Relation other = obj as Relation;

			if (other == null)
			{
				return false;
			}
			else
			{
				return (this.A == other.A && this.B == other.B);
			}
		}
		public override int GetHashCode()
		{
			return Tuple.Create(this.A, this.B).GetHashCode();
		}
	}
}
