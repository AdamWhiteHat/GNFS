using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.IntegerMath
{
	public class CoPrime
	{
		public static bool IsCoprime(BigInteger a, BigInteger b)
		{
			return (GCD.FindGCD(a, b) == 1);
		}
	}
}
