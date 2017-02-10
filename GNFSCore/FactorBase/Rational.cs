using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class Rational
	{
		// m = polynomial base
		// array of (p, p mod m) up to bound
		// quantity = phi(bound)
		public static class Factory
		{
			public static IEnumerable<Tuple<int, int>> BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<int> primes = gnfs.Primes.Take(gnfs.PrimeBound);
				IEnumerable<Tuple<int, int>> result = primes.Select(p => new Tuple<int, int>(p, (int)(gnfs.AlgebraicPolynomial.Base % p)));
				return result;
			}
		}
		
		public static BigInteger Norm(int a, int b, BigInteger polyBase)
		{
			return BigInteger.Add(a, BigInteger.Multiply(b, polyBase));
		}

		internal static bool IsSmooth(int number, IEnumerable<int> primeFactorBase)
		{
			IEnumerable<int> primeFactorization = Factorization.GetPrimeFactorization(number);			
			return primeFactorization.Count() > 1 && primeFactorization.All(p => primeFactorBase.Contains(p));
		}

		public static IEnumerable<Tuple<int, int>> GetRationalNormsElements(GNFS gnfs, int range)
		{
			int m = (int)gnfs.AlgebraicPolynomial.Base;
			var result = new List<Tuple<int, int>>();
			int relationsNeeded = gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count();

			int counter = 0;
			while (result.Count < relationsNeeded && counter++ < 100)
			{
				result.AddRange(
					gnfs.Primes.SelectMany(p =>
						Enumerable.Range(1, range / 2).SelectMany(b =>
							   GetDivisibleElements(b, p, m, range).Select(a => new Tuple<int, int>(a, b))
						)
					)
				);
			}
			return result;
		}
		
		internal static IEnumerable<int> GetDivisibleElements(int b, int p, int m, int range)
		{
			int bm = b * m;

			int kLower = (-range + bm) / p;
			int kUpper = (range + bm) / p;
			int count = Math.Abs(kLower) + Math.Abs(kUpper);

			var divisible = Enumerable.Range(kLower, count).Select(k => -bm + (p * k));
			var rationalNormsA = divisible.Where(a => CoPrime.IsCoprime(a, b)).Distinct();
			return rationalNormsA;
		}
	}
}
