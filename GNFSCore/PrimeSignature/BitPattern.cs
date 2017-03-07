using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.PrimeSignature
{
	public class BitPattern
	{
		public BitPattern()
		{
		}

		public static bool IsMatch(int[] oddParityIndicesPattern, BitVector tocheck)
		{
			if (oddParityIndicesPattern == null) return false;
			return oddParityIndicesPattern.All(i => tocheck.Elements[i] == true);
		}

		public static int[] GetPattern(BitVector a)
		{
			return Enumerable.Range(0, a.Elements.Length).Where(i => a.Elements[i] == true).ToArray();
		}

		public static BitVector FindMatch(int[] oddParityIndicesPattern, BitMatrix tocheck)
		{
			foreach (BitVector bitVector in tocheck.Rows)
			{
				if (IsMatch(oddParityIndicesPattern, bitVector))
				{
					return bitVector;
				}
			}

			return null;
		}
	}
}
