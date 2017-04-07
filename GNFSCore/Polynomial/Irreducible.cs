using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;

namespace GNFSCore.Polynomial
{
	public class Irreducible
	{
		public int Degree { get; private set; }		
		public BigInteger N { get; private set; }
		public BigInteger Base { get; private set; }
		public double[] Terms { get; private set; }
		public BigInteger BaseTotal { get; private set; }
		public BigInteger FormalDerivative { get; private set; }

		public Irreducible(BigInteger n, BigInteger polynomialBase, int degree)
		{
			Base = polynomialBase;
			Degree = degree;
			Terms = Enumerable.Repeat(0d, degree + 1).ToArray();

			N = n;
			SetPolynomialValue(N);

			BaseTotal = Irreducible.Evaluate(this, Base);
			FormalDerivative = Irreducible.Derivative(this, Base);
		}

		private void SetPolynomialValue(BigInteger value)
		{
			N = value;
			int d = Degree;
			BigInteger toAdd = N;

			// Build out Terms[]
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(Base, d);

				if (placeValue == 1)
				{
					Terms[d] = (double)toAdd;
				}
				else if (placeValue < toAdd)
				{
					BigInteger quotient = BigInteger.Divide(toAdd, placeValue);
					if (quotient > Base)
					{
						quotient = Base;
					}

					Terms[d] = (double)quotient;
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);
					toAdd -= toSubtract;
				}

				d--;
			}
		}

		public static double Evaluate(Irreducible polynomial, double baseM)
		{
			double result = 0;

			int d = polynomial.Degree;
			while (d >= 0)
			{
				double placeValue = Math.Pow(baseM, d);

				double addValue = polynomial.Terms[d] * placeValue;

				result += addValue;

				d--;
			}

			return result;
		}

		public static BigInteger Evaluate(Irreducible polynomial, BigInteger baseM)
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

		public static BigInteger Derivative(Irreducible polynomial, BigInteger baseM)
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

		public static IEnumerable<int> GetRootsMod(Irreducible polynomial, BigInteger baseM, IEnumerable<int> modList)
		{
			BigInteger polyResult = Irreducible.Evaluate(polynomial, baseM);
			IEnumerable<int> result = modList.Where(mod => (polyResult % mod) == 0);
			return result;
		}

		public override string ToString()
		{
			return Irreducible.FormatString(this);
		}

		public static string FormatString(Irreducible polynomial)
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