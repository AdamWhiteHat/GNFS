using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;

namespace GNFSCore.Polynomial
{
	using Internal;

	public class SkewSymmetricPolynomial
	{
		public int Degree { get; private set; }
		public BigInteger N { get; private set; }
		public BigInteger BaseA { get; private set; }
		public BigInteger BaseB { get; private set; }
		public BigInteger[] Terms { get; private set; }


		public SkewSymmetricPolynomial(GNFS gnfs, int degree)
		{
			N = gnfs.N;
			Degree = degree;

			if (N.ToString().Count() < 10)
			{
				throw new Exception("Call the other constructor overloads if log10(N) < 10");
			}

			IEnumerable<int> primes = PrimeFactory.GetPrimes(10000);

			BigInteger polyBaseA = N.NthRoot(Degree + 1);
			BigInteger polyBaseB = polyBaseA.NthRoot(3);

			polyBaseA = CommonPolynomial.SuggestPolynomialBase(N, degree, primes, SearchDirection.Decrement);
			polyBaseB = CommonPolynomial.FindNextSmooth(polyBaseB, primes);

			BaseA = polyBaseA;
			BaseB = polyBaseB;
			Terms = Enumerable.Repeat(BigInteger.Zero, degree + 1).ToArray();
			SetPolynomialFromValue(N, BaseA, BaseB);
		}

		public SkewSymmetricPolynomial(BigInteger n, int degree, BigInteger polyBaseA, BigInteger polyBaseB)
		{
			N = n;
			Degree = degree;
			BaseA = polyBaseA;
			BaseB = polyBaseB;
			Terms = Enumerable.Repeat(BigInteger.Zero, degree + 1).ToArray();
			SetPolynomialFromValue(N, BaseA, BaseB);
		}

		public SkewSymmetricPolynomial(BigInteger n, int degree, BigInteger[] terms)
		{
			N = n;
			Degree = degree;
			Terms = terms;
		}

		private void SetPolynomialFromValue(BigInteger value, BigInteger polyBaseA, BigInteger polyBaseB)
		{
			N = value;
			int d = this.Degree;
			BigInteger toAdd = N;

			BigInteger DthRoot = N.NthRoot(d + 1) + 1;

			d = d + 1;
			while (--d >= 0) // Build out Terms[]
			{
				BigInteger placeValue = CalculatePlaceValue(this, d);

				if (d == 0)
				{
					Terms[d] = toAdd;
					toAdd = 0;
				}
				else
				{
					if (placeValue > toAdd)
					{
						if (d < this.Degree)
						{
							while (placeValue > toAdd)
							{
								int biggerDegree = (d + 1);
								Terms[biggerDegree] -= 1;
								toAdd += CalculatePlaceValue(this, biggerDegree);
							}
						}
						else
						{
							throw new Exception("Stuck on first term and PlaceValue > ToAdd!");
						}
					}

					if (placeValue < toAdd)
					{
						BigInteger maxTermValue = DthRoot;

						if (d == this.Degree)
						{
							int counter = d + 1;
							List<BigInteger> coefficients = new List<BigInteger>();
							while (counter-- > 0)
							{
								coefficients.Add(CalculatePlaceValue(this, counter));
							}

							BigInteger sum = coefficients.Sum();
							BigInteger leadingCoefficientValue = BigInteger.Divide(N, sum) + 1;

							Terms[d] = leadingCoefficientValue; // Make monic
							toAdd -= BigInteger.Multiply(leadingCoefficientValue, placeValue);
							continue;
						}

						BigInteger maxContribution = BigInteger.Multiply(maxTermValue, placeValue);

						if (toAdd >= maxContribution)
						{
							Terms[d] = maxTermValue;
							toAdd -= maxContribution;
						}
						else
						{
							BigInteger quotient = BigInteger.Divide(toAdd, placeValue);
							Terms[d] = quotient;
							BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);
							toAdd -= toSubtract;
						}
					}
					else
					{
						throw new Exception("Failed to correct for condition: PlaceValue > ToAdd!");
					}
				}
				// Taken care of during the while loop check...
				//d--;
			}
		}

		public BigInteger Evaluate(BigInteger baseA, BigInteger baseB)
		{
			BigInteger result = 0;

			int d = this.Degree;
			while (d >= 0)
			{
				BigInteger x = BigInteger.Pow(baseA, d);
				BigInteger y = BigInteger.Pow(baseB, this.Degree - d);

				BigInteger placeValue = BigInteger.Multiply(x, y);

				BigInteger addValue = BigInteger.Multiply(this.Terms[d], placeValue);

				result = BigInteger.Add(result, addValue);

				d--;
			}

			return result;
		}

		public BigInteger Derivative(BigInteger baseA, BigInteger baseB)
		{
			BigInteger result = new BigInteger(0);

			int d = this.Degree;
			int d1 = d - 1;
			while (d >= 0)
			{
				BigInteger x = BigInteger.Pow(baseA, d);
				BigInteger y = BigInteger.Pow(baseB, this.Degree - d);

				BigInteger placeValue = BigInteger.Multiply(x, y);

				BigInteger addValue = BigInteger.Multiply(BigInteger.Multiply(d, placeValue), (BigInteger)this.Terms[d]);
				result += addValue;

				d--;
			}

			return result;
		}

		public List<int> GetRootsMod(BigInteger baseA, BigInteger baseB, IEnumerable<int> modList)
		{
			BigInteger polyResult = Evaluate(baseA, baseB);
			IEnumerable<int> result = modList.Where(mod => (polyResult % mod) == 0);
			return result.ToList();
		}

		public static BigInteger CalculatePlaceValue(SkewSymmetricPolynomial poly, int positionD)
		{
			if (positionD == 0)
			{
				return BigInteger.One;
			}
			int smallerDegree = Math.Min(poly.Degree, Math.Abs(poly.Degree - positionD) + 1);

			BigInteger x = BigInteger.Pow(poly.BaseA, positionD);
			BigInteger y = (smallerDegree == 0) ? 1 : BigInteger.Pow(poly.BaseB, smallerDegree);
			return BigInteger.Multiply(x, y);
		}

		private static readonly string MultiplyString = " * ";
		public override string ToString()
		{
			List<string> stringTerms = new List<string>();

			int degree = this.Terms.Length - 1;
			while (degree >= 0)
			{
				int degreeMinusOne = (degree - 1);
				int smallerDegree = Math.Min(this.Degree, Math.Abs(this.Degree - degree) + 1);

				BigInteger y = (degreeMinusOne == 0) ? 1 : BigInteger.Pow(this.BaseB, (this.Degree - degreeMinusOne));
				string a = $"{this.BaseA}^{degree}";
				string timesAB = MultiplyString;
				string b = $"{this.BaseB}^{smallerDegree}";

				if (degree == 0)
				{
					a = "";
					timesAB = "";
					b = "";
				}
				else if (degree == 1)
				{
					a = this.BaseA.ToString();
				}

				if (smallerDegree == 0)
				{
					b = "";
					timesAB = "";
				}
				else if (smallerDegree == 1)
				{
					b = this.BaseB.ToString();
				}

				string exponent = $"{a}{timesAB}{b}";
				string timesExpTrm = MultiplyString;
				string term = $"{this.Terms[degree]}";

				if (this.Terms[degree] == 1)
				{
					term = "1";
					timesExpTrm = "";
				}
				if (string.IsNullOrWhiteSpace(exponent))
				{
					timesExpTrm = "";
				}

				string termDegree = $"{term}{timesExpTrm}{exponent}";

				if (!string.IsNullOrWhiteSpace(termDegree))
				{
					// Add term
					stringTerms.Add(termDegree);
				}


				degree--;
			}

			return "   " + string.Join($"{Environment.NewLine} + ", stringTerms);
		}
	}
}
