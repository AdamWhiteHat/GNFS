using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath
{
	public static class BigRange
	{
		public static IEnumerable<BigInteger> GetRange(BigInteger min, BigInteger count)
		{
			return GetRange(min, count, 1);
		}

		public static IEnumerable<BigInteger> GetRange(BigInteger min, BigInteger count, BigInteger jump)
		{
			BigInteger counter = count;
			BigInteger currentValue = min;
			while (counter-- > 0)
			{
				yield return currentValue;
				currentValue += jump;
			}
			yield break;
		}
	}
}
