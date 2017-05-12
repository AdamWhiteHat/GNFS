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
		protected FactorCollection()
			: base()
		{
		}

		protected FactorCollection(List<FactorPair> collection)
			: base(collection)
		{
		}

		protected static List<FactorPair> PolynomialModP(AlgebraicPolynomial polynomial, IEnumerable<int> primes, int rangeFrom, int rangeTo, int totalFactorPairs)
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
