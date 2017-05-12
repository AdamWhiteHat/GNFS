using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using ExtendedNumerics;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class RationalFactorCollection : FactorCollection
	{
		public RationalFactorCollection(List<FactorPair> collection)
			: base(collection)
		{
		}
		// array of (p, p mod m) up to bound
		// quantity = phi(bound)
		public static class Factory
		{
			public static RationalFactorCollection BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<FactorPair> result = gnfs.RationalPrimeBase.Select(p => new FactorPair(p, (int)(gnfs.Algebraic.Base % p))).Distinct();
				return new RationalFactorCollection(result.ToList());
			}
		}
	}
}
