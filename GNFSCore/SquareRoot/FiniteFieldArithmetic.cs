using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.SquareRoot
{
    using IntegerMath;

    public static class FiniteFieldArithmetic
    {
        public static IPolynomial SquareRoot(IPolynomial startPolynomial, IPolynomial f, BigInteger p, int degree, BigInteger m)
        {
            BigInteger q = BigInteger.Pow(p, degree);
            BigInteger s = q - 1;

            int r = 0;
            while (s.Mod(2) == 0)
            {
                s /= 2;
                r++;
            }

            BigInteger halfS = ((s + 1) / 2);
            if (r == 1 && q.Mod(4) == 3)
            {
                halfS = ((q + 1) / 4);
            }

            BigInteger quadraticNonResidue = Legendre.SymbolSearch(m + 1, q, -1);
            BigInteger theta = quadraticNonResidue;
            BigInteger minusOne = BigInteger.ModPow(theta, ((q - 1) / 2), p);

            IPolynomial omegaPoly = Polynomial.Field.ExponentiateMod(startPolynomial, halfS, f, p);

            BigInteger lambda = minusOne;
            BigInteger zeta = 0;

            int i = 0;
            do
            {
                i++;

                zeta = BigInteger.ModPow(theta, (i * s), p);

                lambda = (lambda * BigInteger.Pow(zeta, (int)Math.Pow(2, (r - i)))).Mod(p);

                omegaPoly = Polynomial.Field.Multiply(omegaPoly, BigInteger.Pow(zeta, (int)Math.Pow(2, ((r - i) - 1))), p);
            }
            while (!((lambda == 1) || (i > (r))));

            return omegaPoly;
        }

        // Finds X such that a*X = 1 (mod p)
        public static BigInteger ModularMultiplicativeInverse(BigInteger a, BigInteger mod)
        {
            BigInteger b = a.Mod(mod);
            for (int x = 1; x < mod; x++)
            {
                if ((b * x).Mod(mod) == 1)
                {
                    return x;
                }
            }
            return 1;
        }

        public static BigInteger ChineseRemainder(BigInteger n, List<BigInteger> values, List<BigInteger> primes)
        {
            BigInteger primeProduct = primes.Product();

            int indx = 0;
            BigInteger Z = 0;
            foreach (BigInteger pi in primes)
            {
                BigInteger Pj = primeProduct / pi;
                BigInteger Aj = ModularMultiplicativeInverse(Pj, pi);
                BigInteger Xj = values[indx];
                BigInteger AXPj = (Aj * Xj * Pj);

                Z += AXPj;
                indx++;
            }

            BigInteger r = Z / primeProduct;
            BigInteger rP = r * primeProduct;
            BigInteger finalResult_sqrt = ((Z - rP).Mod(n));

            return finalResult_sqrt;
        }
    }
}
