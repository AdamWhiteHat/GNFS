using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.SquareRoot
{
	using GNFSCore.Polynomial;
	using GNFSCore.IntegerMath;
	public static class FiniteFieldArithmetic
	{
		public static BigInteger SquareRootWithChineseRemainder(BigInteger n, List<BigInteger> values, List<BigInteger> primes)
		{
			BigInteger primeProduct = primes.Product();

			int indx = 0;
			BigInteger Z = 0;
			foreach (BigInteger pi in primes)
			{
				BigInteger Pj = primeProduct / pi;
				BigInteger Aj = ModularMultiplicativeInverse(Pj, pi); // pi-(Pj % pi);
				BigInteger Xj = values[indx];
				BigInteger AXPj = (Aj * Xj * Pj);

				//$"\nP{indx} = {pi}\nZ += (Aj * Xj * Pj)\nZ += ({Aj} * {Xj} * {Pj}) = {AXPj}".Dump();

				Z += AXPj;
				indx++;
			}

			BigInteger r = Z / primeProduct;
			BigInteger rP = r * primeProduct;
			BigInteger finalResult_sqrt = ((Z - rP) % n);

			//$"\nZ  = {Z}\n\nZp = {r}\nrP = {rP}\n\n\n( z mod N ) - ( rP mod N ) = {Z % N} - {rP % N} = {finalResult_sqrt}".Dump();

			return finalResult_sqrt;
		}

		public static Tuple<IPoly, IPoly> GetPolynomialSquareRoot(int degree, BigInteger S8, IPoly startPolynomial, BigInteger prime)
		{
			BigInteger p = prime;
			int liftingDegree = degree;
			IPoly omega = startPolynomial;
			BigInteger lambda = -1;
			BigInteger zeta = S8;//1273; // ζ
								 //BigInteger zInv = 8656; // ζ^-1

			int mm = 1, e0 = 0, e1 = 0, e2 = 0, e3 = 0;
			BigInteger temp = 1;
			BigInteger zetaProduct = 1;
			IPoly omegaProduct = null;

			do
			{
				e2 = (liftingDegree - mm);
				e1 = (liftingDegree - mm - 1);

				if (e2 < -1)
				{
					break;
				}
				else if (e2 == -1)
				{
					zeta = p - zeta;
					temp = zeta;
				}
				else
				{
					e0 = (int)BigInteger.Pow(2, e2);
					temp = BigInteger.ModPow(zeta, e0, p);
				}

				//$"λ = {lambda} * {zeta}^(2^{e2}) % {p}".Dump();
				lambda = (lambda * temp) % p;
				//$"  = {lambda}\n".Dump();

				if (lambda != -1)
				{
					if (e1 < -1)
					{
						break;
					}
					else if (e1 == -1)
					{
						zetaProduct = zeta;
					}
					else
					{
						e3 = (int)BigInteger.Pow(2, (int)e1);
						//$"{e3} = 2^{e1}".Dump();
						zetaProduct = BigInteger.ModPow(zeta, e3, p);
					}

					omegaProduct = SparsePolynomial.MultiplyMod(omega, zetaProduct, p);
				}

				//$@"ζ = {zetaProduct} = {zeta}^(2^{e1}) mod {p}
				//ω = {omegaProduct} = ({omega} * {zetaProduct}) % {p}
				//- - - - - - - - - - - - - - -".Dump();

				omega = omegaProduct;
				zeta = zetaProduct;
				mm++;
			}
			while (BigInteger.Abs(lambda) != 1 && e1 > -1);

			IPoly solution1 = omega.Clone();
			IPoly solution2 = new SparsePolynomial(PolyTerm.GetTerms(solution1.Terms.Select(trm => p - trm.CoEfficient).ToArray()));

			return new Tuple<IPoly, IPoly>(solution1, solution2);
		}

		public static Tuple<List<BigInteger>, List<BigInteger>> Sylow2Subgroup(BigInteger p, int degree)
		{
			BigInteger q = BigInteger.Pow(p, degree);
			BigInteger order = BigInteger.Pow(2, degree);
			BigInteger s = BigInteger.Divide(q - 1, order);
			BigInteger qnr = Legendre.SymbolSearch(4, q, -1);

			//$"\nq = p³ = {q}\n     S = {s}\n\n    1st quadratic non-residue > θ = {qnr}\n".Dump();

			List<BigInteger> S8 = new List<BigInteger>();
			List<BigInteger> S8Squared = new List<BigInteger>();

			int i = 0;
			while (i < order)
			{
				BigInteger Sn = BigInteger.ModPow(qnr, (i * s), p);
				BigInteger Sn2 = BigInteger.ModPow(qnr, (i * s * 2), p);
				//debug.Add($"{qnr}^({i}*{s}) ≡ {Sn} (mod {p})\n{qnr}^({i}*{s}*2) ≡ {Sn2} (mod {p})");

				S8.Add(Sn);
				S8Squared.Add(Sn2);
				i++;
			}

			//string.Join(Environment.NewLine, debug).Dump();
			return new Tuple<List<BigInteger>, List<BigInteger>>(S8, S8Squared);
		}

		// Finds X such that a*X = 1 (mod p)
		public static BigInteger ModularMultiplicativeInverse(BigInteger a, BigInteger mod)
		{
			BigInteger b = a % mod;
			for (int x = 1; x < mod; x++)
			{
				if ((b * x) % mod == 1)
				{
					return x;
				}
			}
			return 1;
		}
	}
}
