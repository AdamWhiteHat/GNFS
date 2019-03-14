using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GNFSCore
{
	using Interfaces;

	public class Term : ITerm
	{
		public static Term Zero = new Term(BigInteger.Zero, 0);

		[JsonProperty]
		public BigInteger CoEfficient { get; set; }
		[JsonProperty]
		public int Exponent { get; set; }


		[JsonIgnore]
		private static string IndeterminateSymbol = "X";

		public Term()
		{
		}

		public Term(BigInteger coefficient, int exponent)
		{
			CoEfficient = coefficient;
			Exponent = exponent;
		}

		public static ITerm[] GetTerms(BigInteger[] terms)
		{
			List<ITerm> results = new List<ITerm>();

			int degree = 0;
			foreach (BigInteger term in terms)
			{
				results.Add(new Term(term, degree));

				degree += 1;
			}

			return results.ToArray();
		}

		public BigInteger Evaluate(BigInteger indeterminate)
		{
			return BigInteger.Multiply(CoEfficient, BigInteger.Pow(indeterminate, Exponent));
		}

		public ITerm Clone()
		{
			return new Term(this.CoEfficient, this.Exponent);
		}

		public override string ToString()
		{
			return $"{CoEfficient}*{IndeterminateSymbol}^{Exponent}";
		}
	}
}
