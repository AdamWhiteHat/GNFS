using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GNFSCore.PrimeSignature
{
	using IntegerMath;

	public class BitMatrix
	{
		public int Width;
		public List<BitVector> Rows;

		public int[] RowSums { get { return Rows.Select(r => r.RowSum).ToArray(); } }
		public int[] ColumnSums { get { return Enumerable.Range(0, Width).Select(i => ColumnSum(i)).ToArray(); } }

		public BitMatrix(IEnumerable<int> array, int primeBound)
		{
			int maxArraySquareRoot = (int)Math.Ceiling(Math.Sqrt((double)array.Max())) + 1;
			int maxArrayValue = PrimeFactory.GetValueFromIndex(maxArraySquareRoot);
			int maxValue = Math.Min(maxArrayValue, primeBound);

			Width = PrimeFactory.GetIndexFromValue(maxValue) + 1;

			IEnumerable<int> distinctNonPrimeValues = array.Select(i => Math.Abs(i)).Distinct().Where(i => i > 1 && !PrimeFactory.IsPrime(i));
			Rows = distinctNonPrimeValues.Select(i => new BitVector(i, maxValue)).ToList();

			IEnumerable<int> nonSquareColumns = Enumerable.Range(0, Width).Where(i => ColumnSum(i) == 1).ToArray();
			IEnumerable<BitVector> toRemove = nonSquareColumns.SelectMany(col => Rows.Where(r => r.Elements[col] == true));

			Remove(toRemove);

			SortRows();
		}

		public IEnumerable<int[]> GetSquareCombinations()
		{
			BitVector[] squareVectors = MatrixSolver.GetTrivialSquares(this); // Vectors who's RowSum is zero are already squares.
			int[] squareNumbers = squareVectors.Select(r => r.Number).ToArray();
			List<int[]> result = squareNumbers.Select(i => new int[] { i }).ToList(); // Add trivial squares to result
																					  //result.AddRange(Combinatorics.GetCombination(squareNumbers)); 
			Remove(squareVectors);
			Rows.Reverse(); // Reverse array

			IEnumerable<BitVector> oneSums = Rows.Where(v => v.RowSum == 1); // Get vectors with only one odd factor exponents			
			IEnumerable<IGrouping<int, BitVector>> singleFactorGroups = oneSums.GroupBy(v => v.IndexOfLeftmostElement()).Where(g => g.Count() > 1); // Group vectors by their factor exponents
			IEnumerable<BitVector> toRemove = singleFactorGroups.SelectMany(g => g.Select(v => v));

			Remove(toRemove); // Remove selected vectors from remaining vectors

			List<int[]> singleFactorResults = MatrixSolver.GetSingleFactors(this, singleFactorGroups);
			List<int[]> simpleMatchResults = MatrixSolver.GetSimpleMatches(this);
			List<int[]> chainedFactorResults = MatrixSolver.GetChainedFactors(this);
			Remove(chainedFactorResults);

			result.AddRange(singleFactorResults);
			result.AddRange(simpleMatchResults);
			result.AddRange(chainedFactorResults);

			return result;
		}

		public void Remove(IEnumerable<int[]> nextNumbers)
		{
			foreach (int[] numbers in nextNumbers)
			{
				Remove(numbers);
			}
		}

		public void Remove(IEnumerable<int> numbers)
		{
			IEnumerable<BitVector> matches = Rows.Where(v => numbers.Contains(v.Number));
			Remove(matches);
		}

		public void Remove(IEnumerable<BitVector> vectors)
		{
			Rows = Rows.Except(vectors).ToList();
		}

		private int ColumnSum(int index)
		{
			return Rows.Select(bv => bv[index] ? 1 : 0).Sum();
		}

		private void SortRows()
		{
			Rows = Rows.OrderByDescending(bv => bv.IndexOfRightmostElement())
						.ThenBy(bv => bv.IndexOfLeftmostElement())
						.ThenByDescending(bv => bv.RowSum)
						.ToList();
		}

		private bool[] GetColumn(int index)
		{
			return Rows.Select(bv => bv[index]).ToArray();
		}

		public override string ToString()
		{
			SortRows();

			int maxValue = Rows.Select(row => row.Number).Max();
			int padLength = maxValue.ToString().Length + 3;
			string padString = new string(Enumerable.Repeat(' ', padLength).ToArray());

			StringBuilder sb = new StringBuilder();
			sb.Append(padString);
			sb.Append(string.Join(",", ColumnSums));
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine(string.Join(Environment.NewLine, Rows.Select(i => i.ToString(padLength))));
			return sb.ToString();
		}
	}
}
