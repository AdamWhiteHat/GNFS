using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
{
	public class SquareFinder
	{
		public BigInteger RationalSquareRoot;

		public GNFS gnfs;
		public BigInteger polyDerivativeSquare;

		public BigInteger rationalSetProduct;
		public BigInteger rationalSquareRoot;
		public IEnumerable<BigInteger> rationalSet;

		public SquareFinder(GNFS sieve)
		{
			RationalSquareRoot = -1;

			gnfs = sieve;
			polyDerivativeSquare = (BigInteger)(gnfs.AlgebraicPolynomial.FormalDerivative * gnfs.AlgebraicPolynomial.FormalDerivative);
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
			rationalSet = gnfs.Relations.Select(rel => rel.RationalNorm);
			rationalSetProduct = BigInteger.Multiply(rationalSet.Product(), (BigInteger)polyDerivativeSquare);
			rationalSquareRoot = rationalSetProduct.SquareRoot();
			var residue = rationalSquareRoot % gnfs.N;
			RationalSquareRoot = residue;
		}

		public void CalculateAlgebraicSide()
		{
			
		}
	}
}
