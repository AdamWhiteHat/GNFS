using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using ExtendedNumerics;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class FactorCollection : List<FactorPair>
	{
		public FactorCollection()
			: base()
		{
		}

		public FactorCollection(List<FactorPair> collection)
			: base(collection)
		{
		}

		public static class Factory
		{
			// array of (p, r) where f(r) = 0 mod p
			// quantity = 2-3 times RFB.quantity
			public static FactorCollection GetAlgebraicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(FactorCollection.PolynomialModP(gnfs.Algebraic, gnfs.AlgebraicPrimeBase, 0, gnfs.AlgebraicFactorBase, 2000));
			}

			// array of (p, p mod m) up to bound
			// quantity = phi(bound)
			public static FactorCollection BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<FactorPair> result = gnfs.RationalPrimeBase.Select(p => new FactorPair(p, (int)(gnfs.Algebraic.Base % p))).Distinct();
				return new FactorCollection(result.ToList());
			}

			// array of (p, r) where f(r) = 0 mod p		
			// quantity =< 100
			// magnitude p > AFB.Last().p
			public static FactorCollection GetQuadradicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(FactorCollection.PolynomialModP(gnfs.Algebraic, gnfs.QuadraticPrimeBase, 2, gnfs.QuadraticFactorBaseMin, 2000));
			}
		}

		public static List<FactorPair> PolynomialModP(AlgebraicPolynomial polynomial, IEnumerable<int> primes, int rangeFrom, int rangeTo, int totalFactorPairs)
		{
			List<FactorPair> result = new List<FactorPair>();

			int r = rangeFrom;
			while (r < rangeTo && result.Count < totalFactorPairs)
			{
				IEnumerable<int> modList = primes.Where(p => p > r);
				List<int> roots = polynomial.GetRootsMod(r, modList);
				if (roots.Any())
				{
					result.AddRange(roots.Select(p => new FactorPair(p, r)));
				}
				r++;
			}

			return result.OrderBy(tup => tup.P).ToList();
		}

		public override string ToString()
		{
			return string.Join("\t", this.Select(factr => factr.ToString()));
		}
	}
}
