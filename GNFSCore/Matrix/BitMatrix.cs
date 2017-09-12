using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GNFSCore.Matrix
{
	using IntegerMath;
	using System.Numerics;


	public enum SortByProperty
	{
		RowSum,
		Weight,
		Leftmost,
		Rightmost,
		Combined
	}

	public class BitMatrix
	{

		public List<BitVector> Rows;

		public int Width { get { return Rows.Any() ? Rows.First().Length : 0; } }
		public int[] RowSums { get { return Rows.Select(r => r.RowSum).ToArray(); } }
		public int[] ColumnSums { get { return Enumerable.Range(0, Width).Select(i => ColumnSum(i)).ToArray(); } }
		public bool[] GetColumnsMod2() { return ColumnSums.Select(i => (i % 2) == 1).ToArray(); }

		public BitMatrix(IEnumerable<BigInteger> array, int primeBound)
		{
			int maxArraySquareRoot = (int)(array.Max().SquareRoot() + 1);
			BigInteger maxArrayValue = PrimeFactory.GetValueFromIndex(maxArraySquareRoot);
			BigInteger maxValue = BigInteger.Min(maxArrayValue, primeBound);

			IEnumerable<BigInteger> distinctNonPrimeValues = array.Distinct().Where(i => !PrimeFactory.IsPrime((int)i));
			Rows = distinctNonPrimeValues.Select(i => new BitVector(i, maxValue)).Where(bv => bv.Length != 0).ToList();

			//IEnumerable<int> nonSquareColumns = Enumerable.Range(0, Width).Where(i => ColumnSum(i) == 1).ToArray();
			//IEnumerable<BitVector> toRemove = nonSquareColumns.SelectMany(col => Rows.Where(r => r.Elements[col] == true));
			//Remove(toRemove);

			Sort();
		}

		public BitMatrix(IEnumerable<BitVector> vectors)
		{
			Rows = vectors.ToList();

			Sort();
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

		internal int ColumnSum(int columnIndex)
		{
			return Rows.Select(bv => bv[columnIndex] ? 1 : 0).Sum();
		}

		public void Sort()
		{
			Sort(SortByProperty.Combined);
		}

		public void Sort(SortByProperty sortProperty, bool descending = false)
		{
			if (!Rows.Any()) { return; }
			Func<Func<BitVector, int>, IOrderedEnumerable<BitVector>> orderByDelegate = null;

			if (descending)
			{
				orderByDelegate = Rows.OrderByDescending;
			}
			else
			{
				orderByDelegate = Rows.OrderBy;
			}

			switch (sortProperty)
			{
				case SortByProperty.Leftmost:
					Rows = orderByDelegate(vect => vect.IndexOfLeftmostElement()).ToList();
					break;
				case SortByProperty.Rightmost:
					Rows = orderByDelegate(vect => vect.IndexOfRightmostElement()).ToList();
					break;
				case SortByProperty.RowSum:
					Rows = orderByDelegate(vect => vect.RowSum).ToList();
					break;
				case SortByProperty.Weight:
					Rows = orderByDelegate(vect => (BitVector.GetWeight(vect))).ToList();
					break;
				case SortByProperty.Combined:
					Rows = orderByDelegate(bv => (bv.IndexOfRightmostElement() - bv.IndexOfLeftmostElement()) / (bv.RowSum + 1))
						.ThenByDescending(bv => bv.RowSum)
						.ThenBy(bv => bv.IndexOfRightmostElement() - bv.IndexOfLeftmostElement())
						.ToList();



					//(bv => bv.IndexOfLeftmostElement())


					break;
			}
		}

		public bool[] GetColumn(int columnIndex)
		{
			return Rows.Select(bv => bv[columnIndex]).ToArray();
		}

		//public BitMatrix GetTransposeMatrix()
		//{

		//	BigInteger[] numbers = PrimeFactory.GetPrimesTo(PrimeFactory.GetValueFromIndex(Width + 1)).ToArray();//Rows.Select(row => row.Number).ToArray();
		//	int columns = Rows.First().Length;

		//	int index = 0;
		//	List<BitVector> vectors = new List<BitVector>();
		//	while (index < columns)
		//	{
		//		bool[] vectorElements = GetColumn(index);
		//		BitVector vector = new BitVector(numbers[index], vectorElements);
		//		vectors.Add(vector);
		//		index++;
		//	}

		//	return new BitMatrix(vectors);
		//}

		public override string ToString()
		{
			Sort();

			//BigInteger maxValue = Rows.Select(row => row.Number).Max();
			//int numberLength = maxValue.ToString().Length + 3;
			//string padString = new string(Enumerable.Repeat(' ', numberLength).ToArray());
			//int vectorLength = Rows.First().ToString().Length;
			//int padLength = numberLength + vectorLength + 4;

			string result = string.Join(Environment.NewLine, Rows.Select(i => i.ToString())) + Environment.NewLine;
			result += BitVector.FormatElements(GetColumnsMod2()) + Environment.NewLine;
			return result;
		}
	}
}
