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

		public PrimeFactorization()
			: base()
		{ }

		public PrimeFactorization(CountDictionary relationFactorizationDictionary, BigInteger maxValue)
		{
			var results =
				relationFactorizationDictionary
				.ToDictionary()
				.Where(kvp => kvp.Key <= maxValue && kvp.Key != -1)
				.Select(kvp => new Factor(kvp.Key, kvp.Value));
			
			this.AddRange(results);
		}

		public PrimeFactorization(BigInteger number, BigInteger maxValue, bool mod2only = false)
		{
			Number = number;

			var factorization = FactorizationFactory.GetPrimeFactorization(Number, maxValue);

			if (mod2only)
			{
				this.AddRange(factorization.Where(factor => factor.ExponentMod2 == 1));
			}
			else
			{
				this.AddRange(factorization);
			}
		}

		public void RestrictFactors(BigInteger maxFactor)
		{
			this.RemoveAll(factor => factor.Prime > maxFactor);
		}

		public override string ToString()
		{
			return string.Join(" * ", this.Select(factor => factor.ToString()));
		}
	}
}
