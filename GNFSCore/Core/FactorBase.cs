using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public class FactorBase
	{
		public BigInteger MaxRationalFactorBase { get; internal set; }
		public BigInteger MaxAlgebraicFactorBase { get; internal set; }
		public BigInteger MinQuadraticFactorBase { get; internal set; }
		public BigInteger MaxQuadraticFactorBase { get; internal set; }

		public List<BigInteger> RationalFactorBase { get; internal set; }
		public List<BigInteger> AlgebraicFactorBase { get; internal set; }
		public List<BigInteger> QuadraticFactorBase { get; internal set; }
	}
}
