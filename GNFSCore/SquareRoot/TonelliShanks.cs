using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.SquareRoot
{
	using IntegerMath;
	public class TonelliShanks
	{
		/// <summary>
		/// Finds N such that N % P = A
		/// </summary>
		public static BigInteger FindSquareRoot(BigInteger a, BigInteger p)
		{
			if (a >= p)
			{
				throw new Exception("The residue, 'a' cannot be greater than the modulus 'p'!");
			}

			if (Legendre.Symbol(a, p) != 1) //   a^(p-1 / 2) % p == p-1
			{
				throw new ArithmeticException($"Parameter 'a' is not a quadratic residue, mod 'p'");
			}

			if (p % 4 == 3)
			{
				return BigInteger.ModPow(a, (p + 1) / 4, p); // No further processing necessary
			}

			//Initialize 
			BigInteger s = FindS(p);
			BigInteger e = FindE(p);
			BigInteger n = FindGenerator(p);

			BigInteger x = BigInteger.ModPow(a, (s + 1) / 2, p);
			BigInteger b = BigInteger.ModPow(a, s, p);
			BigInteger g = BigInteger.ModPow(n, s, p);
			BigInteger r = e;
			BigInteger m = Order(b, p);
			if (m == 0)
			{
				return x;
			}
			
			while (m > 0)
			{
				x = (x * BigInteger.ModPow(g, TwoExp(r - m - 1), p)) % p;
				b = (b * BigInteger.ModPow(g, TwoExp(r - m), p)) % p;
				g = BigInteger.ModPow(g, TwoExp(r - m), p);
				r = m;
				m = Order(b, p);
			}

			return x;
		}

		private static BigInteger FindGenerator(BigInteger p)
		{
			BigInteger n = 2;
			while (BigInteger.ModPow(n, (p - 1) / 2, p) == 1)
			{
				n++;
			}

			return n;
		}

		private static BigInteger FindS(BigInteger p)
		{
			BigInteger s = p - 1;
			BigInteger e = 0;

			while (s % 2 == 0)
			{
				s /= 2;
				e += 1;
			}

			return s;
		}

		private static BigInteger FindE(BigInteger p)
		{
			BigInteger s = p - 1;
			BigInteger e = 0;

			while (s % 2 == 0)
			{
				s /= 2;
				e += 1;
			}

			return e;
		}


		private static BigInteger Order(BigInteger b, BigInteger p)
		{
			BigInteger m = 1;
			BigInteger e = 0;

			while (BigInteger.ModPow(b, m, p) != 1)
			{
				m *= 2;
				e++;
			}

			return e;
		}

		private static BigInteger TwoExp(BigInteger exp)
		{
			return BigInteger.Pow(2, (int)exp);
		}
	}
}
