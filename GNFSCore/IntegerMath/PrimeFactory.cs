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
		private static List<BigInteger> primes = new List<BigInteger>() { 2, 3, 5, 7, 11, 13 };

		static PrimeFactory()
		{
			MaxValue = 10;
			SetPrimes();
		}

		private static void SetPrimes()
		{
			primes = Eratosthenes.Sieve((Int32)MaxValue).ToList();
		}

		public static IEnumerable<BigInteger> GetPrimes()
		{
			return primes.AsEnumerable();
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
			BigInteger stopIndex = primes.Count - 1;

			while (index < stopIndex)
			{
				yield return primes[index];
				index++;
			}

			yield break;
		}

		private static void IncreaseMaxValue()
		{
			IncreaseMaxValue(BigInteger.Zero);
		}

		public static void IncreaseMaxValue(BigInteger newMaxValue)
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

			int index = primes.IndexOf(primeValue) + 1;
			return index;
		}

		public static BigInteger GetValueFromIndex(int index)
		{
			while ((primes.Count - 1) < index)
			{
				IncreaseMaxValue();
			}
			return primes.ElementAt(index);
		}

		public static BigInteger GetApproximateValueFromIndex(UInt64 n)
		{
			if (n < 6)
			{
				return primes[(int)n];
			}

			double fn = (double)n;
			double flogn = Math.Log(n);
			double flog2n = Math.Log(flogn);

			double upper;

			if (n >= 688383)    /* Dusart 2010 page 2 */
			{
				upper = fn * (flogn + flog2n - 1.0 + ((flog2n - 2.00) / flogn));
			}
			else if (n >= 178974)    /* Dusart 2010 page 7 */
			{
				upper = fn * (flogn + flog2n - 1.0 + ((flog2n - 1.95) / flogn));
			}
			else if (n >= 39017)    /* Dusart 1999 page 14 */
			{
				upper = fn * (flogn + flog2n - 0.9484);
			}
			else                    /* Modified from Robin 1983 for 6-39016 _only_ */
			{
				upper = fn * (flogn + 0.6000 * flog2n);
			}

			if (upper >= (double)UInt64.MaxValue)
			{
				throw new OverflowException($"{upper} > {UInt64.MaxValue}");
			}

			return new BigInteger((UInt64)Math.Ceiling(upper));
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
			BigInteger result = fromValue + 1;

			if (result.IsEven)
			{
				result += 1;
			}

			while (!FactorizationFactory.IsProbablePrime(result))
			{
				result += 2;
			}

			return result;
		}
	}
}
