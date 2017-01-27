using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GNFSCore.Prime
{
	public class Factorization
	{
		public static IEnumerable<int> GetPrimeFactoriation(int value)
		{
			List<int> eratosthenes = Eratosthenes.Sieve((int)Math.Sqrt(value));

			List<int> factors = new List<int>();
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

			factors.Add(value);

			return factors;
		}

		public static string GetPrimeFactoriationString(int value)
		{
			return $"{ string.Join(",", Factorization.GetPrimeFactoriation(value))}";			
		}
	}
}
