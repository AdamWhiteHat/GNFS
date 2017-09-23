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
			// array of (p, m % p) up to bound
			// quantity = phi(bound)
			public static FactorCollection BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<FactorPair> result = gnfs.PrimeBase.RationalPrimeBase.Select(p => new FactorPair(p, (gnfs.CurrentPolynomial.Base % p))).Distinct();
				return new FactorCollection(gnfs, result.ToList());
			}

			// array of (p, r) where ƒ(r) % p == 0
			// quantity = 2-3 times RFB.quantity
			public static FactorCollection BuildAlgebraicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(gnfs, FindPolynomialRootsInRange(gnfs.CancelToken, gnfs.CurrentPolynomial, gnfs.PrimeBase.AlgebraicPrimeBase, 0, gnfs.PrimeBase.AlgebraicFactorBase, 2000));
			}

			// array of (p, r) where ƒ(r) % p == 0
			// quantity =< 100
			// magnitude p > AFB.Last().p
			public static FactorCollection BuildQuadradicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(gnfs, FindPolynomialRootsInRange(gnfs.CancelToken, gnfs.CurrentPolynomial, gnfs.PrimeBase.QuadraticPrimeBase, 2, gnfs.PrimeBase.QuadraticFactorBaseMin, 100));
			}

			private delegate BigInteger BigIntegerEvaluateDelegate(BigInteger x);
			private delegate BigInteger DoubleEvaluateDelegate(BigInteger x);

			public static FactorCollection BuildGFactorBase(GNFS gnfs)
			{
				BigInteger Cd = (gnfs.CurrentPolynomial.Terms.Last());
				BigInteger Cdd = BigInteger.Pow(Cd, (gnfs.CurrentPolynomial.Degree - 1));
				DoubleEvaluateDelegate evalDelegate = gnfs.CurrentPolynomial.Evaluate;

				IEnumerable<FactorPair> results = gnfs.PrimeBase.RationalPrimeBase.Select(p => new FactorPair(p, BigInteger.Multiply(evalDelegate(BigInteger.Divide(p, Cd)), Cdd))).Distinct();
				return new FactorCollection(gnfs, results.ToList());
			}
		}

		public static List<FactorPair> FindPolynomialRootsInRange(CancellationToken cancelToken, IPolynomial polynomial, IEnumerable<BigInteger> primes, BigInteger rangeFrom, BigInteger rangeTo, int totalFactorPairs)
		{
			List<FactorPair> result = new List<FactorPair>();

			BigInteger r = rangeFrom;
			while (r < rangeTo && result.Count < totalFactorPairs)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				IEnumerable<BigInteger> modList = primes.Where(p => p > r);
				List<BigInteger> roots = polynomial.GetRootsMod(r, modList);
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

		public string ToString(int take)
		{
			return string.Join("\t", this.Take(take).Select(factr => factr.ToString()));
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
