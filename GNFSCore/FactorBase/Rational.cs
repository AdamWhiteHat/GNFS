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
	public class Rational
	{
		// m = polynomial base
		// array of (p, p mod m) up to bound
		// quantity = phi(bound)

		public static IEnumerable<Tuple<int, int>> GetRationalFactorBase(BigInteger polynomialBase, int bound)
		{
			List<int> primes = Eratosthenes.Sieve(bound);			
			return primes.Select(p => new Tuple<int, int>(p, (int)(polynomialBase % p)));
		}
	}
}
