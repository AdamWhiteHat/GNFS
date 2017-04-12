using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.PrimeSignature
{
	using IntegerMath;

	public class BitVector
	{
		public int Number;
		public bool[] Elements;
		public int RowSum { get { return Elements.Count(b => b == true); } }

		public bool this[int index] => Elements[index];

		public BitVector(int number, int maxValue)
			: this(number, maxValue, FactorizationFactory.GetPrimeFactorizationTuple(number, maxValue))
		{ }

		public BitVector(int number, int maxValue, IEnumerable<Tuple<int, int>> primeFactorization)
		{
			Number = number;

			bool[] result = new bool[PrimeFactory.GetIndexFromValue(maxValue) + 1];
			foreach (Tuple<int, int> factor in primeFactorization)
			{
				if (factor.Item1 > maxValue)
				{
					break;
				}
				result[PrimeFactory.GetIndexFromValue(factor.Item1)] = ((factor.Item2 % 2) == 1);
			}

			Elements = result;
		}

		public static bool[] CombineVectors(IEnumerable<BitVector> vectors)
		{
			if (vectors == null || vectors.Count() < 2 || vectors.Any(v => v == null || v.Elements == null || v.Elements.Count() < 1))
			{
				throw new ArgumentException($"Argument {nameof(vectors)} cannot be null, empty, contain null or empty vectors or contain less than two vectors.", nameof(vectors));
			}

			IEnumerable<int> vectorNumbers = vectors.Select(v => v.Number);
			IEnumerable<int> distinctVectors = vectorNumbers.Distinct();
			if (distinctVectors.Count() < vectorNumbers.Count())
			{
				throw new ArgumentException($"Argument {nameof(vectors)} cannot contain two vectors with the same value (Number)", nameof(vectors));
			}

			IEnumerable<int> vectorWidths = vectors.Select(v => v.Elements.Count()).Distinct();
			if (vectorWidths.Count() > 1)
			{
				throw new ArgumentException($"All vectors must have the Element count.", nameof(vectors));
			}

			int width = vectorWidths.Single();
			int[] columnTotals = new int[width];

			IEnumerable<int> columnIndices = Enumerable.Range(0, width);
			foreach (int column in columnIndices)
			{
				columnTotals[column] = vectors.Count(v => v.Elements[column]); // Total each column
			}

			bool[] result = columnTotals.Select(i => i % 2 == 1).ToArray(); // Mod 2 each column total
			return result;
		}

		internal int IndexOfRightmostElement()
		{
			return Array.LastIndexOf(Elements, true);
		}

		internal int IndexOfLeftmostElement()
		{
			return Array.IndexOf(Elements, true);
		}

		public override string ToString()
		{
			int maxValue = PrimeFactory.GetValueFromIndex(Elements.Length - 1);
			string maxValueString = maxValue.ToString();
			int padLength = maxValueString.Length + 3;

			return ToString(padLength);
		}

		public string ToString(int padLength)
		{
			string numberString = $"{Number}:".PadRight(padLength);
			return numberString + string.Join(",", Elements.Select(b => b ? '1' : '0'));
		}
	}
}
