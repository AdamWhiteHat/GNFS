using ExtendedArithmetic;
using GNFSCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace GNFSCore.Algorithm.FactorBase
{
	public static class PolynomialSolve
	{
		public static List<FactorPair> FindPolynomialRootsInRange(CancellationToken cancelToken, Polynomial polynomial, IEnumerable<BigInteger> primes, BigInteger rangeFrom, BigInteger rangeTo, int totalFactorPairs)
		{
			List<FactorPair> result = new List<FactorPair>();

			BigInteger r = rangeFrom;
			IEnumerable<BigInteger> modList = primes.AsEnumerable();
			while (!cancelToken.IsCancellationRequested && r < rangeTo && result.Count < totalFactorPairs)
			{
				// Finds p such that ƒ(r) ≡ 0 (mod p)
				List<BigInteger> roots = GetRootsMod(polynomial, r, modList);
				if (roots.Any())
				{
					result.AddRange(roots.Select(p => new FactorPair(p, r)));
				}
				r++;
			}

			return result.OrderBy(tup => tup.P).ToList();
		}

		/// <summary>
		/// Given a list of primes, returns primes p such that ƒ(r) ≡ 0 (mod p)
		/// </summary>
		public static List<BigInteger> GetRootsMod(Polynomial polynomial, BigInteger baseM, IEnumerable<BigInteger> modList)
		{
			BigInteger polyResult = polynomial.Evaluate(baseM);
			IEnumerable<BigInteger> result = modList.Where(mod => (polyResult % mod) == 0);
			return result.ToList();
		}
	}
}
