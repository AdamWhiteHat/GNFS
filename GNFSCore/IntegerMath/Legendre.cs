using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath
{
	public static class Legendre
	{
		public static int Symbol(BigInteger a, BigInteger p)
		{
			if (p < 2) throw new ArgumentOutOfRangeException("p", "p must not be < 2");
			if (a == 0) return 0;
			if (a == 1) return 1;

			int result;
			if (a % 2 == 0)
			{
				result = Symbol(a / 2, p);
				if (((p * p - 1) & 8) != 0) // instead of dividing by 8, shift the mask bit
				{
					result = -result;
				}
			}
			else
			{
				result = Symbol(p % a, a);
				if (((a - 1) * (p - 1) & 4) != 0) // instead of dividing by 4, shift the mask bit
				{
					result = -result;
				}
			}
			return result;
		}
	}
}
