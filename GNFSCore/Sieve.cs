using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class Sieve
	{
		// The elements (a, b) with rational norm divisible by element (p, r) from RFB
		// are those with a on the form a = −bm + kp for k ∈ Z.
		public static IEnumerable<Tuple<int, int>> LineSieve(GNFS gnfs, int range)
		{
			var rationalNormsA = Rational.GetRationalNormsElements(gnfs, range);
			return rationalNormsA;
		}

		public static IEnumerable<Tuple<int, int>> Smooth(GNFS gnfs, IEnumerable<Tuple<int, int>> rels)
		{
			var primeBase = gnfs.Primes.Take(gnfs.PrimeBound);
			var rationalSmooth = rels.Where(a => Rational.IsSmooth(a.Item1, primeBase) && Rational.IsSmooth(a.Item2, primeBase));
			var algebraicSmooth = rationalSmooth.Where(a => Rational.IsSmooth((int)Algebraic.Norm(a.Item1, a.Item2, gnfs.AlgebraicPolynomial), primeBase));

			var ordered = algebraicSmooth.OrderByDescending(tupl => tupl.Item1);
			var results = ordered.Distinct();			
			return results;
		}

		//public static IEnumerable<Tuple<int, int>> LineSieveForRelations(Irreducible poly, int range, int quantity, int bound)
		//{
		//	int b = 1;
		//	int m = (int)poly.Base;
		//	var primes = Eratosthenes.Sieve(bound);
		//	var rangeA = Enumerable.Range(1/*-range*/, range);
		//	var rangeK = Enumerable.Range(1, range);

		//	var result = new List<Tuple<int, int>>();
		//	while (result.Count < quantity)
		//	{
		//		int bm = b * m;

		//		IEnumerable<int> coprimes = rangeA.Where(a => CoPrime.IsCoprime(a, b));

		//		foreach (int a in coprimes)
		//		{
		//			int abm = a + bm;

		//			IEnumerable<int> divisors = primes.Where(p => (abm % p) == 0);

		//			var lnabm = Math.Log(abm);
		//			var lnp = divisors.Select(p => Math.Floor(Math.Log(p)));

		//			if (!divisors.Any())
		//			{
		//				continue;
		//			}

		//			if (!Rational.IsSmooth(abm, primes))
		//			{
		//				continue;
		//			}

					//if (!IsSmooth(Algebraic.Norm(a, b, poly), primes))
					//{
					//	continue;
					//}

					//Tuple<int, int> relation = new Tuple<int, int>(a, b);
					//if (result.Contains(relation))
					//{
					//	continue;
					//}

					//result.Add(relation);


					//foreach (int k in rangeK)
					//{
					//	IEnumerable<BigInteger> toDivide = divisors.Select(p => -bm + (k * p)).Except(new BigInteger[] { -a, -1, 0, 1, a });

					//	foreach (BigInteger p in toDivide)
					//	{
					//		BigInteger quotientA = new BigInteger(a);

					//		BigInteger remainder = new BigInteger(-1);
					//		do
					//		{
					//			BigInteger quotient = BigInteger.DivRem(quotientA, p, out remainder);

					//			if(remainder == 0)
					//			{
					//				quotientA = BigInteger.Divide(quotientA, p);
					//			}
					//		}
					//		while (remainder == 0 && quotientA != 1);
					//
					//		if (quotientA == 1)
					//		{
					//			Tuple<int, int> relation = new Tuple<int, int>(a, b);
					//			if (!result.Contains(relation))
					//			{
					//				result.Add(relation);
					//			}
					//		}
					//
					//		if (result.Count >= quantity)
					//		{
					//			break;
					//		}
					//	}
					//
					//if (result.Count >= quantity)
					//{
					//	break;
					//}
					//}

		//			if (result.Count >= quantity)
		//			{
		//				break;
		//			}
		//		}

		//		b++;
		//	}

		//	return result;
		//}



		//internal static int GetSmoothnessValue(int number, List<int> primeFactorBase)
		//{
		//	IEnumerable<int> primeFactorization = Factorization.GetPrimeFactoriation(number);
		//	if (primeFactorization.Any(p => !primeFactorBase.Contains(p)))
		//	{
		//		return 0;
		//	}
		//	return primeFactorization.Count();
		//}

		//private static bool IsSmooth(Tuple<int, int> pair, Polynomial poly, int threshold)
		//{
		//	BigInteger rationalNorm = BigInteger.Abs(Rational.Norm(pair.Item1, pair.Item2, poly.Base));
		//	BigInteger algebraicNorm = BigInteger.Abs(Algebraic.Norm(pair.Item1, pair.Item2, poly));

		//	if (rationalNorm.IsZero || algebraicNorm.IsZero)
		//	{
		//		return false;
		//	}

		//	IEnumerable<int> rationalNormPrimeFactorization = Factorization.GetPrimeFactoriation((int)rationalNorm);
		//	IEnumerable<int> algebraicNormPrimeFactorization = Factorization.GetPrimeFactoriation((int)algebraicNorm);

		//	return (rationalNormPrimeFactorization.Count() > threshold && algebraicNormPrimeFactorization.Count() > threshold);
		//}
	}
}
