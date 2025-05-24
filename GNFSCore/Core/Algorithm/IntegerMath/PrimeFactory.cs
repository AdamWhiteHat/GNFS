using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.Algorithm.IntegerMath
{
	public static class PrimeFactory
	{
		private static BigInteger MaxValue = 10;

		private static int primesCount;
		private static BigInteger primesLast;
		private static List<BigInteger> primes = new List<BigInteger>() { 2, 3, 5, 7, 11, 13 };

		static PrimeFactory()
		{
			SetPrimes();
		}

		private static void SetPrimes()
		{
			primes = FastPrimeSieve.GetRange(2, (int)MaxValue).ToList();
			primesCount = primes.Count;
			primesLast = primes.Last();
		}

		public static IEnumerable<BigInteger> GetPrimeEnumerator(int startIndex = 0, int stopIndex = -1)
		{
			int index = startIndex;
			int maxIndex = stopIndex > 0 ? stopIndex : primesCount - 1;
			while (index < maxIndex)
			{
				yield return primes[index];
				index++;
			}
			yield break;
		}

		public static void IncreaseMaxValue(BigInteger newMaxValue)
		{
			// Increase bound
			BigInteger temp = BigInteger.Max(newMaxValue + 1000, MaxValue + 100000 /*MaxValue*/);
			MaxValue = BigInteger.Min(temp, int.MaxValue - 1);
			SetPrimes();
		}

		public static int GetIndexFromValue(BigInteger value)
		{
			if (value == -1)
			{
				return -1;
			}
			if (primesLast < value)
			{
				IncreaseMaxValue(value);
			}

			BigInteger primeValue = primes.First(p => p >= value);

			int index = primes.IndexOf(primeValue) + 1;
			return index;
		}

		public static BigInteger GetApproximateValueFromIndex(ulong n)
		{
			if (n < 6)
			{
				return primes[(int)n];
			}

			double fn = n;
			double flogn = Math.Log(n);
			double flog2n = Math.Log(flogn);

			double upper;

			if (n >= 688383)    /* Dusart 2010 page 2 */
			{
				upper = fn * (flogn + flog2n - 1.0 + (flog2n - 2.00) / flogn);
			}
			else if (n >= 178974)    /* Dusart 2010 page 7 */
			{
				upper = fn * (flogn + flog2n - 1.0 + (flog2n - 1.95) / flogn);
			}
			else if (n >= 39017)    /* Dusart 1999 page 14 */
			{
				upper = fn * (flogn + flog2n - 0.9484);
			}
			else                    /* Modified from Robin 1983 for 6-39016 _only_ */
			{
				upper = fn * (flogn + 0.6000 * flog2n);
			}

			if (upper >= ulong.MaxValue)
			{
				throw new OverflowException($"{upper} > {ulong.MaxValue}");
			}

			return new BigInteger((ulong)Math.Ceiling(upper));
		}

		public static IEnumerable<BigInteger> GetPrimesFrom(BigInteger minValue)
		{
			return GetPrimeEnumerator(GetIndexFromValue(minValue));
		}

		public static IEnumerable<BigInteger> GetPrimesTo(BigInteger maxValue)
		{
			if (primesLast < maxValue)
			{
				IncreaseMaxValue(maxValue);
			}
			return GetPrimeEnumerator(0).TakeWhile(p => p < maxValue);
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
