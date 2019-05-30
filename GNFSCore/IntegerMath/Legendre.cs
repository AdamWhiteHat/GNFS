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
		/// <summary>
		/// Legendre Symbol returns 1 for a (nonzero) quadratic residue mod p, -1 for a non-quadratic residue (non-residue), or 0 on zero.
		/// </summary>		
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

		/// <summary>
		///  Find r such that (r | m) = goal, where  (r | m) is the Legendre symbol
		/// </summary>
		public static BigInteger SymbolSearch(BigInteger start, BigInteger modulus, BigInteger goal)
		{
			if (goal != -1 && goal != 0 && goal != 1)
			{
				throw new Exception($"Parameter '{nameof(goal)}' may only be -1, 0 or 1. It was {goal}.");
			}

			BigInteger counter = start;
			do
			{
				if (Symbol(counter, modulus) == goal)
				{
					break;
				}
				counter++;
			}
			while (true);

			return counter;
		}
	}

}
