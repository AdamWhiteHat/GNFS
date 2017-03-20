using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath
{
	using Internal;

	public static class PrimeFactory
	{
		private static int MaxValue;
		private static int[] primes;

		static PrimeFactory()
		{
			MaxValue = 2000;
			SetPrimes();
		}

		private static void SetPrimes()
		{
			primes = Eratosthenes.Sieve(MaxValue).ToArray();
		}

		private static void IncreaseMaxValue(int newMaxValue = 0)
		{
			// Increase bound
			MaxValue = Math.Max(newMaxValue + 1, MaxValue + MaxValue);
			SetPrimes();
		}

		public static int GetIndexFromValue(int value)
		{
			while (primes.Last() < value)
			{
				IncreaseMaxValue();
			}
			int primeValue = primes.First(p => p >= value);
			int index = Array.IndexOf<int>(primes, primeValue);
			return index;
		}

		public static int GetValueFromIndex(int index)
		{
			while ((primes.Length - 1) < index)
			{
				IncreaseMaxValue();
			}
			int value = primes[index];
			return value;
		}

		public static int[] GetPrimes(int maxValue)
		{
			if (maxValue > MaxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return primes.Take(GetIndexFromValue(maxValue)).ToArray();
		}

		public static bool IsPrime(int value)
		{
			return primes.Contains(Math.Abs(value));
		}
	}
}
