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
		public static List<BigInteger> Sieve(BigInteger maxValue)
		{
			if (maxValue < 10)
			{
				if (maxValue == 9 || maxValue == 8 || maxValue == 7)
				{
					return new List<BigInteger>() { 2, 3, 5, 7 };
				}
				else if (maxValue == 6 || maxValue == 5)
				{
					return new List<BigInteger>() { 2, 3, 5 };
				}
				else if (maxValue == 4 || maxValue == 3)
				{
					return new List<BigInteger>() { 2, 3 };
				}
				else
				{
					return new List<BigInteger>() { 2 };
				}
			}

			Int64 counter = 0;
			Int64 counterStart = 3;
			Int64 inc;
			Int64 sqrt = 3;

			Int64 ceil = maxValue >= Int64.MaxValue ? Int64.MaxValue - 2 : (Int64)maxValue;

			Array primeMembershipArray = Array.CreateInstance(typeof(bool), ceil);
			primeMembershipArray.SetValue(true, 2);

			// Set all odds as true
			for (counter = counterStart; counter <= maxValue; counter += 2)
			{
				if ((counter & 1) == 1) // Check if odd. &1 is the same as: %2
				{
					primeMembershipArray.SetValue(true, counter);
				}
			}

			while (sqrt * sqrt <= maxValue)
			{
				counter = sqrt * sqrt;
				inc = sqrt + sqrt;

				while (counter <= maxValue)
				{
					primeMembershipArray.SetValue(false, counter);
					counter += inc;
				}

				sqrt += 2;

				while (!(bool)primeMembershipArray.GetValue(sqrt))
				{
					sqrt++;
				}
			}

			List<BigInteger> result = BigRange.GetRange(2, maxValue).Where(l => (bool)primeMembershipArray.GetValue((Int64)l)).ToList();
			return result;
		}
	}
}
