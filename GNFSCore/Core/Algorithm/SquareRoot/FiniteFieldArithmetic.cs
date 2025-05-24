using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Core.Algorithm.SquareRoot
{
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Algorithm.IntegerMath;

	public static class FiniteFieldArithmetic
	{
		/// <summary>
		/// Tonelli-Shanks algorithm for finding polynomial modular square roots
		/// </summary>
		/// <returns></returns>
		public static Polynomial SquareRoot(Polynomial startPolynomial, Polynomial f, BigInteger p, int degree, BigInteger m)
		{
			BigInteger q = BigInteger.Pow(p, degree);
			BigInteger s = q - 1;

			int r = 0;
			while (s.Mod(2) == 0)
			{
				s /= 2;
				r++;
			}

			Logging.WriteLine($"r     = {r}");
			Logging.WriteLine($"q-1   = {q - 1}");
			Logging.WriteLine($"s     = {s}");

			BigInteger halfS = (s + 1) / 2;
			if (r == 1 && q.Mod(4) == 3)
			{
				halfS = (q + 1) / 4;
				Logging.WriteLine($"(q  + 1)/4 = {halfS}");
			}
			else
			{
				Logging.WriteLine($"(s + 1) / 2 = {halfS}");
			}

			BigInteger quadraticNonResidue = Legendre.SymbolSearch(m + 1, q, -1);
			BigInteger theta = quadraticNonResidue;
			BigInteger minusOne = BigInteger.ModPow(theta, (q - 1) / 2, p);

			Logging.EnableLogging(true);
			Logging.WriteLine($"P = {p}");
			Logging.WriteLine($"θ = {theta}");
			Logging.WriteLine();

			//Logging.WriteLine($"(θp + 1)^s = {BigInteger.ModPow(theta, s, p)}");
			//Logging.WriteLine($"(θp + 1)^((p^d-1)/2) = {minusOne}");
			Logging.WriteLine($"Quadratic Non-Residue Found = {theta}");
			Logging.WriteLine($"p ≡ {p.Mod(4)} (mod 4) ≡ {p.Mod(8)} (mod 8)    L(q-1, p) = {Legendre.Symbol((q - 1), p)}");
			Logging.WriteLine($"s ≡ {s.Mod(4)} (mod 4) ≡ {s.Mod(8)} (mod 8)    L(s,   p) = {Legendre.Symbol(s, p)}");
			Logging.WriteLine();

			Polynomial omegaPoly = Polynomial.Field.ExponentiateMod(startPolynomial, halfS, f, p);
			Logging.WriteLine($"ω₀    = {omegaPoly}");
			//if(omega[r - 1].Sign == -1) { omega = Polynomial.ModularInverse(omega, p); }

			//Polynomial zetaPoly ;
			//Polynomial zetaSquaredPoly ;
			//Polynomial zetaInversePoly ;        
			//Logging.WriteLine($"ζ   = {zetaPoly} ≡ {zeta}");
			//Logging.WriteLine($"ζ²  = {zetaSquaredPoly} ≡ {zetaSquared}");
			//Logging.WriteLine($"ζ⁻² = {zetaInversePoly} ≡ {zetaInverse}");


			BigInteger lambda = minusOne;
			BigInteger zeta = 0;
			BigInteger zetaSquared = 0;
			BigInteger zetaInverse = 0;

			string j = "";
			int i = 0;
			do
			{
				i++;
				Logging.WriteLine();
				j = Logging.GetSubscript(i);

				zeta = BigInteger.ModPow(theta, i * s, p);
				Logging.WriteLine($"{j}ζ   = {zeta}");

				zetaSquared = zeta.Square().Mod(p);
				//BigInteger.ModPow(theta, (2 * i * s), p); 
				// BigInteger.ModPow(quadraticNonResidue, (i * s * 2), p);
				Logging.WriteLine($"{j}ζ²  = {zetaSquared}");

				zetaInverse = (p - zetaSquared);
				Logging.WriteLine($"{j}ζ⁻² = {zetaInverse}");
				Logging.WriteLine();

				lambda = (lambda * BigInteger.Pow(zeta, (int)Math.Pow(2, r - i))).Mod(p);
				Logging.WriteLine($"λ{j}   = {lambda}");

				BigInteger zPow = BigInteger.Pow(zeta, (int)Math.Pow(2, ((r - i) - 1)));
				Polynomial zetaPow = Polynomial.Parse($"{zPow}");

				omegaPoly = Polynomial.Field.Modulus(Polynomial.Multiply(omegaPoly, zetaPow), p);

				Logging.WriteLine($"ω{j}   = ω{Logging.GetSubscript(i - 1)} * ζ^(2^{r} - {i} - 1)");
				Logging.WriteLine($"ω{j}   = ω{Logging.GetSubscript(i - 1)} * ζ^({BigInteger.Pow(2, r) - i - 1})");
				Logging.WriteLine($"ω{j}   = {omegaPoly}");
				Logging.WriteLine($"ω{j}   ≡ {omegaPoly.Evaluate(m).Mod(p)} (mod p)");
				Logging.WriteLine();
				Polynomial zI = Polynomial.Parse($"{zetaInverse}");

				Polynomial nu = Polynomial.Field.Modulus(Polynomial.Multiply(omegaPoly, zI), p);
				Logging.WriteLine($"ν{j}   = ω{j} * ζ⁻²");
				Logging.WriteLine($"ν{j}   = {nu} ≡ {nu.Evaluate(m).Mod(p)} (mod p)");
				Logging.WriteLine();
			}
			while (!(lambda == 1 || i > r));

			return omegaPoly;
		}

		/// <summary>
		/// Finds X such that a*X = 1 (mod p)
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="mod">The modulus</param>
		/// <returns></returns>
		public static BigInteger ModularMultiplicativeInverse(BigInteger a, BigInteger mod)
		{
			if (mod == 1)
			{
				return 0;
			}

			BigInteger b = a.Mod(mod);
			for (int x = 1; x < mod; x++)
			{
				if ((b * x).Mod(mod) == 1)
				{
					return x;
				}
			}
			return 1;
		}

		/// <summary>
		/// Finds N such that primes[i] ≡ values[i] (mod N) for all values[i] with 0 &lt; i &lt; a.Length
		/// </summary>
		public static BigInteger ChineseRemainder(BigInteger n, List<BigInteger> primes, List<BigInteger> values)
		{
			BigInteger primeProduct = primes.Product();

			int indx = 0;
			BigInteger Z = 0;
			foreach (BigInteger pi in primes)
			{
				BigInteger Pj = primeProduct / pi;
				BigInteger Aj = ModularMultiplicativeInverse(Pj, pi);
				BigInteger Xj = values[indx];
				BigInteger AXPj = (Aj * Xj * Pj);

				Logging.WriteLine();
				Logging.WriteLine($"P{Logging.GetSubscript(indx)} = {pi}");
				Logging.WriteLine($"Z += (Aj * Xj * Pj)");
				Logging.WriteLine($"Z += ({Aj} * {Xj} * {Pj}) = {AXPj}");

				Z += AXPj;
				indx++;
			}

			BigInteger r = Z / primeProduct;
			BigInteger rP = r * primeProduct;
			BigInteger finalResult_sqrt = (Z - rP).Mod(n);

			Logging.WriteLine();
			Logging.WriteLine($"Z  = {Z}");
			Logging.WriteLine();
			Logging.WriteLine($"Zp = {r}");
			Logging.WriteLine($"rP = {rP}");
			Logging.WriteLine();
			Logging.WriteLine();
			Logging.WriteLine($"( z mod N ) - ( rP mod N ) = {Z.Mod(n)} - {rP.Mod(n)} = {finalResult_sqrt}");

			return finalResult_sqrt;
		}

		/// <summary>
		/// Reduce a polynomial by a modulus polynomial and modulus integer.
		/// </summary>
		public static Polynomial ModMod(Polynomial toReduce, Polynomial modPoly, BigInteger primeModulus)
		{
			int compare = modPoly.CompareTo(toReduce);
			if (compare > 0)
			{
				return toReduce;
			}
			if (compare == 0)
			{
				return Polynomial.Zero;
			}

			return Remainder(toReduce, modPoly, primeModulus);
		}

		public static Polynomial Remainder(Polynomial left, Polynomial right, BigInteger mod)
		{
			if (left == null)
			{
				throw new ArgumentNullException("left");
			}
			if (right == null)
			{
				throw new ArgumentNullException("right");
			}
			if (right.Degree > left.Degree || right.CompareTo(left) == 1)
			{
				return Polynomial.Zero.Clone();
			}

			int rightDegree = right.Degree;
			int quotientDegree = left.Degree - rightDegree + 1;

			BigInteger leadingCoefficent = right[rightDegree].Mod(mod);
			if (leadingCoefficent != 1) { throw new ArgumentNullException("right", "This method was expecting only monomials (leading coefficient is 1) for the right-hand-side polynomial."); }

			Polynomial rem = left.Clone();
			BigInteger quot = 0;

			for (int i = quotientDegree - 1; i >= 0; i--)
			{
				quot = BigInteger.Remainder(rem[rightDegree + i], mod);//.Mod(mod);

				rem[rightDegree + i] = 0;

				for (int j = rightDegree + i - 1; j >= i; j--)
				{
					rem[j] = BigInteger.Subtract(
													rem[j],
													BigInteger.Multiply(quot, right[j - i]).Mod(mod)
												).Mod(mod);
				}
			}

			return new Polynomial(rem.Terms);
		}
	}
}
