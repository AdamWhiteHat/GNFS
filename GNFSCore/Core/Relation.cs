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

		public Relation(int a, int b, Irreducible poly)
		{
			A = a;
			B = b;
			AlgebraicNorm = Algebraic.Norm(a, b, poly);
			RationalNorm = Rational.Norm(a, b, poly.Base);
			AlgebraicQuotient = AlgebraicNorm;
			RationalQuotient = RationalNorm;
		}

		public void RemoveAlgebraicFactors(IEnumerable<int> factors)
		{
			foreach (int factor in factors)
			{
				if (BigInteger.Abs(AlgebraicQuotient) == 1)
				{
					break;
				}
				while(AlgebraicQuotient % factor == 0)// && BigInteger.Abs(AlgebraicNorm) != 1)
				{
					AlgebraicQuotient /= factor;
				}
			}
		}

		public void RemoveRationalFactors(IEnumerable<int> factors)
		{
			foreach (int factor in factors)
			{
				if (BigInteger.Abs(RationalQuotient) == 1)
				{
					break;
				}
				if (RationalQuotient % factor == 0)// && BigInteger.Abs(RationalNorm) != 1)
				{
					RationalQuotient /= factor;
				}
			}
		}

		public override string ToString()
		{
			return $"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(4)}\t{AlgebraicNorm.ToString().PadLeft(10)},{RationalNorm.ToString().PadLeft(10)})";
		}
	}
}
