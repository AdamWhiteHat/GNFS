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

		public bool IsRationalSquare;
		public bool IsRationalIrreducible;
		public BigInteger RationalSquareRoot;

		public bool IsAlgebraicSquare;
		public bool IsAlgebraicIrreducible;
		public BigInteger AlgebraicSquareRoot;

		public GNFS gnfs;
		public BigInteger SquarePolynomialDerivative;

		public IEnumerable<BigInteger> rationalSet;
		public BigInteger rationalSetProduct;
		public BigInteger rationalInverseSquare;
		public BigInteger rationalInverseSquareRoot;

		public IEnumerable<BigInteger> algebraicSet;
		public BigInteger algebraicSetProduct;

		public SquareFinder(GNFS sieve)
			: this(sieve, sieve.Relations)
		{
		}

		public SquareFinder(GNFS sieve, Relation[] relations)
		{
			RationalSquareRoot = -1;

			gnfs = sieve;
			RelationsSet = relations;
			SquarePolynomialDerivative = (BigInteger)(gnfs.AlgebraicPolynomial.FormalDerivative * gnfs.AlgebraicPolynomial.FormalDerivative);
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
			IsRationalIrreducible = IsIrreducible(rationalSet);
			rationalSetProduct = rationalSet.Product();
			rationalInverseSquare = BigInteger.Multiply(rationalSetProduct, (BigInteger)SquarePolynomialDerivative);
			rationalInverseSquareRoot = rationalSetProduct.SquareRoot();
			var residue = rationalInverseSquareRoot % gnfs.N;
			RationalSquareRoot = residue;
			IsRationalSquare = RationalSquareRoot.IsSquare();
		}
		
		private static bool IsIrreducible(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}

		public void CalculateAlgebraicSide()
		{
			AlgebraicSide(233);
		}

		private void AlgebraicSide(BigInteger prime)
		{
			algebraicSet = RelationsSet.Select(rel => rel.RationalNorm % prime);
			algebraicSetProduct = algebraicSet.ProductMod(prime);

			AlgebraicSquareRoot = algebraicSetProduct;
			IsAlgebraicIrreducible = IsIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareRoot.IsSquare();			
		}
	}
}
