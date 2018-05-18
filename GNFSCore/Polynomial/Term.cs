using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Polynomial
{
	using Internal;

	public class Term
	{
		public BigInteger Coefficient { get; set; }
		public char Variable { get; set; }
		public int Degree { get; set; }

		public Term(BigInteger coefficient, int degree)
		{
			Coefficient = coefficient;
			Degree = degree;
			Variable = 'X';
		}

		public BigInteger Evaluate(BigInteger variableValue)
		{
			return BigInteger.Multiply(Coefficient, BigInteger.Pow(variableValue, Degree));
		}

		public override string ToString()
		{
			return $"{Coefficient} * {Variable}^{Degree}";
		}
	}
}
