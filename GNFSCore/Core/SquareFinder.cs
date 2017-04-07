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
			RelationsSet = relations;
			SquarePolynomialDerivative = (BigInteger)(gnfs.AlgebraicPolynomial.FormalDerivative * gnfs.AlgebraicPolynomial.FormalDerivative);
		}

		public static bool IsIrreducible(IEnumerable<BigInteger> coefficients)
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
			IsRationalIrreducible = IsIrreducible(rationalSet);
			IsRationalSquare = RationalProductMod.IsSquare();

			Y2 = BigInteger.Multiply(RationalProductMod, RationalProductMod);
			Y2_S = BigInteger.Subtract(Y2, RationalProduct);

		}

		public BigInteger CalculateRationalModPrime(BigInteger prime)
		{
			BigInteger mod = Irreducible.Evaluate(gnfs.AlgebraicPolynomial, prime);
			return (RationalProduct % prime) % mod;
		}

		public void CalculateRationalModPolynomial()
		{
			// Should be the same as N
			BigInteger mod = Irreducible.Evaluate(gnfs.AlgebraicPolynomial, gnfs.AlgebraicPolynomial.Base);

			RationalModPolynomial = RationalProduct % mod;

		}

		public void CalculateAlgebraicSide()
		{
			AlgebraicSide(gnfs.AlgebraicPolynomial.Base);
		}

		private void AlgebraicSide(BigInteger prime)
		{
			algebraicSet = RelationsSet.Select(rel => rel.AlgebraicNorm/* % prime*/);

			AlgebraicProduct = algebraicSet.Product();
			AlgebraicProductMod = AlgebraicProduct % prime;
			AlgebraicSum = algebraicSet.Sum();
			AlgebraicNormSum = RelationsSet.Select(rel => rel.AlgebraicNorm).Sum();

			IsAlgebraicIrreducible = IsIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicProductMod.IsSquare();

			//Irreducible g;
			//if (IsAlgebraicIrreducible)
			//{
			//	g = new Irreducible(gnfs.N, prime, gnfs.AlgebraicPolynomial.Degree - 1);
			//	double gm = g.BaseTotal;
			//
			//	string termsString = g.Terms.FormatString();
			//	double gDerivative = g.FormalDerivative;
			//	int h = 0;
			//}
			//
			//int i = 0;
		}
	}
}
