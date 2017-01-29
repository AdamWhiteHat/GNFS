using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace GNFSCore
{
	public class Polynomial
	{
		public BigInteger N { get; private set; }
		public BigInteger Base { get; private set; }
		public int Degree { get; private set; }
		public BigInteger[] Terms { get; private set; }

		public Polynomial(BigInteger n, BigInteger primeBase, int degree)
		{
			N = n;
			Base = primeBase;
			int d = Degree = degree;
			Terms = Enumerable.Repeat(BigInteger.Zero, degree + 1).ToArray();

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

		public BigInteger EvalMod(BigInteger primeBase, BigInteger mod)
		{
			BigInteger polyValue = Eval(primeBase);
			BigInteger result = polyValue % mod;
			return result;
		}

		public void MakeMonic()
		{
			throw new NotImplementedException();
		}

		public Polynomial GCD(Polynomial poly)
		{
			throw new NotImplementedException();
		}

		public BigInteger[] GetRoots()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return Polynomial.FormatString(Base, Terms);
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