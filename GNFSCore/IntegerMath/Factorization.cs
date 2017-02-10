using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GNFSCore.IntegerMath
{
	public class Factorization
	{
		public static int LargestFactorPower(BigInteger value)
		{
			return GetPrimeFactorizationTuple(value).Select(tup => tup.Item2).OrderByDescending(i => i).First();
		}

		public static IEnumerable<Tuple<int, int>> GetPrimeFactorizationTuple(BigInteger value)
		{
			int lastPrime = -1;
			int primeCounter = 1;
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			var factorization = GetPrimeFactorization(value);
			foreach (int prime in factorization)
			{
				if (prime == lastPrime)
				{
					primeCounter += 1;
				}
				else if(lastPrime != -1)
				{
					result.Add(new Tuple<int, int>(lastPrime, primeCounter));
					primeCounter = 1;
				}

				lastPrime = prime;
			}

			result.Add(new Tuple<int, int>(lastPrime, primeCounter));
			
			if(factorization.Distinct().Count() != result.Count)
			{
				throw new Exception($"There is a bug in {nameof(Factorization.GetPrimeFactorizationTuple)}!");
			}

			return result;
		}

		public static IEnumerable<int> GetPrimeFactorization(BigInteger value)
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

			List<int> factors = new List<int>();
			List<int> eratosthenes = Eratosthenes.Sieve(value.SquareRoot()+3);
			foreach (int prime in eratosthenes)
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

		public static string GetPrimeFactorizationString(int value)
		{
			var factorization = GetPrimeFactorizationTuple(value);
			return $"{value}: {string.Join(" * ", factorization.Select(tup => $"{tup.Item1}^{tup.Item2}"))}";
		}
	}
}
