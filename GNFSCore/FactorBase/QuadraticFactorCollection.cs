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
	public class QuadraticFactorCollection : FactorCollection
	{
		public QuadraticFactorCollection(List<FactorPair> collection)
			: base(collection)
		{
		}


		// array of (p, r) where f(r) = 0 mod p		
		// quantity =< 100
		// magnitude p > AFB.Last().p
		public static class Factory
		{
			public static QuadraticFactorCollection GetQuadradicFactorBase(GNFS gnfs)
			{
				return new QuadraticFactorCollection(PolynomialModP(gnfs.Algebraic, gnfs.QuadraticPrimeBase, 2, gnfs.QuadraticFactorBaseIndexMin, 2000));
			}
		}
	}
}
