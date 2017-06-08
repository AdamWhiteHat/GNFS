using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using ExtendedNumerics;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;
using System.IO;
using System.Threading;

namespace GNFSCore.FactorBase
{
	public class FactorCollection : List<FactorPair>
	{
		private GNFS _gnfs;

		public FactorCollection()
			: base()
		{
		}

		public FactorCollection(IEnumerable<FactorPair> collection)
			: base(collection)
		{
		}

		private FactorCollection(GNFS gnfs, List<FactorPair> collection)
			: base(collection)
		{
			_gnfs = gnfs;
		}

		public static class Factory
		{
			// array of (p, p mod m) up to bound
			// quantity = phi(bound)
			public static FactorCollection BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<FactorPair> result = gnfs.RationalPrimeBase.Select(p => new FactorPair(p, (int)(gnfs.CurrentPolynomial.Base % p))).Distinct();
				return new FactorCollection(gnfs, result.ToList());
			}

			// array of (p, r) where f(r) = 0 mod p
			// quantity = 2-3 times RFB.quantity
			public static FactorCollection GetAlgebraicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(gnfs, GetPolynomialRootsInRange(gnfs.CancelToken, gnfs.CurrentPolynomial, gnfs.AlgebraicPrimeBase, 0, gnfs.AlgebraicFactorBase, 2000));
			}

			// array of (p, r) where f(r) = 0 mod p		
			// quantity =< 100
			// magnitude p > AFB.Last().p
			public static FactorCollection GetQuadradicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(gnfs, GetPolynomialRootsInRange(gnfs.CancelToken, gnfs.CurrentPolynomial, gnfs.QuadraticPrimeBase, 2, gnfs.QuadraticFactorBaseMin, 2000));
			}
		}

		public static List<FactorPair> GetPolynomialRootsInRange(CancellationToken cancelToken, AlgebraicPolynomial polynomial, IEnumerable<int> primes, int rangeFrom, int rangeTo, int totalFactorPairs)
		{
			List<FactorPair> result = new List<FactorPair>();

			int r = rangeFrom;
			while (r < rangeTo && result.Count < totalFactorPairs)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				IEnumerable<int> modList = primes.Where(p => p > r);
				List<int> roots = polynomial.GetRootsMod(r, modList);
				if (roots.Any())
				{
					result.AddRange(roots.Select(p => new FactorPair(p, r)));
				}
				r++;
			}

			return result.OrderBy(tup => tup.P).ToList();
		}

		public override string ToString()
		{
			return string.Join("\t", this.Select(factr => factr.ToString()));
		}

		public static void Serialize(string filePath, FactorCollection factorCollection)
		{
			File.WriteAllLines(filePath, factorCollection.Select(fp => fp.Serialize()));
		}

		public static FactorCollection Deserialize(string filePath)
		{
			string[] lines = File.ReadAllLines(filePath);
			IEnumerable<FactorPair> factorPairs = lines.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => FactorPair.Deserialize(s));
			return new FactorCollection(factorPairs);
		}
	}
}
