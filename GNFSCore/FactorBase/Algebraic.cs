
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;
using GNFSCore.Polynomial;
using ExtendedNumerics;

namespace GNFSCore.FactorBase
{
	public class Algebraic
	{
		// array of (p, r) where f(r) = 0 mod p
		// quantity = 2-3 times RFB.quantity
		public static class Factory
		{
			public static IEnumerable<Tuple<int, int>> GetAlgebraicFactorBase(GNFS gnfs)
			{
				int algebraicBound = (int)(gnfs.PrimeBound/* * 3.3*/);
				IEnumerable<int> primes = PrimeFactory.GetPrimes(algebraicBound);
				IEnumerable<int> integers = Enumerable.Range(0, primes.Last());
				return GNFS.PolynomialModP(gnfs.Algebraic, primes, integers);
			}
		}

		public static BigRational Norm(int a, int b, AlgebraicPolynomial polynomial)
		{
			Fraction ratA = new Fraction(a);
			Fraction negB = new Fraction(-b);
			BigRational aOverB = BigRational.Divide(ratA, negB);

			BigRational left = polynomial.Evaluate(aOverB);
			BigRational right = BigRational.Pow(new BigRational(negB), polynomial.Degree);

			BigRational result = BigRational.Multiply(left, right);
			return result;
		}
	}
}
