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
		public static BigInteger FindLCM(IEnumerable<BigInteger> numbers)
		{
			return FindLCM(numbers.ToArray());
		}

		public static BigInteger FindLCM(params BigInteger[] numbers)
		{
			return numbers.Aggregate(FindLCM);
		}

		public static BigInteger FindLCM(BigInteger left, BigInteger right)
		{
			BigInteger absValue1 = BigInteger.Abs(left);
			BigInteger absValue2 = BigInteger.Abs(right);
			return (absValue1 * absValue2) / FindGCD(absValue1, absValue2);
		}

		public static BigInteger FindGCD(IEnumerable<BigInteger> numbers)
		{
			return FindGCD(numbers.ToArray());
		}

		public static BigInteger FindGCD(params BigInteger[] numbers)
		{
			return numbers.Aggregate(FindGCD);
		}

		public static BigInteger FindGCD(BigInteger left, BigInteger right)
		{
			return BigInteger.GreatestCommonDivisor(left, right);
		}
	}
}
