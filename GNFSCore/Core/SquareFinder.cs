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
		public BigInteger SquarePolynomialDerivative;

		public bool IsIrreducible { get { return IsRationalIrreducible && IsAlgebraicIrreducible; } }

		public bool IsRationalSquare;
		public bool IsRationalIrreducible;
		public BigInteger RationalSum;
		public BigInteger RationalNormSum;
		public BigInteger RationalProductMod;
		public BigInteger RationalInverseSquare;
		public BigInteger RationalInverseSquareRoot;
		public BigInteger RationalModPolynomial;
		public BigInteger RationalProduct;

		public bool IsAlgebraicSquare;
		public bool IsAlgebraicIrreducible;
		public BigInteger AlgebraicSum;
		public BigInteger AlgebraicNormSum;
		public BigInteger AlgebraicProductMod;
		public BigInteger AlgebraicProduct;

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
			RationalProductMod = -1;

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
			BigInteger formalDerivative = gnfs.CurrentPolynomial.Derivative(gnfs.CurrentPolynomial.Base);
			SquarePolynomialDerivative = BigInteger.Multiply(formalDerivative, formalDerivative);
		}

		private static bool _isIrreducible(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}

		//        ________________
		// y = ( √  S(m) * f'(m)^2 ) mod N
		//
		// y = 2860383 (for example)
		// 
		// 
		// S(m) mod f(x)
		// 
		// a*x^3+b*x^2+c*x^1+d*x^0

		public void CalculateRationalSide()
		{
			rationalSet = RelationsSet.Select(rel => rel.RationalNorm);
			RationalProduct = rationalSet.Product();
			RationalInverseSquare = BigInteger.Multiply(RationalProduct, SquarePolynomialDerivative);
			RationalInverseSquareRoot = RationalInverseSquare.SquareRoot();

			RationalSum = RelationsSet.Select(rel => rel.A).Sum();
			RationalNormSum = rationalSet.Sum();
			RationalProductMod = (RationalInverseSquareRoot % N);
			IsRationalIrreducible = _isIrreducible(rationalSet);
			IsRationalSquare = RationalProductMod.IsSquare();
		}

		public void CalculateAlgebraicSide()
		{
			algebraicSet = RelationsSet.Select(rel => rel.AlgebraicNorm);

			AlgebraicProduct = algebraicSet.Product();
			AlgebraicProductMod = AlgebraicProduct % N;
			AlgebraicSum = algebraicSet.Sum();

			IsAlgebraicIrreducible = _isIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicProductMod.IsSquare();
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
			BigInteger Y = RationalProductMod;
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
				BigInteger X = BigInteger.Multiply(sX, SquarePolynomialDerivative) % N;

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

			result.AppendLine("Square finder, rational:");
			result.AppendLine($"  √( {RationalProduct} * {SquarePolynomialDerivative} )");
			result.AppendLine($"= √( {RationalInverseSquare} )");
			result.AppendLine($"=    {RationalInverseSquareRoot}");
			result.AppendLine();
			//result.AppendLine($"Rational Product (δ) = {RationalProduct}");
			result.AppendLine($"γ = δ mod N = {RationalProductMod}");
			//result.AppendLine($"*InverseSquare: {RationalInverseSquare}");
			//result.AppendLine($"Sum: {RationalSum}");
			//result.AppendLine($"SumOfNorms: {RationalNormSum}");
			result.AppendLine($"IsRationalSquare ? {IsRationalSquare}");
			result.AppendLine($"IsRationalIrreducible ? {IsRationalIrreducible}");
			result.AppendLine($"RationalModPolynomial: {RationalModPolynomial}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, algebraic:");
			result.AppendLine($"Product: {AlgebraicProduct}");
			result.AppendLine($"X = s(m) * f(m) mod n = {AlgebraicProductMod}");
			result.AppendLine($"Sum: {AlgebraicSum}");
			//result.AppendLine($"SumOfNorms: {AlgebraicNormSum}");
			result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");
			result.AppendLine($"IsAlgebraicIrreducible ? {IsAlgebraicIrreducible}");
			result.AppendLine();
			result.AppendLine($"***  {(RationalInverseSquareRoot * AlgebraicProductMod) % N}");
			result.AppendLine();

			

			return result.ToString();
		}
	}
}
