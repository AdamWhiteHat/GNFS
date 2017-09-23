using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.SquareRoot
{
	using Polynomial;
	using IntegerMath;

	public class SquareFinder
	{
		public Relation[] RelationsSet;

		public BigInteger PolynomialDerivative;
		public BigInteger PolynomialDerivativeSquared;
		
		public BigInteger RationalProduct;
		public BigInteger RationalSquare;
		public BigInteger RationalSquareRoot;
		public BigInteger RationalSquareRootResidue;		
		public bool IsRationalSquare;
		public bool IsRationalIrreducible;
				
		public BigInteger AlgebraicProduct;
		public BigInteger AlgebraicSquare;
		public BigInteger AlgebraicSquareResidue;
		public bool IsAlgebraicSquare;
		public bool IsAlgebraicIrreducible;

		public BigInteger Y2;
		public BigInteger Y2_S;

		private GNFS gnfs;
		private BigInteger N;
		private BigInteger polyBase;
		private Func<BigInteger, BigInteger> f;
		private IEnumerable<BigInteger> rationalSet;
		private IEnumerable<BigInteger> algebraicSet;
		private Random rand;

		public SquareFinder(GNFS sieve, List<Relation> relations)
		{
			RationalSquareRootResidue = -1;

			rand = new Random();

			int counter = 100;
			while (counter-- > 0)
			{
				rand.Next();
			}

			gnfs = sieve;
			N = gnfs.N;
			polyBase = gnfs.CurrentPolynomial.Base;
			f = (x) => gnfs.CurrentPolynomial.Evaluate(x);

			RelationsSet = relations.ToArray();
			PolynomialDerivative = gnfs.CurrentPolynomial.Derivative(gnfs.CurrentPolynomial.Base);
			PolynomialDerivativeSquared = BigInteger.Multiply(PolynomialDerivative, PolynomialDerivative);
		}

		private static bool IsIrreducible(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}

		//        ________________
		// y = ( √  S(m) * f'(m)^2 ) mod N
		//
		// y = 2860383 (for example)
		// 
		// 
		// S(x) mod f(x)
		// 
		// a*x^3+b*x^2+c*x^1+d*x^0

		public void CalculateRationalSide()
		{
			rationalSet = RelationsSet.Select(rel => rel.RationalNorm);

			RationalProduct = rationalSet.Product();
			RationalSquare = BigInteger.Multiply(RationalProduct, PolynomialDerivativeSquared);
			RationalSquareRoot = RationalSquare.SquareRoot();
			RationalSquareRootResidue = (RationalSquareRoot % N);

			IsRationalIrreducible = IsIrreducible(rationalSet);
			IsRationalSquare = RationalSquareRootResidue.IsSquare();
		}

		public void CalculateAlgebraicSide()
		{
			algebraicSet = RelationsSet.Select(rel => rel.AlgebraicNorm);

			AlgebraicProduct = algebraicSet.Product();
			AlgebraicSquare = BigInteger.Multiply(AlgebraicProduct, PolynomialDerivative);
			AlgebraicSquareResidue = AlgebraicSquare % N;

			IsAlgebraicIrreducible = IsIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();
		}

		public List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>> CalculateRootProducts()
		{
			List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>> results = new List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>>();

			IEnumerable<int> roots = gnfs.AFB.Select(fp => fp.R).Distinct();
			foreach (int root in roots)
			{
				BigInteger f = gnfs.CurrentPolynomial.Evaluate(root);

				IEnumerable<BigInteger> rootSet = RelationsSet.Select(rel => rel.Apply(root));

				BigInteger rootProduct = rootSet.Product();

				results.Add(new Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>(root, rootSet.ToArray(), rootProduct, rootProduct % f, f));
			}

			return results.OrderBy(tup => BigInteger.Abs(tup.Item4)).ThenBy(tup => tup.Item1).ThenBy(tup => tup.Item5).ToList();
		}

		private BigInteger LogExpand(int logBase, double logExponent)
		{
			int logWhole = (int)Math.Floor(logExponent);
			double logDecimal = logExponent % 1;

			BigInteger resultWhole = BigInteger.Pow(logBase, logWhole);
			double resultDecimal = Math.Pow(logBase, logDecimal);

			BigInteger result = BigInteger.Multiply(resultWhole, (int)Math.Floor(resultDecimal));

			double diffDecimal = (resultDecimal % 1);

			int resultDiff = (int)Math.Floor(1 / diffDecimal);
			BigInteger resultAdd = BigInteger.Divide(result, resultDiff);

			result += resultAdd;
			return result;
		}

		public BigInteger NewtonDirectSqrt()
		{
			// Set S, Y
			int d = gnfs.CurrentPolynomial.Degree;
			BigInteger S = RationalProduct;
			BigInteger Y = RationalSquareRootResidue;
			Y2 = BigInteger.Multiply(Y, Y);
			Y2_S = BigInteger.Subtract(Y2, S);

			// Find q,R such that (S * R^2) ≡ 1 mod q^2			
			BigInteger q = N;
			BigInteger R = BigInteger.Zero;
			bool congruenceFound = false;
			do
			{
				// Choose q		
				do
				{
					q = PrimeFactory.GetNextPrime(q * 3);
				}
				while (GCD.FindGCD(q, N) != 1);

				// Choose R
				RationalPolynomial randomPolynomial = new RationalPolynomial(N, 2, rand.Next(), N);
				BigInteger R0 = randomPolynomial.Evaluate(q); // RandomPolynomial


				// Compute (R - Y)^( q^(d-1) / 2 ) % (Y^2 - S)
				BigInteger R_Y = BigInteger.Subtract(R0, Y);
				//BigInteger qd_1_2 = BigInteger.Divide(BigInteger.Pow(q, d - 1), 2);

				double logQ = BigInteger.Log10(q);

				double logExponent = logQ * (d - 1); // Equivalent to exponentiation by d-1
				logExponent -= BigInteger.Log10(2);  // Equivalent to division by 2

				BigInteger expTotal = LogExpand(10, logExponent);
				double logBase = BigInteger.Log10(BigInteger.Abs(R_Y));

				double logTotal = logBase * (double)expTotal;
				BigInteger leftTotal = LogExpand(10, logTotal);

				BigInteger R1 = leftTotal % Y2_S;
				R = R1;

				// Check S * R1^2 % f(x) == 1
				BigInteger R1S_N = BigInteger.Multiply(BigInteger.Multiply(R1, R1), S) % N;

				congruenceFound = (R1S_N == 1);
			}
			while (!congruenceFound);

			int k = 0;
			BigInteger Rk = R;
			do
			{
				k += 1;
				Rk = NewtonIteration(Rk, S, q, k);
			}
			while (BigInteger.Pow(BigInteger.Multiply(Rk, S), 2) != S);

			BigInteger Q2 = BigInteger.Multiply(q, q);

			BigInteger sX1 = BigInteger.Multiply(S, Rk) % Q2;
			BigInteger sX2 = BigInteger.Negate(sX1) % Q2;

			BigInteger sX = sX1;

			bool last = false;
			bool escape = false;
			do
			{
				BigInteger X = BigInteger.Multiply(sX, PolynomialDerivativeSquared) % N;

				BigInteger gcd1 = GCD.FindGCD(N, BigInteger.Subtract(X, Y));
				BigInteger gcd2 = GCD.FindGCD(N, BigInteger.Subtract(Y, X));

				if (gcd1 != 1 && gcd1 != N)
				{
					return gcd1;
				}
				else if (gcd2 != 1 && gcd2 != N)
				{
					return gcd2;
				}
				else
				{
					if (!last)
					{
						sX = sX2;
						last = true;
					}
					else
					{
						escape = true;
					}
				}
			}
			while (!escape);

			return BigInteger.MinusOne;
		}

		private Func<BigInteger, int, BigInteger> _newtonModQ = (q, k) => BigInteger.Pow(q, (int)BigInteger.Pow(2, k));
		private Func<BigInteger, BigInteger, BigInteger> _newtonSqrS = (s, r) => BigInteger.Multiply(s, BigInteger.Multiply(r, r));

		private BigInteger NewtonIteration(BigInteger r, BigInteger S, BigInteger q, int k)
		{
			//  R[k](x) = R[k-1](x) * (3 - prod(x)*R[k-1](x)^2) / 2 mod (q^(2^k))

			BigInteger prod = N;

			BigInteger left =
				BigInteger.Divide(
					BigInteger.Multiply(
						S,
						BigInteger.Subtract(
							3,
							_newtonSqrS.Invoke(prod, S)
						)
					),
					2
				);

			BigInteger right = _newtonModQ.Invoke(q, k);


			BigInteger result = left % right;
			return result;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Square finder, Rational:");
			result.AppendLine($"γ² = √(  Sᵣ(m)  *  ƒ'(m)²  )");
			result.AppendLine($"γ² = √( {RationalProduct} * {PolynomialDerivativeSquared} )");
			result.AppendLine($"γ² = √( {RationalSquare} )");
			result.AppendLine($"γ  =    {RationalSquareRoot} mod N");
			result.AppendLine($"γ  =    {RationalSquareRootResidue}"); // δ mod N 
			//result.AppendLine();
			//result.AppendLine($"IsSquare(Rational) ? {IsRationalSquare}");
			//result.AppendLine($"IsIrreducible(Rational) ? {IsRationalIrreducible}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Algebraic:");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicProduct} * {PolynomialDerivative}");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicSquare}");
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareResidue}");
			//result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");
			//result.AppendLine($"IsAlgebraicIrreducible ? {IsAlgebraicIrreducible}");
			result.AppendLine();
			result.AppendLine($"***  {(RationalSquareRoot * AlgebraicSquareResidue) % N}");
			result.AppendLine();



			return result.ToString();
		}
	}
}
