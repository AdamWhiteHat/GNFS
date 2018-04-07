using GNFSCore.IntegerMath;
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

		BigInteger Evaluate(BigInteger baseM);
		BigInteger Derivative(BigInteger baseM);
		List<BigInteger> GetRootsMod(BigInteger baseM, IEnumerable<BigInteger> modList);
	}

	public enum SearchDirection
	{
		Increment,
		Decrement
	}

	namespace Internal
	{
		public static class CommonPolynomial
		{
			public static BigInteger SuggestPolynomialBase(BigInteger n, int degree, IEnumerable<BigInteger> primeFactorBase, SearchDirection searchDirection = SearchDirection.Increment)
			{
				BigInteger polyBaseA = n.NthRoot(degree + 1);

				BigInteger result = FindNextSmooth(polyBaseA, primeFactorBase, searchDirection);
				return result;
			}

			public static BigInteger FindNextSmooth(BigInteger n, IEnumerable<BigInteger> primeFactorBase, SearchDirection searchDirection = SearchDirection.Increment, int maxRounds = 10000000)
			{
				BigInteger incrementValue = 2;

				if (searchDirection == SearchDirection.Decrement)
				{
					incrementValue = BigInteger.Negate(incrementValue);
				}

				if (n % 2 != 0)
				{
					n += 1;
				}

				int counter = maxRounds;
				bool isSmooth = false;
				do
				{
					n += incrementValue;
					isSmooth = FactorizationFactory.IsSmoothOverFactorBase(n, primeFactorBase);
					counter -= 1;
				}
				while (!isSmooth && counter > 0);

				return n;
			}

			public static List<BigInteger> GetRootsMod(IPolynomial polynomial, BigInteger baseM, IEnumerable<BigInteger> modList)
			{
				BigInteger polyResult = Evaluate(polynomial, baseM);
				IEnumerable<BigInteger> result = modList.Where(mod => (polyResult % mod) == 0);
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

			public static BigInteger Evaluate(IPolynomial polynomial, BigInteger baseM)
			{
				return Evaluate(polynomial.Degree, polynomial.Terms, baseM);
			}

			public static BigInteger Evaluate(int degree, BigInteger[] terms, BigInteger baseM)
			{
				BigInteger result = 0;

				int d = degree;
				BigInteger[] localTerms = terms.ToArray();
				while (d >= 0)
				{
					BigInteger placeValue = BigInteger.Pow(baseM, d);

					BigInteger addValue = BigInteger.Multiply(localTerms[d], placeValue);

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
