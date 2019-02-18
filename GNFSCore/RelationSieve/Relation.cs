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
	using Polynomials;

	public class Relation : IEquatable<Relation>, IEqualityComparer<Relation>, IFormattable, IXmlSerializable
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

		[XmlElement(ElementName = "AlgebraicFactorization")]
		public CountDictionary AlgebraicFactorization { get; private set; }

		[XmlElement(ElementName = "RationalFactorization")]
		public CountDictionary RationalFactorization { get; private set; }

		internal BigInteger AlgebraicQuotient;
		internal BigInteger RationalQuotient;

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

		public Relation()
		{
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
			Sieve(relationsSieve.PrimeBase.AlgebraicFactorBase, ref AlgebraicQuotient, AlgebraicFactorization);
			Sieve(relationsSieve.PrimeBase.RationalFactorBase, ref RationalQuotient, RationalFactorization);
		}

		private static void Sieve(IEnumerable<BigInteger> primeFactors, ref BigInteger quotientValue, CountDictionary dictionary)
		{
			if (quotientValue.Sign == -1 || primeFactors.Any(f => f.Sign == -1))
			{
				throw new Exception("There shouldn't be any negative values either in the quotient or the factors");
			}

			foreach (BigInteger factor in primeFactors)
			{
				if (quotientValue == 0 || quotientValue == 1)
				{
					return;
				}

				if ((factor * factor) > quotientValue)
				{
					if (primeFactors.Contains(quotientValue))
					{
						dictionary.Add(quotientValue);
						quotientValue = 1;
					}
					return;
				}

				while (quotientValue != 1 && quotientValue % factor == 0)
				{
					quotientValue = BigInteger.Divide(quotientValue, factor);
					dictionary.Add(factor);
				}
			}
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
			Serializer.Serialize(filename, this); // $"{directory}\\{relation.A}_{relation.B}.relation"
		}

		#region IEquatable / IEqualityComparer

		public override bool Equals(object obj)
		{
			Relation other = obj as Relation;

			if (other == null)
			{
				return false;
			}
			else
			{
				return this.Equals(other);
			}
		}

		public bool Equals(Relation x, Relation y)
		{
			return x.Equals(y);
		}

		public bool Equals(Relation other)
		{
			return (this.A == other.A && this.B == other.B);
		}

		public int GetHashCode(Relation obj)
		{
			return obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return Tuple.Create(this.A, this.B).GetHashCode();
		}

		#endregion

		#region IFormattable

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return ToString();
		}

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t"
				+ $"[ƒ(b) ≡ 0 (mod a):{AlgebraicNorm.ToString().PadLeft(10)} (AlgebraicNorm) IsSquare: {AlgebraicNorm.IsSquare()},\ta+b*m={RationalNorm.ToString().PadLeft(4)} (RationalNorm) IsSquare: {RationalNorm.IsSquare()}]\t";
		}

		#endregion

		#region IXmlSerializable

		private static XmlSerializer AlgebraicFactorizationXmlSerializer = new XmlSerializer(typeof(CountDictionary), new XmlRootAttribute("AlgebraicFactorization"));
		private static XmlSerializer RationalFactorizationXmlSerializer = new XmlSerializer(typeof(CountDictionary), new XmlRootAttribute("RationalFactorization"));
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("A", A.ToString());
			writer.WriteElementString("B", B.ToString());
			writer.WriteElementString("AlgebraicNorm", AlgebraicNorm.ToString());
			writer.WriteElementString("RationalNorm", RationalNorm.ToString());
			writer.WriteElementString("AlgebraicQuotient", AlgebraicQuotient.ToString());
			writer.WriteElementString("RationalQuotient", RationalQuotient.ToString());

			AlgebraicFactorizationXmlSerializer.Serialize(writer, AlgebraicFactorization);

			RationalFactorizationXmlSerializer.Serialize(writer, RationalFactorization);
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();

			A = int.Parse(reader.ReadElementString("A"));
			B = int.Parse(reader.ReadElementString("B"));
			AlgebraicNorm = BigInteger.Parse(reader.ReadElementString("AlgebraicNorm"));
			RationalNorm = BigInteger.Parse(reader.ReadElementString("RationalNorm"));
			AlgebraicQuotient = BigInteger.Parse(reader.ReadElementString("AlgebraicQuotient"));
			RationalQuotient = BigInteger.Parse(reader.ReadElementString("RationalQuotient"));


			AlgebraicFactorization = (CountDictionary)AlgebraicFactorizationXmlSerializer.Deserialize(reader);

			RationalFactorization = (CountDictionary)RationalFactorizationXmlSerializer.Deserialize(reader);
		}

		public XmlSchema GetSchema() { return null; }

		#endregion
	}
}
