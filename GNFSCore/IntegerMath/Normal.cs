using System;
using System.Linq;
using System.Numerics;

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
		public static BigInteger Rational(int a, int b, BigInteger polynomialBase)
		{
			return BigInteger.Add(a, BigInteger.Multiply(b, polynomialBase));
		}

		/// <summary>
		/// a - bm
		/// </summary>
		public static BigInteger RationalSubtract(int a, int b, BigInteger polynomialBase)
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
		public static BigInteger Algebraic(int a, int b, IPolynomial poly)
		{
			int bneg = -b;
			double ab = (double)a / (double)bneg;

			//BigInteger remainder = new BigInteger();
			//BigInteger quotient = BigInteger.DivRem(a, bneg, out remainder);
			//double remainder = (double)remainder / (double)bneg;

			double left = poly.Evaluate(ab);
			double right = Math.Pow(bneg, poly.Degree);

			double deci = left % 1;
			double deciProduct = deci * right;
			deciProduct = Math.Round(deciProduct, MidpointRounding.ToEven);

			BigInteger result = BigInteger.Multiply((BigInteger)left, (BigInteger)right);
			result += (BigInteger)deciProduct;

			return result;
		}

		/// <summary>
		/// ƒ(b) ≡ 0 (mod a)
		/// 
		/// Calclulated as:
		/// ƒ(-a/b) * -b^deg
		/// </summary>
		/// <param name="a">Divisor in the equation ƒ(b) ≡ 0 (mod a)</param>
		/// <param name="b">A root of f(x)</param>
		/// <param name="poly">Base m of f(m) = N</param>
		/// <returns></returns>
		public static BigInteger Algebraic(BigInteger a, BigInteger b, IPolynomial poly)
		{
			BigInteger bneg = BigInteger.Negate(b);

			BigInteger remainder = new BigInteger();
			BigInteger ab = BigInteger.DivRem(a, bneg, out remainder);

			BigInteger left = poly.Evaluate(ab);
			BigInteger right = BigInteger.Pow(bneg, poly.Degree);

			BigInteger result = BigInteger.Multiply((BigInteger)left, (BigInteger)right);

			return result;
		}
	}
}
