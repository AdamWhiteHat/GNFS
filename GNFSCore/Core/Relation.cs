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

	public class Relation
	{
		public int A;
		public int B;
		public BigInteger C;
		//public double D;
		//public double E;
		//public double F;
		//public double G;
		//public double H;
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
			AlgebraicNorm = Algebraic.Norm(a, b, poly); // b^deg * f( a/b )
			RationalNorm = Rational.Norm(a, b, polyBase); // a + bm
			C = Irreducible.Evaluate(poly, RationalNorm) % poly.N;
			//D = Irreducible.Evaluate(poly, (double)a) % (double)poly.N;
			//E = Irreducible.Evaluate(poly, (double)AlgebraicNorm) % (double)poly.N;

			//F = Irreducible.Derivative(poly, (double)a) % (double)poly.N;
			//G = Irreducible.Derivative(poly, (double)RationalNorm) % (double)poly.N;
			//H = Irreducible.Derivative(poly, (double)AlgebraicNorm) % (double)poly.N;
			AlgebraicQuotient = AlgebraicNorm;
			RationalQuotient = RationalNorm;
		}

		public BigInteger GetContribution(BigInteger x, BigInteger modQ)
		{
			return GetContribution(x) % modQ;
		}

		public BigInteger GetContribution(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void RemoveAlgebraicFactors(IEnumerable<int> factors)
		{
			AlgebraicQuotient = RemoveFactors(factors, AlgebraicNorm, AlgebraicQuotient);
		}

		public void RemoveRationalFactors(IEnumerable<int> factors)
		{
			RationalQuotient = RemoveFactors(factors, RationalNorm, RationalQuotient);
		}

		private static BigInteger RemoveFactors(IEnumerable<int> factors, BigInteger norm, BigInteger quotient)
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
			$"f({RationalNorm})%N".PadLeft(10) + $" = {C.ToString().PadLeft(10)}\t" +
			//$"f({RationalNorm})%N".PadLeft(10) + $" = {D.ToString().PadLeft(10)}\t" +
			//$"f({AlgebraicNorm})%N".PadLeft(10) + $" = {E.ToString().PadLeft(10)}\t" +
			//$"f'({A})%N".PadLeft(10) + $" = {F.ToString().PadLeft(10)}\t" +
			//$"f'({RationalNorm})%N".PadLeft(10) + $" = {G.ToString().PadLeft(10)}\t" +
			//$"f'({AlgebraicNorm})%N".PadLeft(10) + $" = {H.ToString().PadLeft(10)}\t" +
			$"[f(b) ≡ 0 mod a:{AlgebraicNorm.ToString().PadLeft(10)},\ta+bm={RationalNorm.ToString().PadLeft(4)}]\t" +
			$"{BigInteger.Abs(A) % 4 % 2}{BigInteger.Abs(B) % 4 % 2}{BigInteger.Abs(AlgebraicNorm) % 4 % 2}{BigInteger.Abs(RationalNorm) % 4 % 2}\t" +
			$"IsAQuadraticB ? {QuadraticResidue.IsQuadraticResidue(A, B)}";
		}
	}
}
