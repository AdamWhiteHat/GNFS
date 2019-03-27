using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public static class StaticRandom
	{
		private static readonly Random rand = new Random();
		static StaticRandom()
		{
			int counter = rand.Next(100, 200);
			while (counter-- > 0)
			{
				rand.Next();
			}
		}

		public static int Next()
		{
			return rand.Next();
		}

		public static int Next(int maxValue)
		{
			return rand.Next(maxValue);
		}

		public static int Next(int minValue, int maxValue)
		{
			return rand.Next(minValue, maxValue);
		}

		public static double NextDouble()
		{
			return rand.NextDouble();
		}

		public static void NextBytes(byte[] bytes)
		{
			rand.NextBytes(bytes);
		}

		/// <summary>
		///  Picks a random number from a range, where such numbers will be chosen uniformly across the entire range.
		/// </summary>
		public static BigInteger NextBigInteger(BigInteger lower, BigInteger upper)
		{
			if (lower > upper) { throw new ArgumentOutOfRangeException("Upper must be greater than upper"); }

			BigInteger delta = upper - lower;
			byte[] deltaBytes = delta.ToByteArray();
			byte[] buffer = new byte[deltaBytes.Length];
			deltaBytes = null;

			BigInteger result;
			while (true)
			{
				NextBytes(buffer);

				result = new BigInteger(buffer) + lower;

				if (result >= lower && result <= upper)
				{
					buffer = null;
					return result;
				}
			}
		}
	}
}
