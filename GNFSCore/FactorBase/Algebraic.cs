using GNFSCore.Prime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.FactorBase
{
	public class Algebraic
	{
		// array of (p, r) where f(r) = 0 mod p
		// quantity = 2-3 times RFB.quantity

		public static IEnumerable<Tuple<int, int>> GetAlgebraicFactorBase(Polynomial poly, int bound)
		{
			List<int> primes = Eratosthenes.Sieve(bound);
			List<int> integers = Enumerable.Range(3, primes.Last()-1).ToList();

			return GetFactorBase(poly, primes, integers);
		}

		internal static IEnumerable<Tuple<int, int>> GetFactorBase(Polynomial poly, List<int> primes, List<int> integers)
		{
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			foreach (int r in integers)
			{
				BigInteger polyR = poly.Eval(r);

				IEnumerable<BigInteger> residues = primes.Select(p => polyR % p);
				IEnumerable<int> factors = primes.Where(p => ((polyR % p) == 0));

				if (factors.Any())
				{
					result.AddRange(factors.Select(p => new Tuple<int, int>(p, r)));
				}
			}

			return result.OrderBy(tup => tup.Item1);
		}

		// The elements(a, b) with algebraic norm divisible by element(p, r) from AFB
		// are those with a on the form a = −br + kp for k ∈ Z.
		public static BigInteger Norm(int a, int b, Polynomial poly)
		{
			BigInteger left = BigInteger.Pow((-1 * b), poly.Degree);
			BigInteger f = BigInteger.Divide(a, b) * -1;
			BigInteger right = poly.Eval(f);

			return BigInteger.Multiply(left, right);
		}
	}
}
