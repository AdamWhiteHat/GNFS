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
		public PrimeFactorization()
			: base()
		{ }

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

		public BigInteger Number;

		//public PrimeFactorization(BigInteger toFactor, BigInteger maxValue)
		//{
		//	Number = toFactor;
		//	this.AddRange(FactorizationFactory.GetPrimeFactorization(toFactor, maxValue));
		//}


		//public static implicit operator PrimeFactorization(IEnumerable<Factor> ienum)
		//{
		//	return new PrimeFactorization(ienum.ToList());
		//}

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
