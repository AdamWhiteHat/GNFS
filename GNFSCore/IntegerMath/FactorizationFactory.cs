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
		private static BigInteger[] primes;

		static FactorizationFactory()
		{
			primes = new BigInteger[] { 2, 3, 5, 7, 11 };
		}

		public static IEnumerable<Tuple<BigInteger, BigInteger>> GetPrimeFactorizationTuple(BigInteger value, BigInteger maxValue)
		{
			if (value == 0)
			{
				return new Tuple<BigInteger, BigInteger>[] { new Tuple<BigInteger, BigInteger>(0, 0) };
			}

			List<Tuple<BigInteger, BigInteger>> result = new List<Tuple<BigInteger, BigInteger>>();
			BigInteger toFactor = value;

			BigInteger lastPrime = int.MinValue;
			BigInteger primeCounter = 1;
			IEnumerable<BigInteger> factorization = GetPrimeFactorization(toFactor, maxValue);
			foreach (BigInteger prime in factorization)
			{
				if (prime == lastPrime)
				{
					primeCounter += 1;
				}
				else if (lastPrime != int.MinValue)
				{
					result.Add(new Tuple<BigInteger, BigInteger>(lastPrime, primeCounter));
					primeCounter = 1;
				}

				lastPrime = prime;
			}

			result.Add(new Tuple<BigInteger, BigInteger>(lastPrime, primeCounter));

			if (factorization.Distinct().Count() != result.Count)
			{
				throw new Exception($"There is a bug in {nameof(FactorizationFactory.GetPrimeFactorizationTuple)}!");
			}

			return result;
		}

		public static IEnumerable<BigInteger> GetPrimeFactorization(BigInteger value, BigInteger maxValue)
		{
			if (value == 0)
			{
				return new BigInteger[] { 0 };
			}

			List<BigInteger> factors = new List<BigInteger>();

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
					factors.Add(toFactor);
					return factors;
				}
			}

			if (primes.Last() < maxValue + 1)
			{
				primes = PrimeFactory.GetPrimes(maxValue + 1);
			}

			if (primes.Contains(toFactor))
			{
				factors.Add(toFactor);
				return factors;
			}

			foreach (BigInteger prime in primes)
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
				factors.Add(toFactor);
			}

			return factors;
		}

		public static BigInteger[] GetFactorizationExponents(BigInteger value, BigInteger maxValue)
		{
			return GetPrimeFactorizationTuple(value, maxValue).Select(tup => tup.Item2).OrderByDescending(i => i).ToArray();
		}

		public static bool IsSmoothOverFactorBase(BigInteger n, IEnumerable<BigInteger> factorBase)
		{
			BigInteger result = BigInteger.Abs(n);
			BigInteger sqrt = result.SquareRoot();

			foreach (BigInteger factor in factorBase)
			{
				while (result % factor == 0 && result != 1)
				{
					result /= factor;
				}

				if (result == 0 || result == 1 || factor > sqrt)
				{
					break;
				}
				else if (result > 1)
				{
					if (factorBase.Contains(result))
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
