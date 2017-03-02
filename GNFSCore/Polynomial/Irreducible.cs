﻿using System;
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
			Terms = Enumerable.Repeat(BigInteger.Zero, degree).ToArray();

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

				if (placeValue == 1)
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

		public BigInteger Eval(BigInteger baseM)
		{
			BigInteger result = 0;

			int d = Degree-1;
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(baseM, d);
				BigInteger addValue = Terms[d] * placeValue;

				result += addValue;

				d--;
			}

			return result;
		}

		public IEnumerable<int> GetRootsMod(BigInteger baseM, IEnumerable<int> modList)
		{
			BigInteger polyResult = Eval(baseM);
			IEnumerable<int> result = modList.Where(mod => (polyResult % mod) == 0);
			return result;
		}

		public override string ToString()
		{
			return FormatString(this.Base, this.Terms);
		}

		public static string FormatString(BigInteger polyBase, BigInteger[] terms)
		{
			List<string> stringTerms = new List<string>();

			int degree = terms.Length - 1;
			while (degree >= 0)
			{
				if (degree > 1)
				{
					if (terms[degree] == 1)
					{
						stringTerms.Add($"{polyBase}^{degree}");
					}
					else
					{
						stringTerms.Add($"{terms[degree]} * {polyBase}^{degree}");
					}
				}
				else if (degree == 1)
				{
					stringTerms.Add($"{terms[degree]} * {polyBase}");
				}
				else if (degree == 0)
				{
					stringTerms.Add($"{terms[degree]}");
				}

				degree--;
			}

			return string.Join(" + ", stringTerms);
		}
	}
}