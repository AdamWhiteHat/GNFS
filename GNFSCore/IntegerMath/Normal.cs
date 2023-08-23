using System;
using System.Linq;
using System.Numerics;
using ExtendedArithmetic;
using ExtendedNumerics;

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
			BigRational aD = (BigRational)a;
			BigRational bD = (BigRational)b;
			BigRational ab = BigRational.Negate(aD) / bD;

			BigRational left = PolynomialEvaluate_BigRational(poly, ab);
			BigInteger right = BigInteger.Pow(BigInteger.Negate(b), poly.Degree);

			BigRational product = right * left;

			Fraction fractionalPart = product.FractionalPart;
			if (fractionalPart != Fraction.Zero)
			{
				GNFS.LogFunction($"{nameof(Algebraic)} failed to result in an integer. This shouldn't happen.");
			}

			BigInteger result = product.WholePart;
			return result;
		}

		private static BigRational PolynomialEvaluate_BigRational(Polynomial polynomial, BigRational indeterminateValue)
		{
			int num = polynomial.Degree;

			BigRational result = (BigRational)polynomial[num];
			while (--num >= 0)
			{
				result *= indeterminateValue;
				result += (BigRational)polynomial[num];
			}

			return result;
		}
	}
}
