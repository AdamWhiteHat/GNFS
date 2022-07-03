using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GNFSCore.IntegerMath
{
	public static partial class FactorizationFactory
	{
		/*
		public static IEnumerable<BigInteger> GetPrimeFactorCollection(BigInteger value, BigInteger maxValue)
		{
			if (value == 0)
			{
				return new BigInteger[] { };
			}

			List<BigInteger> factors = new List<BigInteger>();

			BigInteger toFactor = value;
			if (toFactor < 0)
			{
				//factors.Add(-1);
				toFactor = BigInteger.Abs(toFactor);
			}

			if (toFactor < 2)
			{
				if (toFactor == 1)
				{
					return new BigInteger[] { toFactor };
				}
			}

			if (PrimeFactory.IsPrime(toFactor))
			{
				factors.Add(toFactor);
				return factors;
			}

			foreach (BigInteger prime in PrimeFactory.GetPrimesTo(maxValue).ToList())
			{
				while (toFactor % prime == 0)
				{
					toFactor /= prime;
					factors.Add(prime);

					if (BigInteger.Abs(toFactor) == 1)
					{
						break;
					}
				}

				if (BigInteger.Abs(toFactor) == 1)
				{
					break;
				}
			}

			if (BigInteger.Abs(toFactor) != 1)
			{
				if (PrimeFactory.IsPrime(toFactor))
				{
					factors.Add(toFactor);
				}

				return factors;
			}
			else if (!factors.Any())
			{
				return new BigInteger[] { };
			}
			else
			{
				return factors;
			}
		}
		*/

		private static BigInteger[] primeCheckBases = new BigInteger[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };
		public static bool IsProbablePrime(BigInteger input)
		{
			if (input == 2 || input == 3)
			{
				return true;
			}
			if (input < 2 || input % 2 == 0)
			{
				return false;
			}

			BigInteger d = input - 1;
			int s = 0;

			while (d % 2 == 0)
			{
				d /= 2;
				s += 1;
			}

			foreach (BigInteger a in primeCheckBases)
			{
				BigInteger x = BigInteger.ModPow(a, d, input);
				if (x == 1 || x == input - 1)
				{
					continue;
				}

				for (int r = 1; r < s; r++)
				{
					x = BigInteger.ModPow(x, 2, input);
					if (x == 1)
					{
						return false;
					}
					if (x == input - 1)
					{
						break;
					}
				}

				if (x != input - 1)
				{
					return false;
				}
			}

			return true;
		}
	}
}
