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
        public static Polynomial SquareRoot(Polynomial startPolynomial, Polynomial f, BigInteger p, int degree, BigInteger m)
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

            Polynomial omegaPoly = Polynomial.Field.ExponentiateMod(startPolynomial, halfS, f, p);

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

        /// <summary>
        /// Finds X such that a*X = 1 (mod p)
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="p">The modulus</param>
        /// <returns></returns>
        public static BigInteger ModularMultiplicativeInverse(BigInteger a, BigInteger p)
        {
            if (p == 1)
            {
                return 0;
            }

            BigInteger divisor;
            BigInteger dividend = a;
            BigInteger diff = 0;
            BigInteger result = 1;
            BigInteger quotient = 0;
            BigInteger lastDivisor = 0;
            BigInteger remainder = p;

            while (dividend > 1)
            {
                divisor = remainder;
                quotient = BigInteger.DivRem(dividend, divisor, out remainder); // Divide             
                dividend = divisor;
                lastDivisor = diff; // The thing to divide will be the last divisor

                // Update diff and result 
                diff = result - quotient * diff;
                result = lastDivisor;
            }

            if (result < 0)
            {
                result += p; // Make result positive 
            }
            return result;
        }

        /// <summary>
        /// Finds k such that m[i] ≡ c[i] (mod N) for all c[i] with 0 &lt; i &lt; a.Length
        /// </summary>
        public static BigInteger ChineseRemainder(BigInteger n, List<BigInteger> values, List<BigInteger> primes)
        {
            BigInteger primeProduct = primes.Product();

            int indx = 0;
            BigInteger Z = 0;
            foreach (BigInteger pi in primes)
            {
                BigInteger Pj = primeProduct / pi;
                BigInteger Aj = ModularMultiplicativeInverse(Pj, pi);
                BigInteger AXPj = values[indx] * Aj * Pj;

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
