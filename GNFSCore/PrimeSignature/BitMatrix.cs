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
	using System.Numerics;

	public class BitMatrix
	{
		public int Width;
		public List<BitVector> Rows;

		public int[] RowSums { get { return Rows.Select(r => r.RowSum).ToArray(); } }
		public int[] ColumnSums { get { return Enumerable.Range(0, Width).Select(i => ColumnSum(i)).ToArray(); } }

		public BitMatrix(IEnumerable<BigInteger> array, int primeBound)
		{
			int maxArraySquareRoot = (int)Math.Ceiling(Math.Sqrt((double)array.Max())) + 1;
			int maxArrayValue = PrimeFactory.GetValueFromIndex(maxArraySquareRoot);
			int maxValue = Math.Min(maxArrayValue, primeBound);

			Width = PrimeFactory.GetIndexFromValue(maxValue) + 1;

			IEnumerable<BigInteger> distinctNonPrimeValues = array.Distinct().Where(i => !PrimeFactory.IsPrime((int)i));
			Rows = distinctNonPrimeValues.Select(i => new BitVector(i, maxValue)).ToList();

			IEnumerable<int> nonSquareColumns = Enumerable.Range(0, Width).Where(i => ColumnSum(i) == 1).ToArray();
			IEnumerable<BitVector> toRemove = nonSquareColumns.SelectMany(col => Rows.Where(r => r.Elements[col] == true));

			Remove(toRemove);

			SortRows();
		}

		public BitMatrix(IEnumerable<BitVector> vectors)
		{
			Rows = vectors.ToList();
			Width = Rows.First().Length;

			SortRows();
		}

		public void Remove(IEnumerable<BigInteger[]> nextNumbers)
		{
			foreach (BigInteger[] numbers in nextNumbers)
			{
				Remove(numbers);
			}
		}

		public void Remove(IEnumerable<BigInteger> numbers)
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

			//BigInteger maxValue = Rows.Select(row => row.Number).Max();
			//int padLength = maxValue.ToString().Length + 3;
			//string padString = new string(Enumerable.Repeat(' ', padLength).ToArray());

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Join(Environment.NewLine, Rows.Select(i => i.ToString())));
			sb.AppendLine();
			sb.Append(string.Join(",", ColumnSums));
			return sb.ToString();
		}
	}
}
