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
		public BigInteger[] Terms { get; private set; }

		public Irreducible(BigInteger n, BigInteger polynomialBase, int degree)
		{
			Base = polynomialBase;
			Degree = degree;
			Terms = Enumerable.Repeat(BigInteger.Zero, degree + 1).ToArray();

			SetPolynomialValue(n);
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

				if (toAdd == 0 || placeValue > toAdd)
				{
					Terms[d] = 0;
				}
				else if (placeValue == 1)
				{
					Terms[d] = toAdd;
				}
				else if (placeValue < toAdd)
				{
					BigInteger quotient = BigInteger.Divide(toAdd, placeValue);
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

		public BigInteger Eval(BigInteger primeBase)
		{
			BigInteger result = 0;

			int d = Degree;
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(primeBase, d);
				BigInteger addValue = Terms[d] * placeValue;

				result += addValue;

				d--;
			}

			return result;
		}

		public int EvalMod(BigInteger primeBase, int mod)
		{
			return (int)Eval(primeBase) % mod;
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public static string FormatString(BigInteger polyBase, BigInteger[] terms)
		{
			List<string> stringTerms = new List<string>();

			int degree = terms.Length - 1;
			while (degree >= 0)
			{
				stringTerms.Add($"{terms[degree]} * {polyBase}^{degree}");

				degree--;
			}

			return string.Join(" +  ", stringTerms);
		}
	}
}