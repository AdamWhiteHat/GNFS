using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class Rational
	{
		// m = polynomial base
		// array of (p, p mod m) up to bound
		// quantity = phi(bound)
		public static class Factory
		{
			public static IEnumerable<Tuple<int, int>> BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<int> primes = PrimeFactory.GetPrimes(gnfs.PrimeBound);
				IEnumerable<Tuple<int, int>> result = primes.Select(p => new Tuple<int, int>(p, (int)(gnfs.Algebraic.Base % p)));
				return result.Distinct();
			}
		}

		public static BigInteger Norm(int a, int b, BigInteger polynomialBaseM)
		{
			return BigInteger.Add(a, BigInteger.Multiply(b, polynomialBaseM));
		}
	}
}
