using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.LinearAlgebra
{
	using IntegerMath;

	public class BitVector
	{
		public int Number;
		public bool[] Columns;

		public bool this[int index] => Columns[index];

		public BitVector(int number, int width)
		{
			Number = number;
			Columns = GetVector(number, width);
		}

		public bool IsMatch(int[] pattern)
		{
			if (pattern == null) return false;
			return pattern.All(i => Columns[i] == true);
		}

		public int[] GetPattern()
		{
			return Enumerable.Range(0, Columns.Length).Where(i => Columns[i] == true).ToArray();
		}

		private static bool[] GetVector(int number, int width)
		{
			bool[] result = new bool[width];

			IEnumerable<Tuple<int, int>> factorization = FactorizationFactory.GetPrimeFactorizationTuple(number, width);
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
			return Array.LastIndexOf(Columns, true);
		}

		internal int IndexOfLeftmostElement()
		{
			return Array.IndexOf(Columns, true);
		}

		internal int FactorCount()
		{
			return Columns.Count(b => b == true);
		}

		public override string ToString()
		{
			return $"{string.Join(",", Columns.Select(b => b ? "1" : "0"))}";
		}
	}
}
