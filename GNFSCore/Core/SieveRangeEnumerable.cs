using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
    public static class SieveRange
    {
        public static IEnumerable<BigInteger> GetSieveRange(BigInteger maximumRange)
        {
            return GetSieveRangeContinuation(1, maximumRange);
        }

        public static IEnumerable<BigInteger> GetSieveRangeContinuation(BigInteger currentValue, BigInteger maximumRange)
        {
            BigInteger max = maximumRange;
            BigInteger counter = BigInteger.Abs(currentValue);
            bool flipFlop = !(currentValue.Sign == -1);

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
