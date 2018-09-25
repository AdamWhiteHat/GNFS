using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.SquareRoot
{
	public static class ChineseRemainderTheorem
	{
		public static BigInteger Solve(BigInteger[] n, BigInteger[] a)
		{
			BigInteger prod = n.Aggregate(BigInteger.One, (i, j) => i * j);
			BigInteger p;
			BigInteger sm = 0;
			for (int i = 0; i < n.Length; i++)
			{
				p = prod / n[i];
				sm += a[i] * ModularMultiplicativeInverse(p, n[i]) * p;
			}
			return sm % prod;
		}

		/// <summary>
		/// Finds X such that a*X = 1 (mod m)
		/// </summary>
		public static BigInteger ModularMultiplicativeInverse(BigInteger a, BigInteger m)
		{
			BigInteger b = a % m;
			for (int x = 1; x < m; x++)
			{
				if ((b * x) % m == 1)
				{
					return x;
				}
			}
			return 1;
		}
	}
}
