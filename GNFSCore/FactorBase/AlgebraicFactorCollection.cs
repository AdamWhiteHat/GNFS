using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using ExtendedNumerics;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class AlgebraicFactorCollection : FactorCollection
	{
		public AlgebraicFactorCollection(List<FactorPair> collection)
			: base(collection)
		{
		}
		// array of (p, r) where f(r) = 0 mod p
		// quantity = 2-3 times RFB.quantity
		public static class Factory
		{
			public static AlgebraicFactorCollection GetAlgebraicFactorBase(GNFS gnfs)
			{
				return new AlgebraicFactorCollection(PolynomialModP(gnfs.Algebraic, gnfs.AlgebraicPrimeBase, 0, gnfs.AlgebraicFactorBase, 2000));
			}
		}

	}
}

