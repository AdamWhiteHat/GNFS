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
		private static int lastPrime;

		static PrimeFactory()
		{
			MaxValue = 1000;
			SetPrimes();
		}

		private static void SetPrimes()
		{
			primes = Eratosthenes.Sieve(MaxValue).ToArray();
			lastPrime = primes.Last();
		}

		private static void IncreaseMaxValue(int newMaxValue = 0)
		{
			// Increase bound
			MaxValue = Math.Max(newMaxValue + 1000, MaxValue + 100000 /*MaxValue*/);
			SetPrimes();
		}

		public static int GetIndexFromValue(int value)
		{
			if (value == -1)
			{
				return -1;
			}
			while (primes.Last() < value)
			{
				IncreaseMaxValue(value);
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
			while (maxValue > MaxValue)
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
