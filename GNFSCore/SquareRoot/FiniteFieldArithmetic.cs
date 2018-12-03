using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.SquareRoot
{
	using GNFSCore.Polynomial;
	using GNFSCore.IntegerMath;

	public static class FiniteFieldArithmetic
	{
		public static IPoly SquareRoot(IPoly startPolynomial, IPoly f, BigInteger p, int degree, BigInteger m)
		{
			BigInteger q = BigInteger.Pow(p, degree);
			BigInteger s = q - 1;

			int r = 0;
			while (s.Mod(2) == 0)
			{
				s /= 2;
				r++;
			}

			BigInteger order = BigInteger.Pow(2, r);

			BigInteger k = 0;
			BigInteger halfS = ((s + 1) / 2);
			if (r == 1 && q.Mod(4) == 3)
			{
				k = (q - 3) / 4;
				halfS = ((q + 1) / 4);
			}
			else
			{
				k = (q - 1) / 4;
			}

			BigInteger quadraticNonResidue = Legendre.SymbolSearch(m + 1, q, -1);
			BigInteger theta = quadraticNonResidue;
			BigInteger minusOne = BigInteger.ModPow(theta, ((q - 1) / 2), p);

			IPoly omegaPoly = SparsePolynomial.ExponentiateMod(startPolynomial, halfS, f, p);

			BigInteger lambda = minusOne;
			BigInteger zeta = 0;
			BigInteger zetaSquared = 0;
			BigInteger zetaInverse = 0;


			string j = "";
			int i = 0;
			do
			{
				i++;
				j = GetSub(i);

				zeta = BigInteger.ModPow(theta, (i * s), p);

				zetaSquared = zeta.Square().Mod(p);//BigInteger.ModPow(theta, (2 * i * s), p);  // BigInteger.ModPow(quadraticNonResidue, (i * s * 2), p);

				zetaInverse = (p - zetaSquared);

				lambda = (lambda * BigInteger.Pow(zeta, (int)Math.Pow(2, (r - i)))).Mod(p);

				omegaPoly = SparsePolynomial.MultiplyMod(omegaPoly, BigInteger.Pow(zeta, (int)Math.Pow(2, ((r - i) - 1))), p);

				IPoly nu = SparsePolynomial.MultiplyMod(omegaPoly, zetaInverse, p);
			}
			while (!((lambda == 1) || (i > (r))));

			return omegaPoly;
		}

		private static string subscriptNumbers = "₀₁₂₃₄₅₆₇₈₉";
		private static string GetSub(int number)
		{
			return subscriptNumbers[number].ToString();
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
				BigInteger Aj = ModularMultiplicativeInverse(Pj, pi); // pi-(Pj.Mod(pi));
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
