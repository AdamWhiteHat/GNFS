using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;

namespace GNFSCore.Polynomial
{
	using IntegerMath;

	public static class PolynomialArithmetic
	{
		public static bool IsIrreducible(IPolynomial poly, BigInteger p)
		{
			IPolynomial clone = poly.Clone();
			IPolynomial tmp = null;
			IPolynomial f = ReduceMod(clone, p);
			f = MakeMonic(f, p);

			for (int i = 2; i < f.Degree; i++)
			{
				if (f.Degree % i == 1)
				{
					continue;
				}

				tmp = BigIntegerArithmetic.XPowPD(p, (f.Degree / i), f);

				if (tmp.Degree == 0)
				{
					tmp = new AlgebraicPolynomial(new BigInteger[] { tmp.Terms[0], (p - 1) });
				}
				else
				{
					tmp.Terms[1] = BigIntegerArithmetic.ModSub(tmp.Terms[1], 1, p);
					tmp = FixDegree(tmp);
				}

				tmp = GCD(tmp, f, p);
				if (tmp.Degree > 0 || tmp.Terms[0] != 1)
				{
					return false;
				}
			}

			tmp = BigIntegerArithmetic.XPowPD(p, f.Degree, f);
			if (tmp.Degree == 1 && tmp.Terms[0] == 0 && tmp.Terms[1] == 1)
			{
				return true;
			}

			return false;
		}

		public static IPolynomial GCD(IPolynomial left, IPolynomial right, BigInteger mod)
		{
			IPolynomial a = left.Clone();
			IPolynomial b = right.Clone();

			IPolynomial g = null;
			IPolynomial h = null;

			/* make sure the first GCD iteration actually
			   performs useful work */

			if (a.Degree > b.Degree)
			{
				g = a;
				h = b;
			}
			else
			{
				h = a;
				g = b;
			}

			while (h.Degree > 0 || h.Terms[h.Degree] != 0)
			{
				IPolynomial r = DivMod(g, h, mod);
				if (r == null)
				{
					break;
				}
				g = h;
				h = r;
			}

			if (g.Degree == 0)
			{
				g.Terms[0] = 1;
			}

			return g;
		}

		public static IPolynomial FixDegree(IPolynomial poly)
		{
			IPolynomial clone = poly.Clone();
			int i = clone.Degree;

			while ((i > 0) && (clone.Terms[i] == 0))
			{
				i--;
			}

			return new AlgebraicPolynomial(clone.Terms.Take(i + 1).ToArray());
		}

		public static IPolynomial MakeMonic(IPolynomial poly, BigInteger mod)
		{
			IPolynomial clone = poly.Clone();
			int i;
			int d = clone.Degree;
			BigInteger leadingCoefficient = clone.Terms[d].Clone();

			BigInteger[] terms = new BigInteger[d + 1];

			if (leadingCoefficient != 1)
			{
				leadingCoefficient = BigIntegerArithmetic.ModInv(leadingCoefficient, mod);
				terms[d] = 1;
				for (i = 0; i < d; i++)
				{
					terms[i] = BigIntegerArithmetic.ModMultiply(leadingCoefficient, clone.Terms[i].Clone(), mod);
				}
			}
			else
			{
				return clone;
			}

			IPolynomial result = new AlgebraicPolynomial(terms);
			return result;
		}

		public static IPolynomial Multiply(IPolynomial multiplicand, IPolynomial multiplier)
		{
			IPolynomial a = multiplicand.Clone();
			IPolynomial b = multiplier.Clone();

			BigInteger[] terms = new BigInteger[a.Degree + b.Degree + 1];
			for (int i1 = 0; i1 <= a.Degree; i1++)
			{
				for (int i2 = 0; i2 <= b.Degree; i2++)
				{
					terms[i1 + i2] += BigInteger.Multiply(a.Terms[i1], b.Terms[i2]);
				}
			}
			return new AlgebraicPolynomial(terms);
		}

		public static IPolynomial ReduceMod(IPolynomial poly, BigInteger mod)
		{
			IPolynomial clone = poly.Clone();
			BigInteger[] terms = new BigInteger[clone.Degree + 1];

			for (int i = 0; i <= clone.Degree; i++)
			{
				terms[i] = clone.Terms[i] % mod; // BigInteger.Divide(...)
			}

			IPolynomial result = new AlgebraicPolynomial(terms.ToArray());
			return FixDegree(result);
		}

		public static IPolynomial MultiplyMod(IPolynomial multiplicand, IPolynomial multiplier, IPolynomial modulus)
		{
			IPolynomial a = multiplicand.Clone();
			IPolynomial b = multiplier.Clone();
			IPolynomial mod = modulus.Clone();

			int i = 0;
			BigInteger[] terms = new BigInteger[Math.Max(a.Degree, b.Degree) + 1];
			if (a.Degree < b.Degree)
			{
				for (i = 0; i <= a.Degree; i++)
				{
					terms[i] = BigIntegerArithmetic.ModMultiply(a.Terms[i], b.Terms[i], mod.Terms[1]);
				}
				for (; i <= b.Degree; i++)
				{
					terms[i] = b.Terms[i] % mod.Terms[1];
				}
			}
			else
			{
				for (i = 0; i <= b.Degree; i++)
				{
					BigInteger answ = BigIntegerArithmetic.ModMultiply(a.Terms[i], b.Terms[i], mod.Terms[1]);
					terms[i] = answ;
				}
				for (; i <= a.Degree; i++)
				{
					terms[i] = a.Terms[i] % mod.Terms[1];
				}
			}

			return new AlgebraicPolynomial(terms.ToArray());
		}

		public static IPolynomial MultiplyMod(IPolynomial multiplicand, IPolynomial multiplier, IPolynomial modulus, BigInteger p)
		{
			IPolynomial a = multiplicand.Clone();
			IPolynomial b = multiplier.Clone();
			IPolynomial mod = modulus.Clone();

			BigInteger[] terms = new BigInteger[a.Degree + b.Degree + 1];
			for (int i = 0; i <= a.Degree; i++)
			{
				terms[i] = BigIntegerArithmetic.ModMultiply(a.Terms[i], b.Terms[0], p);
			}

			for (int i = 1; i <= b.Degree; i++)
			{
				int j = 1;
				for (j = 0; j < a.Degree; j++)
				{
					BigInteger c = BigIntegerArithmetic.ModMultiply(a.Terms[j], b.Terms[i], p);
					terms[i + j] = BigIntegerArithmetic.ModAdd(terms[i + j], c, p);
				}
				terms[i + j] = BigIntegerArithmetic.ModMultiply(a.Terms[j], b.Terms[i], p);
			}

			IPolynomial product = new AlgebraicPolynomial(terms);
			product = FixDegree(product);
			IPolynomial result = DivMod(product, mod, p);
			return result;
		}
		
		public static IPolynomial AddMod(IPolynomial multiplicand, IPolynomial multiplier, BigInteger mod)
		{
			IPolynomial a = multiplicand.Clone();
			IPolynomial b = multiplier.Clone();

			int i = 0;
			BigInteger[] terms = new BigInteger[Math.Max(a.Degree, b.Degree) + 1];
			if (a.Degree < b.Degree)
			{
				for (i = 0; i <= a.Degree; i++)
				{
					terms[i] = BigIntegerArithmetic.ModAdd(a.Terms[i], b.Terms[i], mod);
				}
				for (; i <= b.Degree; i++)
				{
					terms[i] = b.Terms[i];
				}
			}
			else
			{
				for (i = 0; i <= b.Degree; i++)
				{
					terms[i] = BigIntegerArithmetic.ModAdd(a.Terms[i], b.Terms[i], mod);
				}
				for (; i <= a.Degree; i++)
				{
					terms[i] = a.Terms[i];
				}
			}

			return FixDegree(new AlgebraicPolynomial(terms));
		}

		private static IPolynomial DivMod(IPolynomial numerator, IPolynomial denominator, BigInteger mod)
		{
			// Divide the polynomial numerator by the polynomial denominator
			// with all polynomial coefficients being reduced modulo mod

			int i;
			BigInteger msw;
			IPolynomial dividend = null;
			IPolynomial divisor = null;

			dividend = numerator.Clone();
			divisor = MakeMonic(denominator.Clone(), mod);

			if (divisor.Degree == 0)
			{
				return null;
			}

			while (dividend.Degree >= divisor.Degree)
			{
				msw = dividend.Terms[dividend.Degree];

				dividend.Terms[dividend.Degree] = 0;
				for (i = divisor.Degree - 1; i >= 0; i--)
				{
					BigInteger c = BigIntegerArithmetic.ModMultiply(msw, divisor.Terms[i], mod);
					int j = dividend.Degree - (divisor.Degree - i);
					dividend.Terms[j] = BigIntegerArithmetic.ModSub(dividend.Terms[j], c, mod);
				}
				dividend = FixDegree(dividend);
			}
			return dividend;
		}

	}
}
