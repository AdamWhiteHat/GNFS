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
				int boundIndex = PrimeFactory.GetIndexFromValue(gnfs.PrimeBound);
				IEnumerable<int> primes = PrimeFactory.GetPrimes(boundIndex);
				IEnumerable<Tuple<int, int>> result = primes.Select(p => new Tuple<int, int>(p, (int)(gnfs.AlgebraicPolynomial.Base % p)));
				return result.Distinct();
			}
		}
		/*
		public static BigInteger Norm(int b, BigInteger baseM, int k, int prime)
		{
			return BigInteger.Add(BigInteger.Multiply(-b, baseM), BigInteger.Multiply(k, prime));
		}

		public static bool IsSmooth(int number, IEnumerable<int> primeFactorBase)
		{
			IEnumerable<int> primeFactorization = Factorization.GetPrimeFactorization(number,primeFactorBase.Last());
			return primeFactorization.Count() > 1 && primeFactorization.All(p => primeFactorBase.Contains(p));
		}

		public static IEnumerable<Tuple<int, int>> GetRationalNormRelations(GNFS gnfs, int range)
		{
			int m = (int)gnfs.AlgebraicPolynomial.Base;
			var result = new List<Tuple<int, int>>();
			int relationsNeeded = gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count();

			int b = 1;
			while (result.Count < relationsNeeded && b < 1000)
			{
				int bm = -b * m;

				result.AddRange(
					gnfs.RFB.SelectMany(tup =>
						GetDivisibleElements(gnfs, b, m, tup.Item1, range)
						.Select(a => new Tuple<int, int>(a, b))
					)
				);

				b++;
			}
			return result.Distinct();
		}

		internal static IEnumerable<int> GetDivisibleElements(GNFS gnfs, int b, int m, int p, int range)
		{
			int bm = b * m;
			var divisible = GetNormsRange(range, bm, p).Select(k => -bm + (p * k)).Distinct();
			var primeBase = PrimeFactory.GetPrimes(gnfs.PrimeBound);
			var smoothNorms = divisible.Where(a => IsSmooth(a, primeBase));
			var rationalNorms = smoothNorms.Where(a => CoPrime.IsCoprime(a, b));
			return rationalNorms;
		}

		internal static IEnumerable<int> GetNormsRange(int range, int bm, int p)
		{

			int kLower = (0 + bm) / p; // (-range + bm) / p;
			int kUpper = (range + bm) / p;
			int count = Math.Abs(kLower) + Math.Abs(kUpper);

			var result = Enumerable.Range(kLower, count);
			return result;
		}
		*/
	}
}
