using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Factors
{
	using Data;
	using Algorithm.FactorBase;

	public static class Factory
	{
		// array of (p, m % p) up to bound
		// quantity = phi(bound)
		public static FactorPairCollection BuildRationalFactorPairCollection(GNFS gnfs)
		{
			IEnumerable<FactorPair> result = gnfs.PrimeFactorBase.RationalFactorBase.Select(p => new FactorPair(p, (gnfs.PolynomialBase % p))).Distinct();
			return new FactorPairCollection(result);
		}

		// array of (p, r) where ƒ(r) % p == 0
		// quantity = 2-3 times RFB.quantity
		public static FactorPairCollection BuildAlgebraicFactorPairCollection(CancellationToken cancelToken, GNFS gnfs)
		{
			return new FactorPairCollection(PolynomialSolve.FindPolynomialRootsInRange(cancelToken, gnfs.CurrentPolynomial, gnfs.PrimeFactorBase.AlgebraicFactorBase, 0, gnfs.PrimeFactorBase.AlgebraicFactorBaseMax, 2000));
		}

		// array of (p, r) where ƒ(r) % p == 0
		// quantity =< 100
		// magnitude p > AFB.Last().p
		public static FactorPairCollection BuildQuadraticFactorPairCollection(CancellationToken cancelToken, GNFS gnfs)
		{
			return new FactorPairCollection(PolynomialSolve.FindPolynomialRootsInRange(cancelToken, gnfs.CurrentPolynomial, gnfs.PrimeFactorBase.QuadraticFactorBase, 2, gnfs.PrimeFactorBase.QuadraticFactorBaseMax, gnfs.PrimeFactorBase.QuadraticBaseCount));
		}
	}


}

