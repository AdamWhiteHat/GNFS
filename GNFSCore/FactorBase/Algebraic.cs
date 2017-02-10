
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;
using GNFSCore.Polynomial;

namespace GNFSCore.FactorBase
{
	public class Algebraic
	{
		// array of (p, r) where f(r) = 0 mod p
		// quantity = 2-3 times RFB.quantity
		public static class Factory
		{
			public static IEnumerable<Tuple<int, int>> GetAlgebraicFactorBase(GNFS gnfs)
			{
				int algebraicBound = gnfs.PrimeBound * 3;
				IEnumerable<int> primes = gnfs.Primes.Take(algebraicBound);
				IEnumerable<int> integers = Enumerable.Range(3, primes.Last() - 1);
				integers = integers.Except(primes);
				return GNFS.PolynomialModP(gnfs.AlgebraicPolynomial, primes, integers);
			}
		}

		// The elements(a, b) with algebraic norm divisible by element(p, r) from AFB
		// are those with a on the form a = −br + kp for k ∈ Z.


		// A first degree prime ideal P represented by the pair 
		// (r, p) | a + bθ
		// iif
		// p | Norm(a + bθ)
		// which occurs iif
		// a ≡ −br(mod p)

		public static BigInteger Norm(int a, int b, Irreducible poly)
		{
			// b^deg(f)*f(a / b) 
			int bneg = -1 * b;
			BigInteger left = BigInteger.Pow(bneg, poly.Degree);
			BigInteger f = BigInteger.Divide(a, bneg);
			BigInteger right = poly.Eval(f);

			return BigInteger.Multiply(left, right);
		}
	}
}
