using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.PrimeSignature
{
	public class BitPattern
	{
		private BitPattern()
		{
		}

		public static bool IsPartialMatch(BitVector vector, int[] searchPattern)
		{
			if (searchPattern == null || searchPattern.Length < 1 || vector == null || vector.Elements.Length < 1) return false;
			return searchPattern.All(i => vector.Elements[i] == true);
		}

		public static bool IsExactMatch(BitVector vector, int[] searchPattern)
		{
			if (searchPattern == null || searchPattern.Length < 1 || vector == null || vector.Elements.Length < 1) return false;

			int[] toCheckPattern = GetPattern(vector);
			return searchPattern.SequenceEqual(toCheckPattern);
		}

		public static int[] GetPattern(BitVector vector)
		{
			return GetPattern(vector.Elements);
		}

		public static int[] GetPattern(bool[] elements)
		{
			return Enumerable.Range(0, elements.Length).Where(i => elements[i] == true).ToArray();
		}

		public static BitVector FindBestPartialMatch(int[] searchPattern, IEnumerable<BitVector> vectors)
		{
			List<int> pattern = searchPattern.OrderBy(i => i).ToList();
			IEnumerable<BitVector> partialMatches = new BitVector[] { };

			while (partialMatches.Count() < 1 && pattern.Count > 0)
			{
				partialMatches = FindPartialMatches(pattern.ToArray(), vectors).OrderBy(v => v.RowSum);

				if (partialMatches.Count() < 1)
				{
					pattern.Remove(pattern.Min());
				}
			}

			return partialMatches.FirstOrDefault();
		}

		public static IEnumerable<BitVector> FindPartialMatches(int[] searchPattern, IEnumerable<BitVector> vectors)
		{
			return vectors.Where(v => IsPartialMatch(v, searchPattern));
		}

		public static IEnumerable<BitVector> FindExactMatches(int[] searchPattern, IEnumerable<BitVector> vectors)
		{
			return vectors.Where(v => IsExactMatch(v, searchPattern));
		}
	}
}
