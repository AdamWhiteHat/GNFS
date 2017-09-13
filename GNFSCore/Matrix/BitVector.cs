using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.Matrix
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

			if (!primeFactorization.Any())
			{
				Elements = new bool[] { };
				return;
			}

			//primeFactorization = primeFactorization.Where(factor => factor.Item1 <= maxValue);

			bool[] result = new bool[PrimeFactory.GetIndexFromValue(maxValue) + 1];
			foreach (Tuple<BigInteger, BigInteger> factor in primeFactorization.Where(f => (f.Item2 % 2) == 1))
			{
				if (factor.Item1 > maxValue)
				{
					Elements = new bool[] { };
					return;
				}
				int index = PrimeFactory.GetIndexFromValue(factor.Item1);
				result[index] = true;
			}

			Elements = result;
		}

		internal BitVector(BigInteger number, bool[] elements)
		{
			Elements = elements;
			Number = number;
		}

		public bool IsSquare()
		{
			if (!Elements.Any()) { return false; }
			else { return Elements.All(e => e == false); }
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

		public static BitVector Add(BitVector left, BitVector right)
		{
			if (left.Length != right.Length)
			{
				throw new ArgumentException($"Both vectors must have the same length.");
			}

			int length = left.Length;

			bool[] result = new bool[length]; //Enumerable.Repeat(false, length).ToArray();

			int index = 0;
			while (index < length)
			{
				result[index] = left[index] ^ right[index];
				index++;
			}

			BigInteger resultNumber = BigInteger.Multiply(left.Number, right.Number);

			return new BitVector(resultNumber, result);
		}

		public int IndexOfRightmostElement()
		{
			return Array.LastIndexOf(Elements, true);
		}

		public int IndexOfLeftmostElement()
		{
			return Array.IndexOf(Elements.ToArray(), true);
		}

		public int ColumnContribution(int columnIndex)
		{
			return Elements[columnIndex] ? 1 : 0;
		}

		public static int GetWeight(BitVector vector)
		{
			int count = vector.Length;

			int left = count - vector.IndexOfLeftmostElement();
			int right = vector.IndexOfRightmostElement() + 1;
			int middle = 0;

			int nonemptyBits = vector.RowSum;

			if (nonemptyBits > 2)
			{
				middle = count - nonemptyBits;
			}

			return (left + middle + right);
		}

		public int GetWeight()
		{
			return GetWeight(this);
		}

		public int CompareTo(BitVector other)
		{
			if (other == null)
			{
				return 1;
			}

			BitVector vector = other as BitVector;
			if (vector == null)
			{
				throw new ArgumentException("BitVector is not of type BitVector");
			}

			int left = GetWeight(this);
			int right = GetWeight(vector);

			return left.CompareTo(right);
		}

		public static string FormatElements(bool[] elements)
		{
			return string.Join(",", elements.Select(b => b ? '1' : '0'));
		}

		public static string FormatElements(bool[] elements, int padLength)
		{
			return string.Join(",", elements.Select(b => b ? "1".PadLeft(padLength) : "0".PadLeft(padLength)));
		}

		public override string ToString()
		{
			//  augmented matrix style
			return $"{Number.ToString().PadLeft(13)}\t|\t{FormatElements(Elements)}";
		}

		public string ToString(int padLength)
		{
			return $"{Number.ToString().PadLeft(13)}\t|\t{FormatElements(Elements, padLength)}";
		}
	}
}
