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
		public bool IsSmooth { get { return AlgebraicNorm == 1 && RationalNorm == 1; } }

		public Relation(int a, int b, Irreducible poly)
		{
			A = a;
			B = b;
			AlgebraicNorm = Algebraic.Norm(a, b, poly);
			RationalNorm = Rational.Norm(a, b, poly.Base);
		}

		public void RemoveAlgebraicFactors(IEnumerable<int> factors)
		{
			foreach (int factor in factors)
			{
				if (AlgebraicNorm == 1)
				{
					break;
				}
				while (AlgebraicNorm % factor == 0 && AlgebraicNorm != 1)
				{
					AlgebraicNorm /= factor;
				}
			}
		}

		public void RemoveRationalFactors(IEnumerable<int> factors)
		{
			foreach (int factor in factors)
			{
				if (AlgebraicNorm == 1)
				{
					break;
				}
				while (RationalNorm % factor == 0)
				{
					RationalNorm /= factor;
				}
			}
		}

		public override string ToString()
		{
			return $"({A},{B})";
		}
	}
}
