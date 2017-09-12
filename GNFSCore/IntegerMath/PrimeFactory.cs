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
		private static IEnumerable<BigInteger> primes;
		private static BigInteger lastPrime;

		static PrimeFactory()
		{
			MaxValue = 10000;
			SetPrimes();
		}

		private static void SetPrimes()
		{
			primes = Eratosthenes.Sieve((Int32)MaxValue);
			lastPrime = primes.Last();
		}

		public static IEnumerable<BigInteger> GetPrimes()
		{
			return primes;
		}

		public static IEnumerable<BigInteger> GetPrimes(BigInteger maxValue)
		{
			if (maxValue > MaxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return primes.Take(GetIndexFromValue(maxValue));
		}

		public static IEnumerable<BigInteger> GetPrimeEnumerator(BigInteger startValue)
		{
			int index = GetIndexFromValue(startValue);
			BigInteger stopIndex = primes.Count() - 1;

			while (index < stopIndex)
			{
				yield return primes.ElementAt(index);
				index++;
			}

			yield break;
		}

		private static void IncreaseMaxValue()
		{
			IncreaseMaxValue(BigInteger.Zero);
		}

		private static void IncreaseMaxValue(BigInteger newMaxValue)
		{
			// Increase bound
			BigInteger temp = BigInteger.Max(newMaxValue + 1000, MaxValue + 100000 /*MaxValue*/);
			MaxValue = BigInteger.Min(temp, (Int32.MaxValue - 1));
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

			int index = Array.IndexOf<BigInteger>(primes.ToArray(), primeValue);
			return index;
		}

		public static BigInteger GetValueFromIndex(int index)
		{
			while ((primes.LongCount() - 1) < index)
			{
				IncreaseMaxValue();
			}
			return primes.ElementAt(index);
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

		public static BigInteger GetNextPrime(BigInteger fromValue)
		{
			BigInteger result = fromValue;

			if(result.IsEven)
			{
				result += 1;
			}

			while(!FactorizationFactory.IsPrime(result))
			{
				result += 2;
			}

			return result;
		}
	}
}
