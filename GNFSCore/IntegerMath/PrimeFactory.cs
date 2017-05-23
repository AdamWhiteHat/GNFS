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
			if (primes.Last() < value)
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
			if (maxValue > MaxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return primes.Take(GetIndexFromValue(maxValue)).ToArray();
		}

		public static IEnumerable<int> GetPrimeEnumerator(int startValue)
		{
			int index = GetIndexFromValue(startValue);
			int stopIndex = primes.Length - 1;

			while (index < stopIndex)
			{
				yield return primes[index];
				index++;
			}

			yield break;
		}

		public static IEnumerable<int> GetPrimesFrom(int minValue)
		{
			return primes.SkipWhile(p => p < minValue);
		}

		public static IEnumerable<int> GetPrimesTo(int maxValue)
		{
			return GetPrimesRange(0, maxValue);
		}

		public static IEnumerable<int> GetPrimesRange(int minValue, int maxValue)
		{
			if (primes.Last() < maxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return primes.SkipWhile(p => p < minValue).TakeWhile(p => p < maxValue);
		}

		public static bool IsPrime(int value)
		{
			return primes.Contains(Math.Abs(value));
		}
	}
}
