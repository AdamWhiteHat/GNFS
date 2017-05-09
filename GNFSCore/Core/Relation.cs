using System;
using System.Linq;
using System.Text;
using System.Numerics;
using GNFSCore.IntegerMath;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore
{
	using FactorBase;
	using Polynomial;
	using IntegerMath;
	using ExtendedNumerics;
	using PrimeSignature;

	public class Relation
	{
		public int A;
		public int B;
		public BigInteger C;
		public BigRational AlgebraicNorm { get; private set; }
		public BigInteger RationalNorm { get; private set; }
		public BigInteger AlgebraicQuotient { get; private set; }
		public BigInteger RationalQuotient { get; private set; }

		public bool IsSmooth
		{
			get
			{
				return BigInteger.Abs(AlgebraicQuotient) == 1 && BigInteger.Abs(RationalQuotient) == 1;
			}
		}


		private GNFS _gnfs;

		public Relation(int a, int b, GNFS gnfs)
		{
			A = a;
			B = b;
			_gnfs = gnfs;

			AlgebraicNorm = Normal.AlgebraicRational(a, b, _gnfs.Algebraic); // b^deg * f( a/b )
			RationalNorm = Normal.Rational(a, b, _gnfs.Algebraic.Base); // a + bm

			AlgebraicQuotient = AlgebraicNorm.WholePart;
			RationalQuotient = RationalNorm;

			C = _gnfs.Algebraic.Evaluate(RationalNorm) % _gnfs.N;
		}

		public BigInteger GetContribution(BigInteger x, BigInteger modQ)
		{
			return GetContribution(x) % modQ;
		}

		public BigInteger GetContribution(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void Sieve()
		{
			AlgebraicQuotient = Factor(_gnfs.AlgebraicPrimeBase, AlgebraicNorm.WholePart, AlgebraicQuotient);
			RationalQuotient = Factor(_gnfs.RationalPrimeBase, RationalNorm, RationalQuotient);
		}

		private static BigInteger Factor(IEnumerable<int> factors, BigInteger norm, BigInteger quotient)
		{
			//BigInteger sqrt = BigInteger.Abs(norm).SquareRoot();

			BigInteger result = quotient;
			foreach (int factor in factors)
			{
				if (result == 0 || result == -1 || result == 1 /*|| factor > sqrt*/)
				{
					break;
				}
				while (result % factor == 0 && result != 1 && result != -1)
				{
					result /= factor;

					BigInteger absResult = BigInteger.Abs(result);
					if (absResult > 1 && absResult < int.MaxValue - 1)
					{
						int intValue = (int)absResult;
						if (factors.Contains(intValue))
						{
							result = 1;
						}
					}
				}
			}
			return result;
		}

		public BitVector GetMatrixRowVector()
		{
			BitVector rationalBitVector = new BitVector(RationalNorm, _gnfs.RationalFactorBase);
			BitVector algebraicBitVector = new BitVector(BigInteger.Abs(AlgebraicNorm.WholePart), _gnfs.AlgebraicFactorBase);
			bool[] quadraticBitVector = QuadraticResidue.GetQuadraticCharacters(this, _gnfs.QFB);

			return new BitVector(rationalBitVector.Elements.Concat(algebraicBitVector.Elements).Concat(quadraticBitVector).ToArray());
		}

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t" +
				$"[f(b) ≡ 0 mod a:{AlgebraicNorm.ToString().PadLeft(10)},\ta+bm={RationalNorm.ToString().PadLeft(4)}]\t" +
				$"f({RationalNorm})%N".PadLeft(10) + $" = {C.ToString().PadLeft(10)}\t";

			//+"\t QUOTIENT(Alg): {AlgebraicQuotient} \t QUOTIENT(Rat): {RationalQuotient}";
		}
	}
}
