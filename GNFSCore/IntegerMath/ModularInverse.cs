using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.IntegerMath
{
	public static class ModularInverse
	{
		public static int Int32(int a, int b)
		{
			int t, nt, r, nr, q, tmp;
			if (b < 0) b = -b;
			if (a < 0) a = b - (-a % b);
			t = 0; nt = 1; r = b; nr = a % b;
			while (nr != 0)
			{
				q = r / nr;
				tmp = nt; nt = t - q * nt; t = tmp;
				tmp = nr; nr = r - q * nr; r = tmp;
			}
			if (r > 1) return -1;  /* No inverse */
			if (t < 0) t += b;
			return t;
		}

		public static BigInteger BigInt(BigInteger a, BigInteger b)
		{
			if (b < 0)
			{
				b = -b;
			}

			if (a < 0)
			{
				a = b - (-a % b);
			}

			BigInteger t = 0;
			BigInteger nt = 1;
			BigInteger r = b;
			BigInteger nr = a % b;

			BigInteger q, tmp;
			while (nr != 0)
			{
				q = r / nr;
				tmp = nt; nt = t - (q * nt); t = tmp;
				tmp = nr; nr = r - (q * nr); r = tmp;
			}
			if (r > 1)
			{
				return -1;  /* No inverse */
			}
			if (t < 0)
			{
				t += b;
			}

			return t;
		}
	}
}
