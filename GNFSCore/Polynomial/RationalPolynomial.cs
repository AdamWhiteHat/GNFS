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

	public class RationalPolynomial : IPolynomial
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

		public BigInteger Evaluate(RationalPolynomial polynomial, BigInteger baseM)
		{
			return PolynomialCommon.Evaluate(this, baseM);
		}

		public BigInteger Derivative(RationalPolynomial polynomial, BigInteger baseM)
		{
			return	PolynomialCommon.Derivative(this, baseM);
		}

		public IEnumerable<int> GetRootsMod(BigInteger baseM, IEnumerable<int> modList)
		{
			return PolynomialCommon.GetRootsMod(this, baseM, modList);
		}

		public override string ToString()
		{
			return PolynomialCommon.FormatString(this);
		}
	}
}
