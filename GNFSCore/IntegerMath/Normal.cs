using System;
using System.Linq;
using System.Numerics;

namespace GNFSCore.Factors
{
	using Polynomial;
	using Polynomial.Internal;

	public static class Normal
	{
		/// <summary>
		///  a + bm
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="polynomialBase">Base m of f(m) = N or m in the polynomial 2m^2 + 34m +1</param>
		/// <returns></returns>
		public static BigInteger Rational(int a, int b, BigInteger polynomialBase)
		{
			return BigInteger.Add(a, BigInteger.Multiply(b, polynomialBase));
		}


		/// <summary>
		/// ƒ(b) ≡ 0 (mod a)
		/// 
		/// Calclulated as:
		/// ƒ(-a/b) * -b^deg
		/// </summary>
		/// <param name="a">Divisor in the equation ƒ(b) ≡ 0 (mod a)</param>
		/// <param name="b">A root of f(x)</param>
		/// <param name="poly">Base m of f(m) = N or m in the polynomial 2m^2 + 34m +1</param>
		/// <returns></returns>
		public static BigInteger Algebraic(int a, int b, IPolynomial poly)
		{		

			int bneg = -b;
			double ab = (double)a / (double)bneg;

			//BigInteger remainder = new BigInteger();
			//BigInteger quotient = BigInteger.DivRem(a, bneg, out remainder);
			//double remainder = (double)remainder / (double)bneg;

			double right = CommonPolynomial.Evaluate(poly, ab);
			double left = Math.Pow(bneg, poly.Degree);

			double deci = right % 1;
			double deciProduct = deci * left;
			deciProduct = Math.Round(deciProduct, MidpointRounding.ToEven);

			BigInteger result = BigInteger.Multiply((BigInteger)right, (BigInteger)left);
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
		/// <param name="poly">Base m of f(m) = N or m in the polynomial 2m^2 + 34m +1</param>
		/// <returns></returns>
		public static BigInteger Algebraic(BigInteger a, BigInteger b, IPolynomial poly)
		{
			BigInteger bneg = BigInteger.Negate(b);

			BigInteger remainder = new BigInteger();
			BigInteger ab = BigInteger.DivRem(a, bneg, out remainder);

			BigInteger right = CommonPolynomial.Evaluate(poly, ab);
			BigInteger left = BigInteger.Pow(bneg, poly.Degree);

			BigInteger result = BigInteger.Multiply((BigInteger)right, (BigInteger)left);

			return result;
		}


		//public static BigRational AlgebraicRational(int a, int b, IPolynomial polynomial)
		//{
		//	ƒ(−a/b) ∙ −bᵈ 
		// (−b)ᵈ ∙ ƒ(−a/b)
		//
		//	Fraction ratA = new Fraction(a);
		//	Fraction negB = new Fraction(-b);
		//	BigRational aOverB = BigRational.Divide(ratA, negB);
		//
		//	BigRational left = polynomial.Evaluate(aOverB);
		//	BigRational right = BigRational.Pow(new BigRational(negB), polynomial.Degree);
		//
		//	BigRational result = BigRational.Multiply(left, right);
		//	return result;
		//}
	}


}
