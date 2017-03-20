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
			: this(number, FactorizationFactory.GetPrimeFactorizationTuple(number, width))
		{ }

		public BitVector(int number, IEnumerable<Tuple<int, int>> primeFactorization)
		{
			Number = number;

			int width = primeFactorization.Count() - 1;
			bool[] Elements = new bool[width];

			foreach (Tuple<int, int> factor in primeFactorization)
			{
				if (factor.Item1 > width)
				{
					break;
				}
				Elements[factor.Item1] = ((factor.Item2 % 2) == 1);
			}
		}

		public bool[] CombineVectors(BitVector vector)
		{
			if (vector == null || vector.Elements == null || vector.Elements.Count() < 1)
			{
				throw new ArgumentException(nameof(vector)); // 
			}
			if (this.Number == vector.Number)
			{
				// The same numbers are of course going to match, so return a result here that will evaluate as a non-match.
				return new bool[] { true }; // Or return an empty array and calling function checks return value by array.Any()
			}

			bool[] longArray;
			bool[] shortArray;

			// In most cases, the arrays will be the same length. For robustness, we handle arrays of different lengths.
			if (this.Elements.Count() > vector.Elements.Count())
			{
				longArray = this.Elements;
				shortArray = vector.Elements;
			}
			else
			{
				longArray = vector.Elements;
				shortArray = this.Elements;
			}

			//int size = longArray.Count() - 1; // Or Math.Max(this.Elements.Count() - 1, vector.Elements.Count() - 1);
			bool[] result = longArray.ToArray(); // Make a copy

			int index = 0;
			bool element2;
			// Only iterate through the shorter array, any left over elements in the longer array have already been copied over to the result buffer
			foreach (bool element1 in shortArray)
			{
				element2 = longArray[index];

				// Just XOR the two elements. This one instruction should be faster than a conditional check and then an assignment statement
				// TODO: Inspect IL code to check above complexity assumption
				result[index] = element1 ^ element2;

				index++;
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
			return $"{string.Join(",", Elements.Select(b => b ? '1' : '0'))}";
		}
	}
}
