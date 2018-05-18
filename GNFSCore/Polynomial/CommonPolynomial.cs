using System;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using GNFSCore.IntegerMath;

namespace GNFSCore.Polynomial
{
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
					d1--;
				}

				return result;
			}

			public static IPolynomial GetDerivativePolynomial(IPolynomial poly)
			{
				int d = poly.Degree + 1;

				BigInteger[] terms = new BigInteger[d - 1];
				while (d >= 0)
				{
					d--;

					if (d < 1)
					{
						continue;
					}

					BigInteger item = poly.Terms[d];

					BigInteger value = d * item;
					terms[d - 1] = value;
				}

				IPolynomial result = new AlgebraicPolynomial(terms.ToArray());
				return result;
			}

			public static IPolynomial RandomPolynomial(int degree, BigInteger polynomialBase, BigInteger minimumCoefficentValue, BigInteger maximumCoefficentValue)
			{
				List<BigInteger> terms = new List<BigInteger>();

				for (int index = 0; index <= degree; index++)
				{
					terms.Add(StaticRandom.NextBigInteger(minimumCoefficentValue, maximumCoefficentValue));
				}

				return new AlgebraicPolynomial(terms.ToArray());
			}

			public static void MakeCoefficientsSmaller(IPolynomial polynomial, BigInteger polynomialBase)
			{
				int pos = 0;
				int deg = polynomial.Degree;

				while (pos < deg)
				{
					if (pos + 1 > deg - 1)
					{
						return;
					}

					if (polynomial.Terms[pos] > polynomialBase &&
						polynomial.Terms[pos] > polynomial.Terms[pos + 1])
					{
						polynomial.Terms[pos] -= polynomialBase;
						polynomial.Terms[pos + 1] += 1;
					}

					pos++;
				}
			}

			public static IPolynomial MakeMonic(IPolynomial polynomial, BigInteger polynomialBase)
			{
				IPolynomial result = new AlgebraicPolynomial(polynomial.Terms.ToArray());

				int deg = result.Degree;

				if (BigInteger.Abs(result.Terms[deg]) > 1)
				{
					BigInteger toAdd = (result.Terms[deg] - 1) * polynomialBase;

					result.Terms[deg] = 1;

					result.Terms[deg - 1] += toAdd;
				}

				return result;
			}

			public static IPolynomial GCD(IPolynomial left, IPolynomial right)
			{
				IPolynomial a = new AlgebraicPolynomial(left.Terms.ToArray());
				IPolynomial b = new AlgebraicPolynomial(right.Terms.ToArray());


				if (b.Degree > a.Degree)
				{
					IPolynomial swap = b;
					b = a;
					a = swap;
				}

				while (!(b.Terms.Length == 1 && b.Terms[0] == 0))
				{
					IPolynomial temp = a;
					a = b;
					b = Mod(temp, b);
				}

				return new AlgebraicPolynomial(a.Terms.ToArray());
			}

			public static IPolynomial Mod(IPolynomial left, IPolynomial right)
			{
				IPolynomial remainder = new AlgebraicPolynomial();
				Divide(left, right, out remainder);
				return remainder;
			}

			public static BigInteger[] RemoveZeros(BigInteger[] terms)
			{
				List<BigInteger> result = terms.ToList();

				int i = result.Count - 1;
				while (result[i] == 0)
				{
					result.RemoveAt(i);
					i--;
				}

				return result.ToArray();
			}

			public static IPolynomial Divide(IPolynomial left, IPolynomial right, out IPolynomial remainder)
			{
				if (left == null) throw new ArgumentNullException(nameof(left));
				if (right == null) throw new ArgumentNullException(nameof(right));

				if (right.Degree > left.Degree)
				{
					throw new InvalidOperationException();
				}

				int i = 0;
				int leftDegree = left.Degree;
				int rightDegree = right.Degree;
				BigInteger leadingCoefficent = right.Terms[rightDegree].Clone();

				// the leading coefficient is the only number we ever divide by
				// (so if right is monic, polynomial division does not involve division at all!)

				BigInteger[] rem = left.Terms.ToArray();

				BigInteger[] quotient = new BigInteger[leftDegree - rightDegree + 1];
				for (i = quotient.Length - 1; i >= 0; i--)
				{
					quotient[i] = BigInteger.Divide(rem[rightDegree + i], leadingCoefficent);
					rem[rightDegree + i] = new BigInteger(0);
					for (int j = rightDegree + i - 1; j >= i; j--)
					{
						rem[j] = BigInteger.Subtract(rem[j], BigInteger.Multiply(quotient[i], right.Terms[j - i]));
					}
				}

				rem = RemoveZeros(rem);
				quotient = RemoveZeros(quotient);


				// form the remainder and quotient polynomials from the arrays
				remainder = new AlgebraicPolynomial(rem.ToArray());
				return new AlgebraicPolynomial(quotient.ToArray());
			}

			public static IPolynomial Divide(IPolynomial poly, BigInteger divisor)
			{
				BigInteger[] coefficientQuotients = poly.Terms.Select(term => BigInteger.Divide(term, divisor)).ToArray();
				return new AlgebraicPolynomial(coefficientQuotients);
			}

			public static IPolynomial Add(IPolynomial left, IPolynomial right)
			{
				if (left == null) throw new ArgumentNullException(nameof(left));
				if (right == null) throw new ArgumentNullException(nameof(right));

				BigInteger[] terms = new BigInteger[Math.Max(left.Degree, right.Degree) + 1];
				for (int i = 0; i < terms.Length; i++)
				{
					terms[i] = BigInteger.Add(left.Terms[i], right.Terms[i]);
				}
				return new AlgebraicPolynomial(terms);
			}

			public static IPolynomial Subtract(IPolynomial left, IPolynomial right)
			{
				if (left == null) throw new ArgumentNullException(nameof(left));
				if (right == null) throw new ArgumentNullException(nameof(right));

				BigInteger[] terms = new BigInteger[Math.Max(left.Degree, right.Degree) + 1];
				for (int i = 0; i < terms.Length; i++)
				{
					terms[i] = BigInteger.Subtract(left.Terms[i], right.Terms[i]);
				}
				return new AlgebraicPolynomial(terms);
			}

			public static IPolynomial Multiply(IPolynomial left, IPolynomial right)
			{
				if (left == null) throw new ArgumentNullException(nameof(left));
				if (right == null) throw new ArgumentNullException(nameof(right));

				BigInteger[] terms = new BigInteger[left.Degree + right.Degree + 1];
				for (int i = 0; i <= left.Degree; i++)
				{
					for (int j = 0; j <= right.Degree; j++)
					{
						terms[i + j] += BigInteger.Multiply(left.Terms[i], right.Terms[j]);
					}
				}
				return new AlgebraicPolynomial(terms);
			}

			public static IPolynomial Inverse(IPolynomial poly)
			{
				return new AlgebraicPolynomial(poly.Terms.Select(coeff => BigInteger.Negate(coeff)).ToArray());
			}

			public static BigInteger Content(IPolynomial poly)
			{
				return IntegerMath.GCD.FindGCD(poly.Terms);
			}

			public static IPolynomial PrimitivePart(IPolynomial poly)
			{
				BigInteger content = Content(poly);
				return Divide(poly, content);
			}

			public static string FormatString(IPolynomial polynomial)
			{
				string variable = "X";

				List<string> stringTerms = new List<string>();

				bool firstPass = true;
				int degree = polynomial.Terms.Length - 1;
				while (degree >= 0)
				{
					BigInteger term = polynomial.Terms[degree];

					if (firstPass)
					{
						firstPass = false;
					}
					else
					{
						if (term.Sign == -1)
						{
							stringTerms.Add(" - ");
						}
						else
						{
							stringTerms.Add(" + ");
						}
					}

					if (degree < polynomial.Terms.Length - 1)
					{
						term = BigInteger.Abs(term);
					}

					if (degree > 1)
					{
						if (term == 1)
						{
							stringTerms.Add($"{variable}^{degree}");
						}
						else
						{
							stringTerms.Add($"{term} * {variable}^{degree}");
						}
					}
					else if (degree == 1)
					{
						stringTerms.Add($"{term} * {variable}");
					}
					else if (degree == 0)
					{
						stringTerms.Add($"{term}");
					}



					degree--;
				}

				return string.Join(string.Empty, stringTerms);
			}
		}
	}
}
