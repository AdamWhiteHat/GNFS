using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
{
	public class FactorBase
	{
		public BigInteger MaxRationalFactorBase { get; internal set; }
		public BigInteger MaxAlgebraicFactorBase { get; internal set; }
		public BigInteger MinQuadraticFactorBase { get; internal set; }
		public BigInteger MaxQuadraticFactorBase { get; internal set; }

		[JsonIgnore]
		public List<BigInteger> RationalFactorBase { get; internal set; }
		[JsonIgnore]
		public List<BigInteger> AlgebraicFactorBase { get; internal set; }
		[JsonIgnore]
		public List<BigInteger> QuadraticFactorBase { get; internal set; }
	}
}
