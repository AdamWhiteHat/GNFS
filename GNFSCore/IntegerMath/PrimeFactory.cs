using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.IntegerMath
{
	using Internal;

	public class PrimeFactory
	{
		private static int bound;
		private static int[] primes;

		static PrimeFactory()
		{
			bound = 2000;
			SetPrimes();
		}

		private static void SetPrimes()
		{
			primes = Eratosthenes.Sieve(bound).ToArray();
		}

		private static void IncreaseBound(int newMaxCount = 0)
		{
			// Increase bound
			bound = Math.Max(newMaxCount + 1, bound + bound);
			SetPrimes();
		}

		public static int GetPrimeIndexFromValue(int value)
		{
			while(primes[primes.Length-1] < value)
			{
				IncreaseBound();
			}
			return primes.First(p => p >= value);
		}

		public static int[] GetPrimes(int quantity)
		{
			if (quantity > bound)
			{
				IncreaseBound(quantity);
			}
			return primes.Take(quantity).ToArray();
		}
	}
}
