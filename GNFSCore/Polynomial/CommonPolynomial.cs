using ExtendedNumerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Polynomial
{
	public interface IPolynomial
	{
		int Degree { get; }
		BigInteger Base { get; }
		BigInteger[] Terms { get; }
	}

	namespace Internal
	{
		public static class CommonPolynomial
		{
			public static BigInteger SuggestPolynomialBase(BigInteger n, int degree)
			{
				return n.NthRoot(degree);
			}

			public static List<int> GetRootsMod(IPolynomial polynomial, BigInteger baseM, IEnumerable<int> modList)
			{
				BigInteger polyResult = Evaluate(polynomial, baseM);
				IEnumerable<int> result = modList.Where(mod => (polyResult % mod) == 0);
				return result.ToList();
			}

			public static double Evaluate(IPolynomial polynomial, double baseM)
			{
				double result = 0;

				int d = polynomial.Degree;
				while (d >= 0)
				{
					double placeValue = Math.Pow(baseM, d);

					double addValue = (double)(polynomial.Terms[d]) * placeValue;

					result += addValue;

					d--;
				}

				return result;
			}

			public static BigRational Evaluate(IPolynomial polynomial, BigRational baseM)
			{
				BigRational result = new BigRational(0);

				int d = polynomial.Degree;
				while (d >= 0)
				{
					BigRational placeValue = BigRational.Pow(baseM, d);

					BigRational addValue = BigRational.Multiply(new BigRational(polynomial.Terms[d]), placeValue);

					result = BigRational.Add(result, addValue);

					d--;
				}

				return result;
			}

			public static BigInteger Evaluate(IPolynomial polynomial, BigInteger baseM)
			{
				BigInteger result = 0;

				int d = polynomial.Degree;
				while (d >= 0)
				{
					BigInteger placeValue = BigInteger.Pow(baseM, d);

					BigInteger addValue = BigInteger.Multiply(polynomial.Terms[d], placeValue);

					result = BigInteger.Add(result, addValue);

					d--;
				}

				return result;
			}

			public static BigInteger Derivative(IPolynomial polynomial, BigInteger baseM)
			{
				BigInteger result = new BigInteger(0);
				BigInteger m = baseM;
				int d = polynomial.Degree;
				int d1 = d - 1;
				while (d >= 0)
				{
					BigInteger placeValue = 0;

					if (d1 > -1)
					{
						placeValue = BigInteger.Pow(m, d1);
					}

					BigInteger addValue = BigInteger.Multiply(BigInteger.Multiply(d, placeValue), (BigInteger)polynomial.Terms[d]);
					result += addValue;

					d--;
				}

				return result;
			}

			public static void MakeCoefficientsSmaller(IPolynomial polynomial)
			{
				int pos = 0;
				int deg = polynomial.Degree;

				while (pos < deg)
				{
					if (pos + 1 > deg - 1)
					{
						return;
					}

					if (polynomial.Terms[pos] > polynomial.Base &&
						polynomial.Terms[pos] > polynomial.Terms[pos + 1])
					{
						polynomial.Terms[pos] -= polynomial.Base;
						polynomial.Terms[pos + 1] += 1;
					}

					pos++;
				}
			}

			public static string FormatString(IPolynomial polynomial)
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
}
