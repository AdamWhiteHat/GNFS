using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath.Internal
{
	public static class Eratosthenes
	{
		public static List<int> Sieve(int maxValue)
		{
			if (maxValue < 10)
			{
				if (maxValue == 9 || maxValue == 8 || maxValue == 7)
				{
					return new List<int>() { 2, 3, 5, 7 };
				}
				else if (maxValue == 6 || maxValue == 5)
				{
					return new List<int>() { 2, 3, 5 };
				}
				else if (maxValue == 4 || maxValue == 3)
				{
					return new List<int>() { 2, 3 };
				}
				else
				{
					return new List<int>() { 2 };
				}
			}

			int counter = 0;
			int counterStart = 3;
			int inc;
			int sqrt = 3;

			int ceil = maxValue > Int32.MaxValue ? Int32.MaxValue - 2 : (int)maxValue;
			bool[] primeMembershipArray = new bool[ceil + 2];

			primeMembershipArray[2] = true;

			// Set all odds as true
			for (counter = counterStart; counter <= maxValue; counter += 2)
			{
				if ((counter & 1) == 1) // Check if odd. &1 is the same as: %2
				{
					primeMembershipArray[counter] = true;
				}
			}

			while (sqrt * sqrt <= maxValue)
			{
				counter = sqrt * sqrt;
				inc = sqrt + sqrt;

				while (counter <= maxValue)
				{
					primeMembershipArray[counter] = false;
					counter += inc;
				}

				sqrt += 2;

				while (!primeMembershipArray[sqrt])
				{
					sqrt++;
				}
			}

			List<int> result = Enumerable.Range(2, (int)maxValue).Where(l => primeMembershipArray[l]).ToList();

			return result;
		}
	}
}
