using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class Sieve
	{
		// The elements (a, b) with rational norm divisible by element (p, r) from RFB
		// are those with a on the form a = −bm + kp for k ∈ Z.
		public static IEnumerable<Tuple<int, int>> LineSieve(GNFS gnfs, int range)
		{
			var rationalNormsA = Rational.GetRationalNormsElements(gnfs, range);
			return rationalNormsA;
		}

		public static IEnumerable<Tuple<int, int>> Smooth(GNFS gnfs, IEnumerable<Tuple<int, int>> rels)
		{
			var primeBase = gnfs.Primes.Take(gnfs.PrimeBound);
			var rationalSmooth = rels.Where(a => Rational.IsSmooth(a.Item1, primeBase) && Rational.IsSmooth(a.Item2, primeBase));
			var algebraicSmooth = rationalSmooth.Where(a => Rational.IsSmooth((int)Algebraic.Norm(a.Item1, a.Item2, gnfs.AlgebraicPolynomial), primeBase));

			var ordered = algebraicSmooth.OrderByDescending(tupl => tupl.Item1);
			var results = ordered.Distinct();			
			return results;
		}

	}
}
