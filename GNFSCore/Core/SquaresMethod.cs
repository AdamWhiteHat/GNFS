using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GNFSCore.IntegerMath;

namespace GNFSCore.SquareRoot
{
	public class SquaresMethod
	{
		public BigInteger N = BigInteger.MinusOne;
		public BigInteger[] squares = new BigInteger[0];
		public List<BigInteger> combinations = new List<BigInteger>();
		public IEnumerable<BigInteger> lifted = null;


		public BigInteger Scale { get { return combinations.Max(); } }

		public SquaresMethod(BigInteger n, IEnumerable<BigInteger> smoothSquares)
		{
			combinations = new List<BigInteger>();

			N = n;
			squares = smoothSquares.ToArray();
			combinations.AddRange(squares);
		}

		public BigInteger[] Attempt(int permutations = 2)
		{
			if (lifted == null)
			{
				Permute(permutations);

				int toRaise = (int)BigInteger.Log(N, (double)Scale);

				lifted = combinations.Select(bi => BigInteger.Pow(bi, toRaise));


			}
			else
			{
				lifted = lifted.Select(bi => BigInteger.Pow(bi, 2));
			}

			return SiftFactors(lifted);
		}

		public void Permute(int permutations)
		{
			int counter = permutations;
			while (counter > 0)
			{
				Permute();
				counter--;
			}
		}

		public void Permute()
		{
			IEnumerable<BigInteger> cumulate = Combinatorics.GetCombination(combinations).Select(arr => arr.Product()).Distinct();
			combinations.AddRange(cumulate);

			//Remove duplicates
			Dedupe();
		}

		private void Dedupe()
		{
			List<BigInteger> distinct = combinations.Distinct().ToList();
			if (combinations.Count != distinct.Count)
			{
				combinations = distinct;
			}
		}

		public BigInteger[] SiftFactors(IEnumerable<BigInteger> squares)
		{
			IEnumerable<Factor> congruentSquares = squares.Select(sqr => new Factor(sqr, (sqr % N)));
			congruentSquares = congruentSquares.Where(factor => factor.Prime != factor.Exponent);
			congruentSquares = congruentSquares.Where(factor => factor.Exponent.IsSquare());

			if (congruentSquares.Any())
			{
				//congruentSquares = congruentSquares.OrderBy(factor => factor.Prime);
				IEnumerable<Factor> roots = congruentSquares.Select(factor => new Factor(factor.Prime.SquareRoot(), factor.Exponent.SquareRoot()));

				List<BigInteger> plusminusRoots = new List<BigInteger>();
				plusminusRoots.AddRange(roots.Select(factor => BigInteger.Add(factor.Prime, factor.Exponent)));
				plusminusRoots.AddRange(roots.Select(factor => BigInteger.Subtract(factor.Prime, factor.Exponent)));

				IEnumerable<BigInteger> result = plusminusRoots.Select(bi => GCD.FindGCD(N, bi)).Where(gcd => (gcd > 1) && (gcd != N));
				result = result.Distinct().OrderByDescending(bi => bi);

				if (result.Any())
				{
					return result.ToArray();
				}
			}

			return new BigInteger[0];
		}
	}
}
