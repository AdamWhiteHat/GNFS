using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.FactorBase;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class Sieve
	{
		public static IEnumerable<Tuple<int, int>> LineSieveForRelations(Polynomial poly, int range, int quantity, int threshold)
		{
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();
			IEnumerable<int> sieveNumbers = Enumerable.Range(0/*-range*/, range);
			int b = 1;

			while (result.Count < quantity)
			{
				BigInteger bm = -1 * b * poly.Base;

				var gcd = sieveNumbers.Where(i => CoPrime.IsCoprime(i,b));
				IEnumerable<Tuple<int, int>> pairs = gcd.Select(i => new Tuple<int, int>(i, b));
				IEnumerable<Tuple<int, int>> relations = pairs.Where(tupl => IsSmooth(tupl, poly, threshold));
				result.AddRange(relations);

				b++;
			}

			return result;
		}

		private static bool IsSmooth(Tuple<int, int> pair, Polynomial poly, int threshold)
		{
			BigInteger rationalNorm = BigInteger.Abs(Rational.Norm(pair.Item1, pair.Item2, poly.Base));
			BigInteger algebraicNorm = BigInteger.Abs(Algebraic.Norm(pair.Item1, pair.Item2, poly));

			if (rationalNorm.IsZero || algebraicNorm.IsZero)
			{
				return false;
			}

			IEnumerable<int> rationalNormPrimeFactorization = Factorization.GetPrimeFactoriation((int)rationalNorm);
			IEnumerable<int> algebraicNormPrimeFactorization = Factorization.GetPrimeFactoriation((int)algebraicNorm);

			return (rationalNormPrimeFactorization.Count() > 4 && algebraicNormPrimeFactorization.Count() > 4);
		}
	}
}
