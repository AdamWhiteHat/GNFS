using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
{
	public static class BigIntegerCollectionExtensionMethods
	{
		public static BigInteger Sum(this IEnumerable<BigInteger> source)
		{
			BigInteger result = BigInteger.Zero;
			foreach (BigInteger bi in source)
			{
				result = BigInteger.Add(result, bi);
			}

			return result;
		}

		public static BigInteger Product(this IEnumerable<BigInteger> input)
		{
			BigInteger result = 1;
			foreach (BigInteger bi in input)
			{
				result = BigInteger.Multiply(result, bi);
			}
			return result;
		}

		public static BigInteger ProductMod(this IEnumerable<BigInteger> input, BigInteger modulus)
		{
			BigInteger result = 1;
			foreach (BigInteger bi in input)
			{
				result = BigInteger.Multiply(result, bi);
				if (result >= modulus || result <= -modulus)
				{
					result = result % modulus;
				}
			}
			return result;
		}
	}
}
