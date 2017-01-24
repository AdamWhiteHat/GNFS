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

		public static IEnumerable<Tuple<int, int>> GetAlgebraicFactorBase(int bound)
		{
			List<int> primes = Eratosthenes.Sieve(bound);
			List<int> integers = Enumerable.Range(2, primes.Last()).ToList();

			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			foreach(int p in primes)
			{
				IEnumerable<int> factors = integers.Where(i => i % p == 0);

				if(factors.Any())
				{
					result.AddRange(factors.Select(f => new Tuple<int, int>(p, f)));
				}
			}
						
			return result;
		}

		public static BigInteger Norm(int a, int b, Polynomial poly)
		{
			BigInteger left = BigInteger.Pow((-1 * b), poly.Degree);
			BigInteger f = BigInteger.Divide(a, b) * -1;
			BigInteger right = poly.Eval(f);

			return BigInteger.Multiply(left, right);
		}
	}
}
