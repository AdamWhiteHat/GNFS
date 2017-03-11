
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
				int algebraicBound = (int)(gnfs.PrimeBound * 3.3);
				int primeBound = PrimeFactory.GetIndexFromValue(algebraicBound);
				IEnumerable<int> primes = PrimeFactory.GetPrimes(primeBound);
				IEnumerable<int> integers = Enumerable.Range(0, primes.Last());
				return GNFS.PolynomialModP(gnfs.AlgebraicPolynomial, primes, integers);
			}
		}

		public static BigInteger Norm(int a, int b, Irreducible poly)
		{
			// b^deg * f( a/b )

			int bneg = -b;
			var ab = (double)a / (double)bneg;
			double right = poly.Eval(ab);
			double left = Math.Pow(bneg, poly.Degree);
			double norm = left * right;
			int result = (int)Math.Round(norm);

			return result;
		}
	}
}
