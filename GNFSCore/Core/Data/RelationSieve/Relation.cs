using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GNFSCore.Data.RelationSieve
{
	using Algorithm.IntegerMath;
	using Algorithm.ExtensionMethods;

	public class Relation : IEquatable<Relation>, IEqualityComparer<Relation>
	{
		[JsonProperty(Order = 0)]
		public BigInteger A { get; protected set; }

		/// <summary>
		/// Root of f(x) in algebraic field
		/// </summary>
		[JsonProperty(Order = 1)]
		public BigInteger B { get; protected set; }

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

		[JsonProperty(Order = 6)]
		public CountDictionary AlgebraicFactorization { get; private set; }
		[JsonProperty(Order = 7)]
		public CountDictionary RationalFactorization { get; private set; }

		[JsonProperty(Order = 8)]
		public bool IsSmooth { get { return IsRationalQuotientSmooth && IsAlgebraicQuotientSmooth; } }

		[JsonProperty(Order = 9)]
		public bool IsRationalQuotientSmooth { get { return RationalQuotient == 1 || RationalQuotient == 0; } }

		[JsonProperty(Order = 10)]
		public bool IsAlgebraicQuotientSmooth { get { return AlgebraicQuotient == 1 || AlgebraicQuotient == 0; } }


		[JsonIgnore]
		public bool IsPersisted { get; set; }

		public Relation()
		{
			IsPersisted = false;
			RationalFactorization = new CountDictionary();
			AlgebraicFactorization = new CountDictionary();
		}

		public Relation(GNFS gnfs, BigInteger a, BigInteger b)
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

		public Relation(BigInteger a, BigInteger b, BigInteger algebraicNorm, BigInteger rationalNorm, CountDictionary algebraicFactorization, CountDictionary rationalFactorization)
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
				return Equals(other);
			}
		}

		public bool Equals(Relation x, Relation y)
		{
			return x.Equals(y);
		}

		public bool Equals(Relation other)
		{
			return A == other.A && B == other.B;
		}

		public int GetHashCode(Relation obj)
		{
			return obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return Tuple.Create(A, B).GetHashCode();
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
