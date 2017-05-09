using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using ExtendedNumerics;
using GNFSCore.Polynomial;

namespace GNFSCore.FactorBase
{
	public static class Normal
	{
		public static BigRational AlgebraicRational(int a, int b, AlgebraicPolynomial polynomial)
		{
			Fraction ratA = new Fraction(a);
			Fraction negB = new Fraction(-b);
			BigRational aOverB = BigRational.Divide(ratA, negB);

			BigRational left = polynomial.Evaluate(aOverB);
			BigRational right = BigRational.Pow(new BigRational(negB), polynomial.Degree);

			BigRational result = BigRational.Multiply(left, right);
			return result;
		}

		public static BigInteger Algebraic(int a, int b, AlgebraicPolynomial poly)
		{
			// b^deg * f( a/b )

			int bneg = -b;
			double ab = (double)a / (double)bneg;

			//BigInteger remainder = new BigInteger();
			//BigInteger quotient = BigInteger.DivRem(a, bneg, out remainder);
			//double remaind = (double)remainder / (double)bneg;

			double right = poly.Evaluate(ab);
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
			return BigInteger.Add(a, BigInteger.Multiply(b, polynomialBase));
		}
	}
}
