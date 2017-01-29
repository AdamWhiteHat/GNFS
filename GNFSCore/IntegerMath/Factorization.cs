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
		public static IEnumerable<int> GetPrimeFactoriation(int value)
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException("Value must be greater than zero");
			}

			if (value < 10)
			{
				//if (value == 9)
				//{
				//	return new List<int>() { 3, 3, 3 };
				//}
				//else if (value == 8)
				//{
				//	return new List<int>() { 2, 2, 2 };
				//}
				//else if (value == 6)
				//{
				//	return new List<int>() { 2, 3 };
				//}
				//else if (value == 4)
				//{
				//	return new List<int>() { 2, 2 };
				//}				else
				if (value == 0 || value == 1 || value == 2 || value == 3 || value == 5 || value == 7)
				{
					return new List<int>() { value };
				}
			}

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
