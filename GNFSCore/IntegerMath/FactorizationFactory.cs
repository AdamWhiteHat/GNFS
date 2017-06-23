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
		private static int[] primes;

		static FactorizationFactory()
		{
			primes = new int[] { 2, 3, 5, 7, 11 };
		}

		public static IEnumerable<Tuple<int, int>> GetPrimeFactorizationTuple(BigInteger value, int maxValue)
		{
			if (value == 0)
			{
				return new Tuple<int, int>[] { new Tuple<int, int>(0, 0) };
			}

			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			BigInteger toFactor = value;

			int lastPrime = int.MinValue;
			int primeCounter = 1;
			IEnumerable<int> factorization = GetPrimeFactorization(toFactor, maxValue);
			foreach (int prime in factorization)
			{
				if (prime == lastPrime)
				{
					primeCounter += 1;
				}
				else if (lastPrime != int.MinValue)
				{
					result.Add(new Tuple<int, int>(lastPrime, primeCounter));
					primeCounter = 1;
				}

				lastPrime = prime;
			}

			result.Add(new Tuple<int, int>(lastPrime, primeCounter));

			if (factorization.Distinct().Count() != result.Count)
			{
				throw new Exception($"There is a bug in {nameof(FactorizationFactory.GetPrimeFactorizationTuple)}!");
			}

			return result;
		}

		public static IEnumerable<int> GetPrimeFactorization(BigInteger value, int maxValue)
		{
			if (value == 0)
			{
				return new int[] { 0 };
			}

			List<int> factors = new List<int>();

			BigInteger toFactor = value;
			if (toFactor < 0)
			{
				factors.Add(-1);
				toFactor = BigInteger.Abs(toFactor);
			}

			if (toFactor < 10)
			{
				if (toFactor == 1 || toFactor == 2 || toFactor == 3 || toFactor == 5 || toFactor == 7)
				{
					factors.Add((int)toFactor);
					return factors;
				}
			}

			if (primes.Last() < maxValue + 1)
			{
				primes = PrimeFactory.GetPrimes(maxValue + 1);
			}

			if (primes.Contains((int)toFactor))
			{
				factors.Add((int)toFactor);
				return factors;
			}

			foreach (int prime in primes)
			{
				while (toFactor % prime == 0)
				{
					toFactor /= prime;
					factors.Add(prime);
				}

				if (toFactor == 1)
				{
					break;
				}
			}

			if (toFactor != 1)
			{
				factors.Add((int)toFactor);
			}

			return factors;
		}

		public static int[] GetFactorizationExponents(BigInteger value, int maxValue)
		{
			return GetPrimeFactorizationTuple(value, maxValue).Select(tup => tup.Item2).OrderByDescending(i => i).ToArray();
		}

		public static bool IsSmoothOverFactorBase(BigInteger n, IEnumerable<int> factorBase)
		{
			BigInteger result = BigInteger.Abs(n);
			BigInteger sqrt = result.SquareRoot();

			foreach (int factor in factorBase)
			{
				while (result % factor == 0 && result != 1)
				{
					result /= factor;
				}

				if (result == 0 || result == 1 || factor > sqrt)
				{
					break;
				}
				else if (result > 1 && result < (int.MaxValue - 1))
				{
					if (factorBase.Contains((int)result))
					{
						return true;
					}
				}
			}

			return (result == 1);
		}

		public static class FormatString
		{
			public static string PrimeFactorization(IEnumerable<Tuple<int, int>> factorization)
			{
				return $"{string.Join(" * ", factorization.Select(tup => $"{tup.Item1}^{tup.Item2}"))}";
			}
		}
	}
}
