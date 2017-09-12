using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
{
	public class PrimeBase
	{
		public BigInteger RationalFactorBase { get; internal set; }
		public BigInteger AlgebraicFactorBase { get; internal set; }
		public BigInteger QuadraticFactorBaseMin { get; internal set; }
		public BigInteger QuadraticFactorBaseMax { get; internal set; }

		[JsonIgnore]
		public List<BigInteger> RationalPrimeBase { get; internal set; }
		[JsonIgnore]
		public List<BigInteger> AlgebraicPrimeBase { get; internal set; }
		[JsonIgnore]
		public List<BigInteger> QuadraticPrimeBase { get; internal set; }
	}
}
