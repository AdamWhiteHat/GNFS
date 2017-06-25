using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath
{
	using Internal;
	using System.Numerics;

	public static class PrimeFactory
	{
		private static BigInteger MaxValue;
		private static BigInteger[] primes;
		private static BigInteger lastPrime;

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

		private static void IncreaseMaxValue()
		{
			IncreaseMaxValue(BigInteger.Zero);
		}

		private static void IncreaseMaxValue(BigInteger newMaxValue)
		{
			// Increase bound
			MaxValue = BigInteger.Max(newMaxValue + 1000, MaxValue + 100000 /*MaxValue*/);
			SetPrimes();
		}

		public static int GetIndexFromValue(BigInteger value)
		{
			if (value == -1)
			{
				return -1;
			}
			if (primes.Last() < value)
			{
				IncreaseMaxValue(value);
			}
			BigInteger primeValue = primes.First(p => p >= value);
			int index = Array.IndexOf<BigInteger>(primes, primeValue);
			return index;
		}

		public static BigInteger GetValueFromIndex(long index)
		{
			while ((primes.LongLength - 1) < index)
			{
				IncreaseMaxValue();
			}
			return (BigInteger)primes.GetValue(index);
		}

		public static BigInteger[] GetPrimes()
		{
			return primes;
		}

		public static BigInteger[] GetPrimes(BigInteger maxValue)
		{
			if (maxValue > MaxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return primes.Take(GetIndexFromValue(maxValue)).ToArray();
		}

		public static IEnumerable<BigInteger> GetPrimeEnumerator(BigInteger startValue)
		{
			int index = GetIndexFromValue(startValue);
			BigInteger stopIndex = primes.Length - 1;

			while (index < stopIndex)
			{
				yield return primes[index];
				index++;
			}

			yield break;
		}

		public static IEnumerable<BigInteger> GetPrimesFrom(BigInteger minValue)
		{
			return primes.SkipWhile(p => p < minValue);
		}

		public static IEnumerable<BigInteger> GetPrimesTo(BigInteger maxValue)
		{
			return GetPrimesRange(0, maxValue);
		}

		public static IEnumerable<BigInteger> GetPrimesRange(BigInteger minValue, BigInteger maxValue)
		{
			if (primes.Last() < maxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return primes.SkipWhile(p => p < minValue).TakeWhile(p => p < maxValue);
		}

		public static bool IsPrime(BigInteger value)
		{
			return primes.Contains(BigInteger.Abs(value));
		}
	}
}
