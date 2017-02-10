using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.IntegerMath;
using GNFSCore.Polynomial;

namespace GNFSCore.FactorBase
{
	public class Quadradic
	{
		// array of (p, r) where f(r) = 0 mod p
		// where p > AFB.Last().p
		// quantity =< 100
		public static class Factory
		{
			public static IEnumerable<Tuple<int, int>> GetQuadradicFactorBase(GNFS gnfs)
			{
				int min = gnfs.PrimeBound * 3;
				int max = min + gnfs.PrimeBound;
				List<int> primes = gnfs.Primes.Take(max).Except(Enumerable.Range(0, min)).ToList();
				List<int> integers = Enumerable.Range(2, min).ToList();
				return GNFS.PolynomialModP(gnfs.AlgebraicPolynomial, primes, integers);
			}
		}		
	}
}
