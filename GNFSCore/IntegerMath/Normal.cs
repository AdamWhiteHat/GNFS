using System;
using System.Linq;
using System.Numerics;
using ExtendedNumerics;

namespace GNFSCore.FactorBase
{
	using GNFSCore.Polynomial;
	using GNFSCore.Polynomial.Internal;

	public static class Normal
	{
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

		public static BigInteger Algebraic(int a, int b, IPolynomial poly)
		{
			// ƒ(-a/b) * -b^deg

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

		public static BigInteger Rational(int a, int b, BigInteger polynomialBase)
		{
			// a + bm
			return BigInteger.Add(a, BigInteger.Multiply(b, polynomialBase));
		}
	}
}
