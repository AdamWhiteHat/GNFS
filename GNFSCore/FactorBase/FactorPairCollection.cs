using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Factors
{
    using Interfaces;
    public class FactorPairCollection : List<FactorPair>
    {
        public FactorPairCollection()
            : base()
        {
        }

        public FactorPairCollection(IEnumerable<FactorPair> collection)
            : base(collection)
        {
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
            public static FactorPairCollection BuildRationalFactorPairCollection(GNFS gnfs)
            {
                IEnumerable<FactorPair> result = gnfs.PrimeFactorBase.RationalFactorBase.Select(p => new FactorPair(p, (gnfs.PolynomialBase % p))).Distinct();
                return new FactorPairCollection(result.ToList());
            }

            // array of (p, r) where ƒ(r) % p == 0
            // quantity = 2-3 times RFB.quantity
            public static FactorPairCollection BuildAlgebraicFactorPairCollection(CancellationToken cancelToken, GNFS gnfs)
            {
                return new FactorPairCollection(FindPolynomialRootsInRange(cancelToken, gnfs.CurrentPolynomial, gnfs.PrimeFactorBase.AlgebraicFactorBase, 0, gnfs.PrimeFactorBase.AlgebraicFactorBaseMax, 2000));
            }

            // array of (p, r) where ƒ(r) % p == 0
            // quantity =< 100
            // magnitude p > AFB.Last().p
            public static FactorPairCollection BuildQuadraticFactorPairCollection(CancellationToken cancelToken, GNFS gnfs)
            {
                return new FactorPairCollection(FindPolynomialRootsInRange(cancelToken, gnfs.CurrentPolynomial, gnfs.PrimeFactorBase.QuadraticFactorBase, 2, gnfs.PrimeFactorBase.QuadraticFactorBaseMax, gnfs.PrimeFactorBase.QuadraticBaseCount));
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
                List<BigInteger> roots = GetRootsMod(polynomial, r, modList);
                if (roots.Any())
                {
                    result.AddRange(roots.Select(p => new FactorPair(p, r)));
                }
                r++;
            }

            return result.OrderBy(tup => tup.P).ToList();
        }

        public static List<BigInteger> GetRootsMod(IPolynomial polynomial, BigInteger baseM, IEnumerable<BigInteger> modList)
        {
            BigInteger polyResult = polynomial.Evaluate(baseM);
            IEnumerable<BigInteger> result = modList.Where(mod => (polyResult % mod) == 0);
            return result.ToList();
        }
    }
}
