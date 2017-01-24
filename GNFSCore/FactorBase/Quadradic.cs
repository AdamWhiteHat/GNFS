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

		public static IEnumerable<Tuple<int, int>> GetQuadradicFactorBase(Polynomial poly, int min, int max)
		{
			List<int> primes = Eratosthenes.Sieve(max);
			primes = primes.Except(Enumerable.Range(0, min)).ToList();
			List<int> integers = Enumerable.Range(min, max).ToList();

			return Algebraic.GetFactorBase(poly, primes, integers);
		}
	}
}
