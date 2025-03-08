using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.Core.Algorithm.ExtensionMethods;

namespace GNFSCore.Core.Algorithm.IntegerMath
{
	public static class Legendre
	{
		/// <summary>
		/// Legendre Symbol returns 1 for a (nonzero) quadratic residue mod p, -1 for a non-quadratic residue (non-residue), or 0 on zero.
		/// </summary>		
		public static int Symbol(BigInteger a, BigInteger p)
		{
			if (p < 2) { throw new ArgumentOutOfRangeException(nameof(p), $"Parameter '{nameof(p)}' must not be < 2, but you have supplied: {p}"); }
			if (a == 0) { return 0; }
			if (a == 1) { return 1; }

			int result;
			if (a.Mod(2) == 0)
			{
				result = Symbol(a >> 2, p); // >> right shift == /2
				if ((p * p - 1 & 8) != 0) // instead of dividing by 8, shift the mask bit
				{
					result = -result;
				}
			}
			else
			{
				result = Symbol(p.Mod(a), a);
				if (((a - 1) * (p - 1) & 4) != 0) // instead of dividing by 4, shift the mask bit
				{
					result = -result;
				}
			}
			return result;
		}

		/// <summary>
		///  Find r such that (r | m) = goal, where  (r | m) is the Legendre symbol, and m = modulus
		/// </summary>
		public static BigInteger SymbolSearch(BigInteger start, BigInteger modulus, BigInteger goal)
		{
			if (goal != -1 && goal != 0 && goal != 1)
			{
				throw new Exception($"Parameter '{nameof(goal)}' may only be -1, 0 or 1. It was {goal}.");
			}

			BigInteger counter = start;
			BigInteger max = counter + modulus + 1;
			do
			{
				if (Symbol(counter, modulus) == goal)
				{
					return counter;
				}
				counter++;
			}
			while (counter <= max);

			//return counter;
			throw new Exception("Legendre symbol matching criteria not found.");
		}
	}

}
