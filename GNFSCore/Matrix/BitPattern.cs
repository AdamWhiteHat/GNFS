using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Matrix
{
	public class BitPattern
	{
		internal int[] Pattern;

		public BitPattern(BitVector vector) : this(vector.Elements) { }

		public BitPattern(bool[] elements) : this(GetBitPositions(elements)) { }

		public BitPattern(int[] bitPositions)
		{
			if (bitPositions == null) { throw new ArgumentNullException(); }

			Pattern = bitPositions;

			if (Pattern.Any())
			{
				Pattern = bitPositions.OrderBy(i => i).ToArray();
			}
		}

		public BitVector FindBestPartialMatch(BitMatrix matrix)
		{
			return FindBestPartialMatch(matrix.Rows);
		}

		public IEnumerable<BitVector> FindPartialMatches(BitMatrix matrix)
		{
			return FindPartialMatches(matrix.Rows);
		}

		public IEnumerable<BitVector> FindExactMatches(BitMatrix matrix)
		{
			return FindExactMatches(matrix.Rows);
		}

		public BitVector FindBestPartialMatch(IEnumerable<BitVector> vectors)
		{
			List<int> pattern = Pattern.ToList();
			IEnumerable<BitVector> partialMatches = new BitVector[0];

			while (partialMatches.Count() < 1 && pattern.Count > 0)
			{
				partialMatches = vectors.Where(vector => IsPartialMatch(pattern.ToArray(), vector)).OrderBy(v => v.RowSum);

				if (!partialMatches.Any())
				{
					pattern.Remove(pattern.Min());
				}
			}

			return partialMatches.FirstOrDefault();
		}

		public IEnumerable<BitVector> FindPartialMatches(IEnumerable<BitVector> vectors)
		{
			return vectors.Where(v => IsPartialMatch(v));
		}

		public IEnumerable<BitVector> FindExactMatches(IEnumerable<BitVector> vectors)
		{
			return vectors.Where(v => IsExactMatch(v));
		}

		public bool IsPartialMatch(BitVector vector)
		{
			if (vector == null || vector.Elements.Length < 1) { throw new ArgumentException(); }
			return IsPartialMatch(Pattern, vector);
		}

		public bool IsExactMatch(BitVector vector)
		{
			if (vector == null || vector.Elements.Length < 1) { throw new ArgumentException(); }
			return IsExactMatch(Pattern, vector);
		}

		#region Private Methods

		private static bool IsPartialMatch(int[] bitPositions, BitVector vector)
		{
			if (vector == null || vector.Elements.Length < 1) return false;
			return bitPositions.All(i => vector.Elements[i] == true);
		}

		private static bool IsExactMatch(int[] bitPositions, BitVector vector)
		{
			if (vector == null || vector.Elements.Length < 1) return false;

			int[] toCheckPattern = GetBitPositions(vector.Elements);
			return bitPositions.SequenceEqual(toCheckPattern);
		}

		private static int[] GetBitPositions(bool[] elements)
		{
			if (elements == null || elements.Length < 2) { throw new ArgumentException(); }

			return Enumerable.Range(0, elements.Length).Where(i => elements[i] == true).ToArray();
		}

		#endregion
	}

	public static class BitPatternExtensionMethods
	{
		public static BitVector FindBestPartialMatch(this BitMatrix matrix, BitPattern pattern)
		{
			return pattern.FindBestPartialMatch(matrix.Rows);
		}

		public static IEnumerable<BitVector> FindPartialMatches(this BitMatrix matrix, BitPattern pattern)
		{
			return pattern.FindPartialMatches(matrix.Rows);
		}

		public static IEnumerable<BitVector> FindExactMatches(this BitMatrix matrix, BitPattern pattern)
		{
			return pattern.FindExactMatches(matrix.Rows);
		}
	}
}
