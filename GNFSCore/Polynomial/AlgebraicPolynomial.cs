using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;
using ExtendedNumerics;

namespace GNFSCore.Polynomial
{
	using Internal;

	public class AlgebraicPolynomial : IPolynomial
	{
		public int Degree { get; private set; }
		public BigInteger N { get; private set; }
		public BigInteger Base { get; private set; }
		public BigInteger[] Terms { get; private set; }
		public BigInteger BaseTotal { get; private set; }
		public BigInteger FormalDerivative { get; private set; }

		public AlgebraicPolynomial(BigInteger n, BigInteger polynomialBase, int degree)
		{
			Base = polynomialBase;
			Degree = degree;
			Terms = Enumerable.Repeat(BigInteger.Zero, degree + 1).ToArray();

			N = n;
			SetPolynomialValue(N);

			BaseTotal = PolynomialCommon.Evaluate(this, Base);
			FormalDerivative = PolynomialCommon.Derivative(this, Base);
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
					Terms[d] = toAdd;
				}
				else if (placeValue < BigInteger.Abs(toAdd))
				{
					BigInteger remainder = 0;
					BigInteger quotient = BigInteger.DivRem(toAdd, placeValue, out remainder);

					Fraction fractionRemainder = new Fraction(remainder, placeValue);

					//bool roundUp = (Fraction.Abs(fractionRemainder) > Fraction.OneHalf);

					//if (roundUp)
					//{
					//	int adjustment = fractionRemainder.Sign;
					//	quotient += adjustment;
					//}

					if (quotient > Base)
					{
						quotient = Base;
					}

					Terms[d] = quotient;
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);
					toAdd -= toSubtract;
				}

				d--;
			}
		}

		public double Evaluate(double baseM)
		{
			return PolynomialCommon.Evaluate(this, baseM);
		}

		public BigInteger Evaluate(BigInteger baseM)
		{
			return PolynomialCommon.Evaluate(this, baseM);
		}

		public BigRational Evaluate(BigRational baseM)
		{
			return PolynomialCommon.Evaluate(this, baseM);
		}

		public BigInteger Derivative(BigInteger baseM)
		{
			return PolynomialCommon.Derivative(this, baseM);
		}

		public BigInteger g(BigInteger x, int p)
		{
			return BigInteger.Subtract(BigInteger.Pow(x, p), x);
		}

		public List<int> GetRootsMod(BigInteger baseM, IEnumerable<int> modList)
		{
			return PolynomialCommon.GetRootsMod(this, baseM, modList);
		}

		public IEnumerable<int> GetRootsModEnumerable(BigInteger baseM, IEnumerable<int> modList)
		{
			BigInteger polyResult = Evaluate(baseM);
			IEnumerable<int> primeList = modList;

			foreach (int mod in primeList)
			{
				if ((polyResult % mod) == 0)
				{
					yield return mod;
				}
			}

			yield break;
		}

		public override string ToString()
		{
			return PolynomialCommon.FormatString(this);
		}
	}
}