using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.PrimeSignature
{
	using IntegerMath;
	using System.Numerics;

	public class MatrixSolver
	{
		public static List<BigInteger[]> GetTrivialSquares(BitMatrix matrix)
		{
			return matrix.Rows.Where(r => r.RowSum == 0).Select(r => r.Number).Select(i => new BigInteger[] { i }).ToList();
		}

		public static List<BigInteger[]> GetSingleFactors(BitMatrix bitMatrix, IEnumerable<IGrouping<int, BitVector>> input)
		{
			if (input == null)
			{
				throw new ArgumentException(nameof(input));
			}

			List<BigInteger[]> results = new List<BigInteger[]>();
			// Single factors
			foreach (var group in input)
			{
				if (group.Count() > 1)
				{
					BigInteger[] groupNumbers = group.Select(v => v.Number).ToArray();

					bitMatrix.Remove(groupNumbers);
					results.AddRange(Combinatorics.GetCombination(groupNumbers));
				}
				else
				{
					throw new Exception();
				}
			}

			return results;
		}

		public static List<BigInteger[]> GetSimpleMatches(BitMatrix bitMatrix)
		{
			if (bitMatrix == null)
			{
				throw new ArgumentException(nameof(bitMatrix));
			}

			// Simple matches
			int skip = 0;
			bool done = false;

			BitVector[] input = bitMatrix.Rows.ToArray();
			List<BigInteger[]> results = new List<BigInteger[]>();
			do
			{
				if (skip >= input.Length)
				{
					break;
				}

				BitVector toMatch = input.Skip(skip).First();
				int[] matchPattern = BitPattern.GetPattern(toMatch);

				IEnumerable<BitVector> matches = BitPattern.FindExactMatches(matchPattern, input);

				if (matches.Count() > 1)
				{
					input = input.Except(matches).ToArray();
					BigInteger[] matchNumbers = matches.Select(v => v.Number).ToArray();
					bitMatrix.Remove(matchNumbers);
					results.AddRange(Combinatorics.GetCombination(matchNumbers));
				}
				else
				{
					skip++;
				}
			}
			while (!done);

			return results;
		}

		public static List<BigInteger[]> GetChainedFactors(BitMatrix bitMatrix)
		{
			if (bitMatrix == null)
			{
				throw new ArgumentException(nameof(bitMatrix));
			}

			// Chained factor matching 
			int skip = 0;
			bool done = false;

			BitVector[] input = bitMatrix.Rows.ToArray();
			List<BigInteger[]> result = new List<BigInteger[]>();
			do
			{
				if (skip >= input.Length)
				{
					break;
				}

				List<BitVector> matchCollection = new List<BitVector>();
				BitVector toMatch = input.Skip(skip).First();
				matchCollection.Add(toMatch);
				int[] matchPattern = BitPattern.GetPattern(toMatch);
				BitVector bestMatch = BitPattern.FindBestPartialMatch(matchPattern, input.Except(matchCollection));

				if (bestMatch == null || bestMatch == default(BitVector))
				{
					skip++;
					continue;
				}

				matchCollection.Add(bestMatch);
				bool[] combinedVector = BitVector.CombineVectors(matchCollection);

				while (!combinedVector.All(b => !b))
				{
					matchPattern = BitPattern.GetPattern(combinedVector);
					BitVector found = BitPattern.FindBestPartialMatch(matchPattern, input.Except(matchCollection));

					if (found == null || found == default(BitVector))
					{
						break;
					}

					matchCollection.Add(found);
					combinedVector = BitVector.CombineVectors(matchCollection);
				}

				if (combinedVector.All(b => !b))
				{
					input = input.Except(matchCollection).ToArray();
					BigInteger[] matchNumbers = matchCollection.Select(v => v.Number).ToArray();
					result.Add(matchNumbers);
				}
				else
				{
					skip++;
				}
			}
			while (!done);

			return result;
		}

		public static IEnumerable<BigInteger[]> GetSquareCombinations(BitMatrix matrix)
		{
			List<BigInteger[]> squareVectors = MatrixSolver.GetTrivialSquares(matrix); // Vectors who's RowSum is zero are already squares.
			List<BigInteger[]> result = squareVectors;  // Add trivial squares to result
														//result.AddRange(Combinatorics.GetCombination(squareNumbers)); 
			matrix.Remove(squareVectors);
			matrix.Rows.Reverse(); // Reverse array

			IEnumerable<BitVector> oneSums = matrix.Rows.Where(v => v.RowSum == 1); // Get vectors with only one odd factor exponents			
			IEnumerable<IGrouping<int, BitVector>> singleFactorGroups = oneSums.GroupBy(v => v.IndexOfLeftmostElement()).Where(g => g.Count() > 1); // Group vectors by their factor exponents
			IEnumerable<BitVector> toRemove = singleFactorGroups.SelectMany(g => g.Select(v => v));

			matrix.Remove(toRemove); // Remove selected vectors from remaining vectors

			List<BigInteger[]> singleFactorResults = MatrixSolver.GetSingleFactors(matrix, singleFactorGroups);
			List<BigInteger[]> simpleMatchResults = MatrixSolver.GetSimpleMatches(matrix);
			List<BigInteger[]> chainedFactorResults = MatrixSolver.GetChainedFactors(matrix);
			matrix.Remove(chainedFactorResults);

			result.AddRange(singleFactorResults);
			result.AddRange(simpleMatchResults);
			result.AddRange(chainedFactorResults);

			return result;
		}

		public static List<BigInteger> GetCombinationsProduct(IEnumerable<BigInteger[]> squareCombinations)
		{
			return squareCombinations.Select(arr => arr.Product()).ToList();
		}

		public static string FormatCombinations(IEnumerable<BigInteger[]> squareCombinations)
		{
			return string.Join(Environment.NewLine, squareCombinations.Select(arr => { BigInteger product = arr.Product(); return $"({product.IsSquare()})\t:\t{product}\t=>\t{{{string.Join(",", arr.Select(bi => bi.ToString()))}}}"; }));
		}
	}
}

