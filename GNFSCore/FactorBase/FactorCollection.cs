using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;
using System.IO;
using System.Threading;
using GNFSCore.Polynomial.Internal;

namespace GNFSCore.Factors
{
	public class FactorCollection : List<FactorPair>
	{
		public FactorCollection()
			: base()
		{
		}

		public FactorCollection(IEnumerable<FactorPair> collection)
			: base(collection)
		{
		}

		public List<int> Primes
		{
			get
			{
				return this.Select(fp => fp.P).Distinct().ToList();
			}
		}

		public override string ToString()
		{
			return string.Join("\t", this.Select(factr => factr.ToString()));
		}

		public string ToString(int take)
		{
			return string.Join("\t", this.Take(take).Select(factr => factr.ToString()));
		}

		public static class Factory
		{
			// array of (p, m % p) up to bound
			// quantity = phi(bound)
			public static FactorCollection BuildRationalFactorBase(GNFS gnfs)
			{
				IEnumerable<FactorPair> result = gnfs.PrimeFactorBase.RationalFactorBase.Select(p => new FactorPair(p, (gnfs.PolynomialBase % p))).Distinct();
				return new FactorCollection(result.ToList());
			}

			// array of (p, r) where ƒ(r) % p == 0
			// quantity = 2-3 times RFB.quantity
			public static FactorCollection BuildAlgebraicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(FindPolynomialRootsInRange(gnfs.CancelToken, gnfs.CurrentPolynomial, gnfs.PrimeFactorBase.AlgebraicFactorBase, 0, gnfs.PrimeFactorBase.MaxAlgebraicFactorBase, 2000));
			}

			// array of (p, r) where ƒ(r) % p == 0
			// quantity =< 100
			// magnitude p > AFB.Last().p
			public static FactorCollection BuildQuadradicFactorBase(GNFS gnfs)
			{
				return new FactorCollection(FindPolynomialRootsInRange(gnfs.CancelToken, gnfs.CurrentPolynomial, gnfs.PrimeFactorBase.QuadraticFactorBase, 2, gnfs.PrimeFactorBase.MinQuadraticFactorBase, 100));
			}

			private delegate BigInteger BigIntegerEvaluateDelegate(BigInteger x);
			private delegate BigInteger DoubleEvaluateDelegate(BigInteger x);

			public static FactorCollection BuildGFactorBase(GNFS gnfs)
			{
				BigInteger Cd = (gnfs.CurrentPolynomial.Terms.Last());
				BigInteger Cdd = BigInteger.Pow(Cd, (gnfs.CurrentPolynomial.Degree - 1));
				DoubleEvaluateDelegate evalDelegate = gnfs.CurrentPolynomial.Evaluate;

				IEnumerable<FactorPair> results = gnfs.PrimeFactorBase.RationalFactorBase.Select(p => new FactorPair(p, BigInteger.Multiply(evalDelegate(BigInteger.Divide(p, Cd)), Cdd))).Distinct();
				return new FactorCollection(results.ToList());
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
				List<BigInteger> roots = CommonPolynomial.GetRootsMod(polynomial, r, modList);
				if (roots.Any())
				{
					result.AddRange(roots.Select(p => new FactorPair(p, r)));
				}
				r++;
			}

			return result.OrderBy(tup => tup.P).ToList();
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
