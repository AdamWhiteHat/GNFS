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

		public bool this[int index] => Elements[index];

		public BitVector(int number, int width)
		{
			Number = number;
			Elements = BuildExponentVector(number, width);
		}

		
		private static bool[] BuildExponentVector(int number, int width)
		{
			bool[] result = new bool[width];

			IEnumerable<Tuple<int, int>> factorization = Factorization.GetPrimeFactorizationTuple(number, width);
			foreach (Tuple<int, int> factor in factorization)
			{
				if (factor.Item1 > width)
				{
					return result;
				}
				result[factor.Item1] = ((factor.Item2 % 2) == 1);
			}

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

		internal int FactorCount()
		{
			return Elements.Count(b => b == true);
		}

		public override string ToString()
		{
			return $"{string.Join(",", Elements.Select(b => b ? "1" : "0"))}";
		}
	}
}
