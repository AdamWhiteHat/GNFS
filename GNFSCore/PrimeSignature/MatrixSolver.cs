using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.PrimeSignature
{
	using IntegerMath;

	public class MatrixSolver
	{
		public static BitVector[] GetTrivialSquares(BitMatrix matrix)
		{
			return matrix.Rows.Where(r => r.RowSum == 0).ToArray();
		}

		public static List<int[]> GetSingleFactors( BitMatrix bitMatrix, IEnumerable<IGrouping<int, BitVector>> input)
		{
			if (input == null)
			{
				throw new ArgumentException(nameof(input));
			}

			List<int[]> results = new List<int[]>();
			// Single factors
			foreach (var group in input)
			{
				if (group.Count() > 1)
				{
					int[] groupNumbers = group.Select(v => v.Number).ToArray();

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

		public static List<int[]> GetSimpleMatches(BitMatrix bitMatrix)
		{
			if (bitMatrix == null)
			{
				throw new ArgumentException(nameof(bitMatrix));
			}

			// Simple matches
			int skip = 0;
			bool done = false;

			BitVector[] input = bitMatrix.Rows.ToArray();
			List<int[]> results = new List<int[]>();
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
					int[] matchNumbers = matches.Select(v => v.Number).ToArray();
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

		public static List<int[]> GetChainedFactors(BitMatrix bitMatrix)
		{
			if (bitMatrix == null)
			{
				throw new ArgumentException(nameof(bitMatrix));
			}

			// Chained factor matching 
			int skip = 0;
			bool done = false;

			BitVector[] input = bitMatrix.Rows.ToArray();
			List<int[]> result = new List<int[]>();
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
					int[] matchNumbers = matchCollection.Select(v => v.Number).ToArray();
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
	}
}

