using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Core.Algorithm.SquareRoot
{
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Algorithm.IntegerMath;
	using IntegerMath;

	public static class FiniteFieldArithmetic
	{
		/// <summary>
		/// Tonelli-Shanks algorithm for finding polynomial modular square roots
		/// </summary>
		/// <returns></returns>
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

			BigInteger halfS = (s + 1) / 2;
			if (r == 1 && q.Mod(4) == 3)
			{
				halfS = (q + 1) / 4;
			}

			BigInteger quadraticNonResidue = Legendre.SymbolSearch(m + 1, q, -1);
			BigInteger theta = quadraticNonResidue;
			BigInteger minusOne = BigInteger.ModPow(theta, (q - 1) / 2, p);

			Polynomial omegaPoly = Polynomial.Field.ExponentiateMod(startPolynomial, halfS, f, p);

			BigInteger lambda = minusOne;
			BigInteger zeta = 0;

			int i = 0;
			do
			{
				i++;

				zeta = BigInteger.ModPow(theta, i * s, p);

				lambda = (lambda * BigInteger.Pow(zeta, (int)Math.Pow(2, r - i))).Mod(p);

				omegaPoly = Polynomial.Field.Multiply(omegaPoly, BigInteger.Pow(zeta, (int)Math.Pow(2, r - i - 1)), p);
			}
			while (!(lambda == 1 || i > r));

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
		/// Finds N such that primes[i] ≡ values[i] (mod N) for all values[i] with 0 &lt; i &lt; a.Length
		/// </summary>
		public static BigInteger ChineseRemainder(List<BigInteger> primes, List<BigInteger> values)
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
			BigInteger finalResult_sqrt = Z - rP;
			return finalResult_sqrt;
		}

		/// <summary>
		/// Reduce a polynomial by a modulus polynomial and modulus integer.
		/// </summary>
		public static Polynomial ModMod(Polynomial toReduce, Polynomial modPoly, BigInteger primeModulus)
		{
			int compare = modPoly.CompareTo(toReduce);
			if (compare > 0)
			{
				return toReduce;
			}
			if (compare == 0)
			{
				return Polynomial.Zero;
			}

			return Remainder(toReduce, modPoly, primeModulus);
		}

		public static Polynomial Remainder(Polynomial left, Polynomial right, BigInteger mod)
		{
			if (left == null)
			{
				throw new ArgumentNullException("left");
			}
			if (right == null)
			{
				throw new ArgumentNullException("right");
			}
			if (right.Degree > left.Degree || right.CompareTo(left) == 1)
			{
				return Polynomial.Zero.Clone();
			}

			int rightDegree = right.Degree;
			int quotientDegree = left.Degree - rightDegree + 1;

			BigInteger leadingCoefficent = right[rightDegree].Mod(mod);
			if (leadingCoefficent != 1) { throw new ArgumentNullException("right", "This method was expecting only monomials (leading coefficient is 1) for the right-hand-side polynomial."); }

			Polynomial rem = left.Clone();
			BigInteger quot = 0;

			for (int i = quotientDegree - 1; i >= 0; i--)
			{
				quot = BigInteger.Remainder(rem[rightDegree + i], mod);//.Mod(mod);

				rem[rightDegree + i] = 0;

				for (int j = rightDegree + i - 1; j >= i; j--)
				{
					rem[j] = BigInteger.Subtract(
													rem[j],
													BigInteger.Multiply(quot, right[j - i]).Mod(mod)
												).Mod(mod);
				}
			}

			return new Polynomial(rem.Terms);
		}
	}
}
