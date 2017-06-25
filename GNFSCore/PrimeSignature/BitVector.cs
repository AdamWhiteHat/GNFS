using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.PrimeSignature
{
	using IntegerMath;
	using System.Numerics;

	public class BitVector
	{
		public BigInteger Number;
		public bool[] Elements;
		public int RowSum { get { return Elements.Count(b => b == true); } }

		public int Length { get { return Elements.Length; } }

		public bool this[int index] => Elements[index];

		public BitVector(BigInteger number, BigInteger maxValue)
			: this(number, maxValue, FactorizationFactory.GetPrimeFactorizationTuple(number, maxValue))
		{ }

		public BitVector(BigInteger number, BigInteger maxValue, IEnumerable<Tuple<BigInteger, BigInteger>> primeFactorization)
		{
			Number = number;

			bool[] result = new bool[PrimeFactory.GetIndexFromValue(maxValue) + 2];
			foreach (Tuple<BigInteger, BigInteger> factor in primeFactorization)
			{
				if (factor.Item1 > maxValue)
				{
					break;
				}
				result[PrimeFactory.GetIndexFromValue(factor.Item1) + 1] = ((factor.Item2 % 2) == 1);
			}

			Elements = result;
		}

		internal BitVector(BigInteger number, bool[] elements)
		{
			Elements = elements;
			Number = number;
		}

		public static bool[] CombineVectors(IEnumerable<BitVector> vectors)
		{
			if (vectors == null || vectors.Count() < 2 || vectors.Any(v => v == null || v.Elements == null || v.Elements.Count() < 1))
			{
				throw new ArgumentException($"Argument {nameof(vectors)} cannot be null, empty, contain null or empty vectors or contain less than two vectors.", nameof(vectors));
			}

			IEnumerable<BigInteger> vectorNumbers = vectors.Select(v => v.Number);
			IEnumerable<BigInteger> distinctVectors = vectorNumbers.Distinct();

			int distinctVectorCount = distinctVectors.Count();
			int vectorCount = vectorNumbers.Count();

			if (distinctVectorCount < vectorCount)
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

		public static string FormatElements(bool[] elements)
		{
			return string.Join(",", elements.Select(b => b ? '1' : '0'));
		}

		public override string ToString()
		{
			//  augmented matrix style
			return $"{Number} | {FormatElements(Elements)}";
		}
	}
}
