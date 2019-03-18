using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Numerics;
using Newtonsoft.Json;
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

	using Interfaces;

	public class Relation : IEquatable<Relation>, IEqualityComparer<Relation>
	{
		[JsonProperty(Order = 0)]
		public int A { get; protected set; }

		/// <summary>
		/// Root of f(x) in algebraic field
		/// </summary>
		[JsonProperty(Order = 1)]
		public uint B { get; protected set; }

		/// <summary> ƒ(b) ≡ 0 (mod a); Calculated as: ƒ(-a/b) * -b^deg </summary>
		[JsonProperty(Order = 2)]
		public BigInteger AlgebraicNorm { get; protected set; }
		/// <summary>  a + bm </summary>
		[JsonProperty(Order = 3)]
		public BigInteger RationalNorm { get; protected set; }

		[JsonProperty(Order = 4)]
		internal BigInteger AlgebraicQuotient;
		[JsonProperty(Order = 5)]
		internal BigInteger RationalQuotient;

		public bool ShouldSerializeAlgebraicQuotient() { return !(AlgebraicQuotient == 1 || AlgebraicQuotient == 0); }
		public bool ShouldSerializeRationalQuotient() { return !(RationalQuotient == 1 || RationalQuotient == 0); }

		[JsonProperty(Order = 6)]
		public CountDictionary AlgebraicFactorization { get; private set; }
		[JsonProperty(Order = 7)]
		public CountDictionary RationalFactorization { get; private set; }

		[JsonProperty(Order = 8)]
		public bool IsSmooth { get { return (AlgebraicQuotient == 1 || AlgebraicQuotient == 0) && (RationalQuotient == 1 || RationalQuotient == 0); } }

		[JsonIgnore]
		public bool IsPersisted { get; set; }

		public Relation()
		{
			IsPersisted = false;
			RationalFactorization = new CountDictionary();
			AlgebraicFactorization = new CountDictionary();
		}

		public Relation(GNFS gnfs, int a, uint b)
			: this()
		{
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

		/*
		public Relation(Relation relation)
		{
			this.A = relation.A;
			this.B = relation.B;
			this.AlgebraicNorm = relation.AlgebraicNorm;
			this.RationalNorm = relation.RationalNorm;
			this.AlgebraicQuotient = BigInteger.Abs(relation.AlgebraicQuotient);
			this.RationalQuotient = BigInteger.Abs(relation.RationalQuotient);
			this.AlgebraicFactorization = relation.AlgebraicFactorization;
			this.RationalFactorization = relation.RationalFactorization;
			this.IsPersisted = relation.IsPersisted;
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
		*/

		public BigInteger Apply(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void Sieve(PolyRelationsSieveProgress relationsSieve)
		{
			Sieve(relationsSieve._gnfs.PrimeFactorBase.AlgebraicFactorBase, ref AlgebraicQuotient, AlgebraicFactorization);
			Sieve(relationsSieve._gnfs.PrimeFactorBase.RationalFactorBase, ref RationalQuotient, RationalFactorization);
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

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t"
				+ $"[ƒ(b) ≡ 0 (mod a):{AlgebraicNorm.ToString().PadLeft(10)} (AlgebraicNorm) IsSquare: {AlgebraicNorm.IsSquare()},\ta+b*m={RationalNorm.ToString().PadLeft(4)} (RationalNorm) IsSquare: {RationalNorm.IsSquare()}]\t";
		}

	}
}
