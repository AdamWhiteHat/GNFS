using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Polynomial
{
	public static class BigIntegerArithmetic
	{
		public static IPolynomial XPowPD(BigInteger p, BigInteger d, IPolynomial modulus)
		{
			int i;

			IPolynomial mod = modulus.Clone();
			BigInteger exponent = p.Clone();
			exponent = BigInteger.Pow(exponent, mod.Degree);

			IPolynomial x = new AlgebraicPolynomial(new BigInteger[] { 0, 1 });
			IPolynomial res = new AlgebraicPolynomial(x.Terms);

			for (i = (int)Math.Round(BigInteger.Log(exponent, 2)) - 2; i >= 0; i--)
			{
				res = PolynomialArithmetic.MultiplyMod(res, res, mod, p);
				BitArray exponentBits = new BitArray(exponent.ToByteArray());
				if (exponentBits[i])
				{
					res = PolynomialArithmetic.MultiplyMod(res, x, mod, p);
				}
			}

			return res;
		}

		public static BigInteger ModInv(BigInteger a, BigInteger mod)
		{
			BigInteger ps1, ps2, dividend, divisor, rem, q, t;
			int parity;

			q = 1;
			rem = a.Clone();
			dividend = mod.Clone();
			divisor = a.Clone();
			ps1 = 1;
			ps2 = 0;
			parity = 0;

			while (divisor > 1)
			{
				rem = dividend - divisor;
				t = rem - divisor;
				if (rem >= divisor)
				{
					q += ps1; rem = t; t -= divisor;
					if (rem >= divisor)
					{
						q += ps1; rem = t; t -= divisor;
						if (rem >= divisor)
						{
							q += ps1; rem = t; t -= divisor;
							if (rem >= divisor)
							{
								q += ps1; rem = t; t -= divisor;
								if (rem >= divisor)
								{
									q += ps1; rem = t; t -= divisor;
									if (rem >= divisor)
									{
										q += ps1; rem = t; t -= divisor;
										if (rem >= divisor)
										{
											q += ps1; rem = t; t -= divisor;
											if (rem >= divisor)
											{
												q += ps1; rem = t;
												if (rem >= divisor)
												{
													q = dividend / divisor;
													rem = dividend % divisor;
													q *= ps1;
												}
											}
										}
									}
								}
							}
						}
					}
				}

				q += ps2;
				parity = ~parity;
				dividend = divisor;
				divisor = rem;
				ps2 = ps1;
				ps1 = q;
			}

			if (parity == 0)
			{
				return ps1;
			}
			else
			{
				return (mod - ps1);
			}
		}

		public static BigInteger ModMultiply(BigInteger a, BigInteger b, BigInteger mod)
		{
			return (BigInteger.Multiply(a, b) % mod);
		}

		public static BigInteger ModAdd(BigInteger a, BigInteger b, BigInteger mod)
		{
			return ModSub(a, mod - b, mod);
		}

		public static BigInteger ModSub(BigInteger a, BigInteger b, BigInteger mod)
		{
			BigInteger result = a - b;
			if (result > a)
			{
				result = result + mod;
			}
			return result;
		}
	}
}
