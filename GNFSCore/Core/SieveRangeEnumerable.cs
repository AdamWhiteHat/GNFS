using System;
using System.Linq;
using System.Collections.Generic;

namespace GNFSCore
{
	public static class SieveRange
	{
		public static IEnumerable<int> GetSieveRange(int maximumRange)
		{
			return GetSieveRangeContinuation(1, maximumRange);
		}

		public static IEnumerable<int> GetSieveRangeContinuation(int currentValue, int maximumRange)
		{
			int max = maximumRange;
			int counter = Math.Abs(currentValue);
			bool flipFlop = !(Math.Sign(currentValue) == -1);
			
			while (counter <= max)
			{
				if (flipFlop)
				{
					yield return counter;
					flipFlop = false;
				}
				else if (!flipFlop)
				{
					yield return -counter;
					counter++;
					flipFlop = true;
				}
			}
		}		
	}
}
