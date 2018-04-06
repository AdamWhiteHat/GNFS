using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Polynomial
{
	using Internal;
	public class RandomPolynomial
	{
		public int Degree { get; private set; }
		public BigInteger[] Terms { get; private set; }

		public BigInteger MinimumCoefficentValue { get; private set; }
		public BigInteger MaximumCoefficentValue { get; private set; }

		public RandomPolynomial(int degree, BigInteger minimumCoefficentValue, BigInteger maximumCoefficentValue)
		{
			Degree = degree;
			MinimumCoefficentValue = minimumCoefficentValue;
			MaximumCoefficentValue = maximumCoefficentValue;
			Terms = Enumerable.Repeat(BigInteger.Zero, Degree + 1).ToArray();

			RandomizeCoefficients();		
		}

		public void RandomizeCoefficients()
		{
			int index = 0;
			while (index <= Degree)
			{
				Terms[index] = StaticRandom.NextBigInteger(MinimumCoefficentValue, MaximumCoefficentValue);
			}
		}

		public BigInteger Evaluate(BigInteger baseM)
		{
			return CommonPolynomial.Evaluate(Degree, Terms, baseM);
		}
	}
}
