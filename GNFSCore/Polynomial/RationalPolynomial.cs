using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;

namespace GNFSCore.Polynomial
{
	public class RationalPolynomial
	{
		public int Degree { get; private set; }
		public BigInteger N { get; private set; }
		public BigInteger Base { get; private set; }
		public BigInteger[] Terms { get; private set; }


		public RationalPolynomial(BigInteger n, int degree, BigInteger polyBase, BigInteger fromValue)
		{
			N = n;
			Degree = degree;
			Base = polyBase;
			Terms = Enumerable.Repeat(BigInteger.Zero, degree + 1).ToArray();
			SetPolynomialFromValue(fromValue, polyBase);
		}

		public RationalPolynomial(BigInteger n, int degree, BigInteger[] terms)
		{
			N = n;
			Degree = degree;
			Terms = terms;
		}

		private void SetPolynomialFromValue(BigInteger value, BigInteger polyBase)
		{
			N = value;
			int d = Degree;
			BigInteger toAdd = N;

			// Build out Terms[]
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(polyBase, d);

				if (placeValue == 1)
				{
					Terms[d] = toAdd;
				}
				else if (placeValue < toAdd)
				{
					BigInteger quotient = BigInteger.Divide(toAdd, placeValue);

					if (quotient > 10)
					{
						quotient = BigInteger.Divide(quotient, d - 1);
					}

					Terms[d] = quotient;
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);
					toAdd -= toSubtract;
				}

				d--;
			}
		}

		public static BigInteger Evaluate(RationalPolynomial polynomial, BigInteger baseM)
		{
			BigInteger result = 0;

			int d = polynomial.Degree;
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(baseM, d);

				BigInteger addValue = (BigInteger)polynomial.Terms[d] * placeValue;

				result += addValue;

				d--;
			}

			return result;
		}

		public static BigInteger Derivative(RationalPolynomial polynomial, BigInteger baseM)
		{
			BigInteger result = 0;

			int d = polynomial.Degree;
			int d1 = d - 1;
			while (d >= 0)
			{
				BigInteger placeValue = 0;

				if (d1 > -1)
				{
					placeValue = BigInteger.Pow(baseM, d1);
				}

				BigInteger addValue = (BigInteger)polynomial.Terms[d] * d * placeValue;
				result += addValue;

				d--;
			}

			return result;
		}

		public static IEnumerable<int> GetRootsMod(RationalPolynomial polynomial, BigInteger baseM, IEnumerable<int> modList)
		{
			BigInteger polyResult = RationalPolynomial.Evaluate(polynomial, baseM);
			IEnumerable<int> result = modList.Where(mod => (polyResult % mod) == 0);
			return result;
		}


		public override string ToString()
		{
			return RationalPolynomial.FormatString(this);
		}

		public static string FormatString(RationalPolynomial polynomial)
		{
			List<string> stringTerms = new List<string>();

			int degree = polynomial.Terms.Length - 1;
			while (degree >= 0)
			{
				if (degree > 1)
				{
					if (polynomial.Terms[degree] == 1)
					{
						stringTerms.Add($"{polynomial.Base}^{degree}");
					}
					else
					{
						stringTerms.Add($"{polynomial.Terms[degree]} * {polynomial.Base}^{degree}");
					}
				}
				else if (degree == 1)
				{
					stringTerms.Add($"{polynomial.Terms[degree]} * {polynomial.Base}");
				}
				else if (degree == 0)
				{
					stringTerms.Add($"{polynomial.Terms[degree]}");
				}

				degree--;
			}

			return string.Join(" + ", stringTerms);
		}
	}
}
