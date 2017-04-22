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

	public class Relation
	{
		public int A;
		public int B;
		public BigInteger C;
		public BigRational AlgebraicNorm { get; private set; }
		public BigInteger RationalNorm { get; private set; }
		public BigInteger AlgebraicQuotient { get; private set; }
		public BigInteger RationalQuotient { get; private set; }

		public bool IsSmooth { get { return BigInteger.Abs(AlgebraicQuotient) == 1 && BigInteger.Abs(RationalQuotient) == 1; } }

		private BigInteger polyBase;

		public Relation(int a, int b, AlgebraicPolynomial polynomial)
		{
			A = a;
			B = b;
			polyBase = polynomial.Base;

			AlgebraicNorm = Algebraic.Norm(a, b, polynomial); // b^deg * f( a/b )
			RationalNorm = Rational.Norm(a, b, polyBase); // a + bm

			AlgebraicQuotient = AlgebraicNorm.WholePart;
			RationalQuotient = RationalNorm;

			C = polynomial.Evaluate(RationalNorm) % polynomial.N;
		}

		public BigInteger GetContribution(BigInteger x, BigInteger modQ)
		{
			return GetContribution(x) % modQ;
		}

		public BigInteger GetContribution(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void FactorAlgebraicSide(IEnumerable<int> factors)
		{
			AlgebraicQuotient = /*new BigRational(*/Factor(factors, AlgebraicNorm.WholePart, AlgebraicQuotient);
		}

		public void FactorRationalSide(IEnumerable<int> factors)
		{
			RationalQuotient = Factor(factors, RationalNorm, RationalQuotient);
		}

		private static BigInteger Factor(IEnumerable<int> factors, BigInteger norm, BigInteger quotient)
		{
			BigInteger sqrt = BigInteger.Abs(norm).SquareRoot();
			BigInteger absResult;

			BigInteger result = quotient;
			foreach (int factor in factors)
			{
				absResult = BigInteger.Abs(result);
				if (absResult == 1 || factor > sqrt)
				{
					break;
				}
				while (result % factor == 0 && absResult != 1)
				{
					result /= factor;
				}
			}
			return result;
		}

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t" +
				$"[f(b) ≡ 0 mod a:{AlgebraicNorm.ToString().PadLeft(10)},\ta+bm={RationalNorm.ToString().PadLeft(4)}]\t" +
				$"f({RationalNorm})%N".PadLeft(10) + $" = {C.ToString().PadLeft(10)}\t";
		}
	}
}
