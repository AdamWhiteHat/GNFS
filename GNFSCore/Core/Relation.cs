using System;
using System.Linq;
using System.Text;
using System.Numerics;
using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore
{
	public class Relation
	{
		public int A;
		public int B;
		public BigInteger AlgebraicNorm { get; private set; }
		public BigInteger RationalNorm { get; private set; }
		public BigInteger AlgebraicQuotient { get; private set; }
		public BigInteger RationalQuotient { get; private set; }

		public bool IsSmooth { get { return BigInteger.Abs(AlgebraicQuotient) == 1 && BigInteger.Abs(RationalQuotient) == 1; } }

		private BigInteger polyBase;

		public Relation(int a, int b, Irreducible poly)
		{
			A = a;
			B = b;
			polyBase = poly.Base;
			AlgebraicNorm = Algebraic.Norm(a, b, poly);
			RationalNorm = Rational.Norm(a, b, polyBase);
			AlgebraicQuotient = AlgebraicNorm;
			RationalQuotient = RationalNorm;
		}

		public BigInteger GetContribution(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void RemoveAlgebraicFactors(IEnumerable<int> factors)
		{
			BigInteger sqrt = BigInteger.Abs(AlgebraicNorm).SquareRoot();

			foreach (int factor in factors)
			{
				if (BigInteger.Abs(AlgebraicQuotient) == 1 || factor > sqrt)
				{
					break;
				}
				while (AlgebraicQuotient % factor == 0 && BigInteger.Abs(AlgebraicQuotient) != 1)
				{
					AlgebraicQuotient /= factor;
				}
			}
		}

		public void RemoveRationalFactors(IEnumerable<int> factors)
		{
			BigInteger sqrt = BigInteger.Abs(RationalNorm).SquareRoot();

			foreach (int factor in factors)
			{
				if (BigInteger.Abs(RationalQuotient) == 1 || factor > sqrt)
				{
					break;
				}
				while (RationalQuotient % factor == 0 && BigInteger.Abs(RationalQuotient) != 1)
				{
					RationalQuotient /= factor;
				}
			}
		}

		public override string ToString()
		{
			return
			$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)}\t" +
			$"Z:{AlgebraicNorm.ToString().PadLeft(10)},\ta+bm={RationalNorm.ToString().PadLeft(4)},\t" +
			$"{BigInteger.Abs(A) % 4}{BigInteger.Abs(B) % 4}{BigInteger.Abs(AlgebraicNorm) % 4}{BigInteger.Abs(RationalNorm % 4)})";
		}
	}
}
