
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

		/*
		// The elements(a, b) with algebraic norm divisible by element(p, r) from AFB
		// are those with a on the form a = −br + kp for k ∈ Z.
		public static IEnumerable<Tuple<int, int>> GetAlgebraicNormRelations(GNFS gnfs, int range)
		{
			int m = (int)gnfs.AlgebraicPolynomial.Base;
			var result = new List<Tuple<int, int>>();
			int relationsNeeded = gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count();

			int b = 1;
			while (result.Count < relationsNeeded && b < 1000)
			{
				result.AddRange(
					gnfs.RFB.SelectMany(tup =>
						GetDivisibleElements(b, tup.Item1, tup.Item2, range).Select(a => new Tuple<int, int>(a, b))
					)
				);

				b++;
			}
			return result.Distinct();
		}

		internal static IEnumerable<int> GetDivisibleElements(int b, int p, int m, int range)
		{
			int bm = b * m;

			int kLower = (-range + bm) / p;
			int kUpper = (range + bm) / p;
			int count = Math.Abs(kLower) + Math.Abs(kUpper);
			var divisible = Enumerable.Range(kLower, count).Select(k => -bm + (p * k)).Distinct();

			var algebraicNorms = divisible.Where(a => CoPrime.IsCoprime(a, b));
			return algebraicNorms;
		}

		// A first degree prime ideal P represented by the pair 
		// (r, p) | a + bθ
		// iif
		// p | Norm(a + bθ)
		// which occurs iif
		// a ≡ −br(mod p)

		public static BigInteger Norm(int a, int b, Irreducible poly)
		{
			// -b^deg * f( -a/b ) 
			int bneg = -1 * b;
			BigInteger left = BigInteger.Pow(bneg, poly.Degree);
			BigInteger f = BigInteger.Divide(a, bneg);
			BigInteger right = poly.Eval(f);

			return BigInteger.Multiply(left, right);
		}
		*/
	}
}
