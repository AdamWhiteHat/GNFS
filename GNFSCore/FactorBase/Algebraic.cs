
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
				IEnumerable<int> primes = PrimeFactory.GetPrimes(algebraicBound);
				IEnumerable<int> integers = Enumerable.Range(0, primes.Last());
				return GNFS.PolynomialModP(gnfs.AlgebraicPolynomial, primes, integers);
			}
		}

		public static BigInteger Norm(int a, int b, Irreducible poly)
		{
			// b^deg * f( a/b )

			int bneg = -b;
			double ab = (double)a / (double)bneg;

			BigInteger remainder = new BigInteger();
			BigInteger quotient = BigInteger.DivRem(a, bneg, out remainder);
			double remaind = (double)remainder/(double)bneg;
			
			double right = Irreducible.Evaluate(poly, ab);
			double left = Math.Pow(bneg, poly.Degree);
			
			double deci = right % 1;
			double deciProduct = deci * left;
			deciProduct = Math.Round(deciProduct, MidpointRounding.ToEven);

			BigInteger result = BigInteger.Multiply((BigInteger)right, (BigInteger)left);
			result += (BigInteger)deciProduct;

			if(remainder>0)
			{
				int i = 0;
			}
			
			return result;
		}
	}
}
