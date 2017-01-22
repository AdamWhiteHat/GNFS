using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.Prime;

namespace GNFSCore.FactorBase
{
	public class Quadradic
	{
		// array of (p, r) where f(r) = 0 mod p
		// where p > AFB.Last().p
		// quantity = < 100

		public static IEnumerable<Tuple<int, int>> GetQuadradicFactorBase(int min, int max)
		{
			List<int> primes = Eratosthenes.Sieve(max);
			primes = primes.Except(Enumerable.Range(0, min)).ToList();
			List<int> integers = Enumerable.Range(min, max).ToList();

			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			foreach (int p in primes)
			{
				IEnumerable<int> factors = integers.Where(i => i % p == 0);

				if (factors.Any())
				{
					result.AddRange(factors.Select(f => new Tuple<int, int>(p, f)));
				}
			}

			return result;
		}
	}
}
