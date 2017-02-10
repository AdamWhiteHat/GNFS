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

		// a = (c^2)−(d^2);
		// a + d^2 = c^2;
		// b = 2*c*d = 2*c*(b/(2*c));
		// b = 2*d*(b/(2*d))

		// c^2 = b^2-c^4 / 4*a
		// c = b/(2*d);
		// c =(2*c*d)/(2*d);
		// c = b/2*b/2*c;
		// d = (b/(2*c));
		// d = (2*c*d)/(2*c);
		// d = (b/(2*(b/(2*d))));

		// d = b/2*b/2*d;
		// d = (b/2)^2*d;

		// c^2-a = -1 * d^2;
		//
		// 0 = c^4−4*a*c^2−b^2;
		// 0 = c^4−4*a*c^2−b^2;

		// 0 = c^4 − 4*a*c^2 − (2*c*d)^2;

		// 0 = c^2*c^2 − 4^2*a^2*c^2 − 4*c^2*d^2

		// x^2 = a + b*i;

		// b^2 = c^4 − 4*a*c^2



	}
}
