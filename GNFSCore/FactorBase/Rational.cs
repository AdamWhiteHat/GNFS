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
			IEnumerable<Tuple<int,int>> result = primes.Select(p => new Tuple<int, int>(p, (int)(polynomialBase % p)));
			return result;
		}

		// The elements (a, b) with rational norm divisible by element (p, r) from RFB
		// are those with a on the form a = −bm + kp for k ∈ Z.
		public static BigInteger Norm(int a, int b, BigInteger polyBase)
		{
			return BigInteger.Add(a, BigInteger.Multiply(b, polyBase));
		}
	}
}
