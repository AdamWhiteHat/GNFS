using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.IntegerMath
{
	public static class GCD
	{
		public static BigInteger FindGCD(IEnumerable<BigInteger> numbers)
		{
			return numbers.Aggregate(FindGCD);
		}

		public static BigInteger FindGCD(BigInteger value1, BigInteger value2)
		{
			value1 = BigInteger.Abs(value1);
			value2 = BigInteger.Abs(value2);

			while (value1 != 0 && value2 != 0)
			{
				if (value1 > value2)
				{
					value1 %= value2;
				}
				else
				{
					value2 %= value1;
				}
			}
			return BigInteger.Max(value1, value2);
		}
	}
}
