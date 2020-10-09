using System;
using System.Linq;
using System.Numerics;
using ExtendedArithmetic;

namespace GNFSCore.Factors
{
	using Interfaces;

	public static class Normal
	{
		/// <summary>
		///  a + bm
		/// </summary>
		/// <param name="polynomialBase">Base m of f(m) = N</param>
		/// <returns></returns>
		public static BigInteger Rational(BigInteger a, BigInteger b, BigInteger polynomialBase)
		{
			return BigInteger.Add(a, BigInteger.Multiply(b, polynomialBase));
		}

		/// <summary>
		/// a - bm
		/// </summary>
		public static BigInteger RationalSubtract(BigInteger a, BigInteger b, BigInteger polynomialBase)
		{
			return BigInteger.Subtract(a, BigInteger.Multiply(b, polynomialBase));
		}

		/// <summary>
		/// ƒ(b) ≡ 0 (mod a)
		/// 
		/// Calculated as:
		/// ƒ(-a/b) * -b^deg
		/// </summary>
		/// <param name="a">Divisor in the equation ƒ(b) ≡ 0 (mod a)</param>
		/// <param name="b">A root of f(x)</param>
		/// <param name="poly">Base m of f(m) = N</param>
		/// <returns></returns>
		public static BigInteger Algebraic(BigInteger a, BigInteger b, Polynomial poly)
		{
			decimal aD = (decimal)a;
			decimal bD = (decimal)b;
			decimal ab = (-aD) / bD;

			decimal left = poly.Evaluate(ab);
			BigInteger right = BigInteger.Pow(BigInteger.Negate(b), poly.Degree);

			decimal product = (decimal)right;
			product *= left;

			BigInteger result = (BigInteger)Math.Round(product);
			return result;
		}

		/*
		private static decimal Evaluate(Term[] terms, decimal indeterminateValue)
		{
			decimal result = 0;
			decimal placeValue = 0;

			int d = terms.Count() - 1;
			while (d >= 0)
			{
				placeValue = Power(indeterminateValue, terms[d].Exponent);
				result += decimal.Multiply((decimal)terms[d].CoEfficient, placeValue);
				d--;
			}
			return result;
		}	

		private static decimal Power(decimal value, int exponent)
		{
			if (exponent < 0) throw new ArgumentOutOfRangeException(nameof(exponent), "Negative exponents not supported!");
			if (exponent == 0m) { return 1; }
			if (exponent == 1m) { return value; }

			int exp = exponent;
			decimal result = 1m;
			decimal multiplier = value;
			while (exp > 0)
			{
				if (exp % 2 == 1) // If exp is odd
				{
					result *= multiplier;
					exp -= 1;
					if (exp == 0) { break; }
				}
				multiplier *= multiplier;
				exp /= 2;
			}
			return result;
		}
		*/

		/*
		/// <summary>
		/// ƒ(b) ≡ 0 (mod a)
		/// 
		/// Calculated as:
		/// ƒ(-a/b) * -b^deg
		/// </summary>
		/// <param name="a">Divisor in the equation ƒ(b) ≡ 0 (mod a)</param>
		/// <param name="b">A root of f(x)</param>
		/// <param name="poly">Base m of f(m) = N</param>
		/// <returns></returns>
		public static BigInteger Algebraic(BigInteger a, BigInteger b, Polynomial poly)
		{
			BigInteger bneg = BigInteger.Negate(b);

			BigInteger remainder = new BigInteger();
			BigInteger ab = BigInteger.DivRem(a, bneg, out remainder);

			BigInteger left = poly.Evaluate(ab);
			BigInteger right = BigInteger.Pow(bneg, poly.Degree);

			BigInteger result = BigInteger.Multiply(left, right);

			return result;
		}
		*/
	}
}
