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
			primes = PrimeFactory.GetPrimes(1000);
		}

		public static IEnumerable<Tuple<int, int>> GetPrimeFactorizationTuple(BigInteger value, int maxValue)
		{
			int lastPrime = -1;
			int primeCounter = 1;
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			var factorization = GetPrimeFactorization(value, maxValue);
			foreach (int prime in factorization)
			{
				if (prime == lastPrime)
				{
					primeCounter += 1;
				}
				else if (lastPrime != -1)
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
			value = BigInteger.Abs(value);

			if (value == 0)
			{
				return new int[] { 0 };
			}

			if (value < 10)
			{
				if (value == 0 || value == 1 || value == 2 || value == 3 || value == 5 || value == 7)
				{
					return new List<int>() { (int)value };
				}
			}

			if (primes.Last() < maxValue + 1)
			{
				primes = PrimeFactory.GetPrimes(maxValue + 1);
			}

			if (primes.Contains((int)value))
			{
				return new List<int>() { (int)value };
			}

			List<int> factors = new List<int>();
			foreach (int prime in primes)
			{
				while (value % prime == 0)
				{
					value /= prime;
					factors.Add(prime);
				}

				if (value == 1)
				{
					break;
				}
			}

			if (value != 1)
			{
				factors.Add((int)value);
			}

			return factors;
		}

		public static int[] GetFactorizationExponents(BigInteger value, int maxValue)
		{
			return GetPrimeFactorizationTuple(value, maxValue).Select(tup => tup.Item2).OrderByDescending(i => i).ToArray();
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
