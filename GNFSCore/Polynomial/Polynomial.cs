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
		public int Degree { get; private set; } = 0;
		public List<BigInteger> Terms { get; private set; } = new List<BigInteger>();

		public Polynomial(BigInteger n, BigInteger primeBase, int degree)
		{
			N = n;
			Base = primeBase;
			Degree = degree;


			BigInteger toAdd = N;
			Terms = new List<BigInteger>();

			int d = Degree;
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(Base, d);
				BigInteger maxPlaceValue = BigInteger.Multiply(placeValue, Base);

				if (toAdd == 0 || placeValue < toAdd)
				{
					Terms.Add(0);
				}
				else if (placeValue > toAdd)
				{
					BigInteger quotient = BigInteger.Divide(toAdd, placeValue);
					if (quotient > Base)
					{
						quotient = Base;
					}

					Terms.Add(quotient);
					toAdd -= BigInteger.Multiply(quotient, placeValue);
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
			return Eval(primeBase) % mod;
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

		public static string FormatString(BigInteger polyBase, List<BigInteger> terms)
		{
			List<string> stringTerms = new List<string>();

			int degree = terms.Count - 1;
			while (degree >= 0)
			{
				stringTerms.Add($"{terms[degree]} * {polyBase}^{degree}");

				degree--;
			}

			return string.Join(" +  ", stringTerms);
		}
	}
}