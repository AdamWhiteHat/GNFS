using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class SquaresMethod
	{
		BigInteger n;
		List<BigInteger> squares;
		List<BigInteger> combinations;

		public SquaresMethod(BigInteger N, IEnumerable<BigInteger> smoothSquares)
		{
			n = N;
			squares = smoothSquares.ToList();
			combinations = squares;
		}

		public BigInteger[] Step()
		{
			List<BigInteger[]> combos = Combinatorics.GetCombination(combinations);

			combinations.Clear();
			combinations.AddRange(combos.Select(arr => arr.Product()));
			combinations.AddRange(squares);
			combinations = combinations.Distinct()/*.OrderByDescending(bi => bi)*/.ToList();

			var congruentSquares = combinations.Select(bi => new Tuple<BigInteger, BigInteger>(bi, bi % n));
			congruentSquares = congruentSquares.Where(tup => tup.Item1 != tup.Item2);
			congruentSquares = congruentSquares.Where(tup => tup.Item2.IsSquare());

			if (congruentSquares.Any())
			{
				//congruentSquares = congruentSquares.OrderBy(tup => tup.Item1);
				var roots = congruentSquares.Select(tup => new Tuple<BigInteger, BigInteger>(tup.Item1.SquareRoot(), tup.Item2.SquareRoot()));

				List<BigInteger> plusminusRoots = new List<BigInteger>();
				plusminusRoots.AddRange(roots.Select(tup => BigInteger.Add(tup.Item1, tup.Item2)));
				plusminusRoots.AddRange(roots.Select(tup => BigInteger.Subtract(tup.Item1, tup.Item2)));

				IEnumerable<BigInteger> factors = plusminusRoots.Select(bi => GCD.FindGCD(n, bi)).Where(gcd => (gcd > 1) && (gcd != n));
				if (factors.Any())
				{
					return factors/*.Distinct().OrderByDescending(bi => bi)*/.ToArray();
				}
			}

			return new BigInteger[0];
		}
	}
}
