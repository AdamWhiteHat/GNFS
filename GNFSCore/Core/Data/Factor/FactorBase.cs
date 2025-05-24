using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GNFSCore.Data
{
	public class FactorBase
	{
		public FactorBase()
		{
			RationalFactorBase = new List<BigInteger>();
			AlgebraicFactorBase = new List<BigInteger>();
			QuadraticFactorBase = new List<BigInteger>();
		}

		[JsonProperty]
		public BigInteger RationalFactorBaseMax { get; internal set; }
		[JsonProperty]
		public BigInteger AlgebraicFactorBaseMax { get; internal set; }
		[JsonProperty]
		public BigInteger QuadraticFactorBaseMin { get; internal set; }
		[JsonProperty]
		public BigInteger QuadraticFactorBaseMax { get; internal set; }
		[JsonProperty]
		public int QuadraticBaseCount { get; internal set; }
		[JsonIgnore]
		public IEnumerable<BigInteger> RationalFactorBase { get; internal set; }
		[JsonIgnore]
		public IEnumerable<BigInteger> AlgebraicFactorBase { get; internal set; }
		[JsonIgnore]
		public IEnumerable<BigInteger> QuadraticFactorBase { get; internal set; }
	}
}
