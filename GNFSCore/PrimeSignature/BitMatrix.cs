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
		public BitVector[] Rows;

		public int[] RowSums { get { return Enumerable.Range(0, Rows.Length).Select(i => RowSum(i)).ToArray(); } }
		public int[] ColumnSums { get { return Enumerable.Range(0, Width).Select(i => ColumnSum(i)).ToArray(); } }

		public BitMatrix(IEnumerable<int> array, int maxValue)
		{
			Width = PrimeFactory.GetIndexFromValue(maxValue);

			IEnumerable<int> distinctNonPrimeValues = array.Select(i => Math.Abs(i)).Distinct().Where(i => i > 1 && !PrimeFactory.IsPrime(i));
			Rows = distinctNonPrimeValues.Select(i => new BitVector(i, maxValue)).ToArray();

			IEnumerable<int> nonSquareColumsn = Enumerable.Range(0, Width).Where(i => ColumnSum(i) == 1).ToArray();
			IEnumerable<BitVector> toRemove = nonSquareColumsn.SelectMany(col => Rows.Where(r => r.Elements[col] == true));

			Rows = Rows.Except(toRemove).ToArray();

			SortRows();
		}

		private int ColumnSum(int index)
		{
			return Rows.Select(bv => bv[index] ? 1 : 0).Sum();
		}

		private int RowSum(int index)
		{
			int result = Rows[index].FactorCount();
			return result;
		}

		private void SortRows()
		{
			Rows = Rows.OrderByDescending(bv => bv.IndexOfRightmostElement())
						.ThenBy(bv => bv.IndexOfLeftmostElement())
						.ThenByDescending(bv => bv.FactorCount())
						.ToArray();
		}

		private bool[] GetColumn(int index)
		{
			return Rows.Select(bv => bv[index]).ToArray();
		}

		public override string ToString()
		{
			SortRows();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Join(",", ColumnSums));
			sb.AppendLine();
			sb.AppendLine(string.Join(Environment.NewLine, Rows.Select(i => i.ToString())));
			return sb.ToString();
		}
	}
}
