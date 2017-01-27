using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.Prime
{
	// TODO: Make return IEnumerable
	public static class Eratosthenes
	{
		private static List<int> longestSieve;
		private static List<bool> longestprimeMembershipArray;

		static Eratosthenes()
		{
			longestSieve = new List<int>();
			longestprimeMembershipArray = new List<bool>();
		}

		public static List<int> Sieve(int ceiling)
		{
			return Sieve(2, ceiling);
		}

		public static List<int> Sieve(int floor, int ceiling)
		{
			if (floor < 2)
			{
				floor = 2;
			}

			if (ceiling < 10)
			{
				throw new ArgumentOutOfRangeException("ceiling < 10");
			}
			if (floor > ceiling)
			{
				throw new ArgumentOutOfRangeException("floor > ceiling");
			}

				int cacheMaxValue = 0;
			if (longestSieve.Count > 0)
			{
				cacheMaxValue = longestSieve.Last();
			}

			if (cacheMaxValue >= ceiling || longestprimeMembershipArray.Count >= ceiling)
			{
				// Cached Value					
				return longestSieve.TakeWhile(l => l < ceiling).ToList();
			}

			int counter = 0;
			int counterStart = 3;
			int inc;
			int sqrt = 3;
			bool[] primeMembershipArray = new bool[ceiling + 1];

			if (longestprimeMembershipArray.Count > counterStart /*&& longestprimeMembershipArray.Length < ceiling+1*/)
			{
				Array.ConstrainedCopy(longestprimeMembershipArray.ToArray(), 0, primeMembershipArray, 0, (int)Math.Min(longestprimeMembershipArray.Count, ceiling + 1));
				//counterStart = longestprimeMembershipArray.Count - 2;
			}

			primeMembershipArray[2] = true;

			// Set all odds as true
			for (counter = counterStart; counter <= ceiling; counter += 2)
			{
				if ((counter & 1) == 1)//% 2 == 1) // Check if odd
				{
					primeMembershipArray[counter] = true;
				}
			}

			while (sqrt * sqrt <= ceiling)
			{
				counter = sqrt * sqrt;
				inc = sqrt + sqrt;

				while (counter <= ceiling)
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
			
			List<int> result = Enumerable.Range(2, (int)ceiling - 2).Select(n => (int)n).Where(l => l >= floor && primeMembershipArray[l]).ToList();

			if (result.Count > longestSieve.Count)
			{
				longestSieve = result;
			}

			if (primeMembershipArray.Length > longestprimeMembershipArray.Count)
			{
				longestprimeMembershipArray = primeMembershipArray.ToList();
			}

			return result;
		}
	}
}
