using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.IntegerMath
{
	public class Factor
	{
		public BigInteger Prime;
		public BigInteger Exponent;
		public BigInteger ExponentMod2 { get { return Exponent % 2; } }

		public Factor(BigInteger prime, BigInteger exponent)
		{
			Prime = prime;
			Exponent = exponent;
		}

		public override string ToString()
		{
			return $"{Prime}^{Exponent}";
		}
	}

	public class PrimeFactorization : List<Factor>
	{
		public BigInteger Number;

		public PrimeFactorization(CountDictionary relationFactorizationDictionary, BigInteger maxValue)
		{
			var results =
				relationFactorizationDictionary
				.ToDictionary()
				.Where(kvp => kvp.Key <= maxValue && kvp.Key != -1)
				.Select(kvp => new Factor(kvp.Key, kvp.Value));

			this.AddRange(results);
		}

		public override string ToString()
		{
			return string.Join(" * ", this.Select(factor => factor.ToString()));
		}
	}
}
