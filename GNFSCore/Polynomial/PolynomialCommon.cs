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
		internal static class PolynomialCommon
		{
			public static IEnumerable<int> GetRootsMod(IPolynomial polynomial, BigInteger baseM, IEnumerable<int> modList)
			{
				BigInteger polyResult = Evaluate(polynomial, baseM);
				IEnumerable<int> result = modList.Where(mod => (polyResult % mod) == 0);
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
