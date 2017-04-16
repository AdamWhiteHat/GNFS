using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
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
		private BigInteger polyBase;
		private IEnumerable<BigInteger> rationalSet;
		private IEnumerable<BigInteger> algebraicSet;


		public SquareFinder(GNFS sieve)
			: this(sieve, sieve.Relations)
		{
		}

		public SquareFinder(GNFS sieve, Relation[] relations)
		{
			RationalProductMod = -1;

			gnfs = sieve;
			polyBase = gnfs.Algebraic.Base;
			RelationsSet = relations;
			SquarePolynomialDerivative = (BigInteger)(gnfs.Algebraic.FormalDerivative * gnfs.Algebraic.FormalDerivative);
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
			RationalInverseSquare = BigInteger.Multiply(RationalProduct, (BigInteger)SquarePolynomialDerivative);
			RationalInverseSquareRoot = RationalInverseSquare.SquareRoot();
			var residue = RationalInverseSquareRoot % gnfs.N;

			RationalSum = RelationsSet.Select(rel => rel.A).Sum();
			RationalNormSum = rationalSet.Sum();
			RationalProductMod = residue;
			IsRationalIrreducible = _isIrreducible(rationalSet);
			IsRationalSquare = RationalProductMod.IsSquare();

			Y2 = BigInteger.Multiply(RationalProductMod, RationalProductMod);
			Y2_S = BigInteger.Subtract(Y2, RationalProduct);

		}

		public BigInteger CalculateRationalModPrime(BigInteger prime)
		{
			BigInteger mod = AlgebraicPolynomial.Evaluate(gnfs.Algebraic, prime);
			return (RationalProduct % prime) % mod;
		}

		public void CalculateRationalModPolynomial()
		{
			// Should be the same as N
			BigInteger mod = AlgebraicPolynomial.Evaluate(gnfs.Algebraic, polyBase);

			RationalModPolynomial = RationalProduct % mod;

		}
		
		public void CalculateAlgebraicSide()
		{
			AlgebraicSide(polyBase);
		}

		private void AlgebraicSide(BigInteger prime)
		{
			AlgebraicNormSum = RelationsSet.Select(rel => rel.AlgebraicNorm).Sum();

			algebraicSet = RelationsSet.Select(rel => rel.AlgebraicNorm % prime);

			AlgebraicProduct = algebraicSet.Product();
			AlgebraicProductMod = AlgebraicProduct % prime;
			AlgebraicSum = algebraicSet.Sum();

			IsAlgebraicIrreducible = _isIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicProductMod.IsSquare();
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Square finder, rational:");
			result.AppendLine($"  √( {this.RationalProduct} * {this.SquarePolynomialDerivative} )");
			result.AppendLine($"= √( {this.RationalInverseSquare} )");
			result.AppendLine($"=    {this.RationalInverseSquareRoot}");
			result.AppendLine();
			result.AppendLine($"Product(R) = {this.RationalProduct}");
			result.AppendLine($"Product(R) mod N = γ = {this.RationalProductMod}");
			result.AppendLine($"*InverseSquare: {this.RationalInverseSquare}");
			result.AppendLine($"Sum: {this.RationalSum}");
			result.AppendLine($"SumOfNorms: {this.RationalNormSum}");
			result.AppendLine($"IsRationalSquare ? {this.IsRationalSquare}");
			result.AppendLine($"IsRationalIrreducible ? {this.IsRationalIrreducible}");
			result.AppendLine($"RationalModPolynomial: {this.RationalModPolynomial}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, algebraic:");
			result.AppendLine($"Product: {this.AlgebraicProduct}");
			result.AppendLine($"ProductMod: {this.AlgebraicProductMod}");
			result.AppendLine($"Sum: {this.AlgebraicSum}");
			result.AppendLine($"SumOfNorms: {this.AlgebraicNormSum}");
			result.AppendLine($"IsAlgebraicSquare ? {this.IsAlgebraicSquare}");
			result.AppendLine($"IsAlgebraicIrreducible ? {this.IsAlgebraicIrreducible}");
			result.AppendLine();
			result.AppendLine();

			return result.ToString();
		}
	}
}
