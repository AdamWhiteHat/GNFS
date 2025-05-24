using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Algorithm.IntegerMath
{
	using ExtensionMethods;

	public static class Arithmetic
	{
		/// <summary>
		/// Exponentiation by squaring, using arbitrarily large signed integers
		/// </summary>
		public static BigInteger Pow(BigInteger @base, BigInteger exponent)
		{
			BigInteger b = BigInteger.Abs(@base);
			BigInteger exp = BigInteger.Abs(exponent);
			BigInteger result = BigInteger.One;
			while (exp > 0)
			{
				if ((exp & 1) == 1) // If exponent is odd (&1 == %2)
				{
					result = (result * b);
					exp -= 1;
					if (exp == 0) { break; }
				}

				b = (b * b);
				exp >>= 1; // exp /= 2;
			}
			return result;
		}

		/// <summary>
		/// Exponentiation by squaring, modulus some number (as needed)
		/// </summary>
		public static BigInteger PowerMod(BigInteger @base, BigInteger exponent, BigInteger modulus)
		{
			BigInteger result = BigInteger.One;
			while (exponent > 0)
			{
				if ((exponent & 1) == 1) // If exponent is odd
				{
					result = (result * @base).Mod(modulus);
					exponent -= 1;
					if (exponent == 0) { break; }
				}

				@base = (@base * @base).Mod(modulus);
				exponent >>= 1; // exponent /= 2;
			}
			return result.Mod(modulus);
		}
	}
}
