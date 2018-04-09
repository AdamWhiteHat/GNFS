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
	using Polynomial.Internal;
	using System.Collections;

	public partial class SquareFinder
	{
		public Relation[] RelationsSet;
		public Relation[] AlgebraicRelationsSet;

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
		public BigInteger AlgebraicProductModF;
		public BigInteger AlgebraicSquareResidue;
		public bool IsAlgebraicSquare;
		public bool IsAlgebraicIrreducible;

		public List<Complex> AlgebraicComplexSet;

		public BigInteger Y2;
		public BigInteger Y2_S;

		public IPolynomial S;

		private GNFS gnfs;
		private BigInteger N;
		private BigInteger polyBase;
		private Func<BigInteger, BigInteger> f;
		private IEnumerable<BigInteger> rationalSet;
		private IEnumerable<BigInteger> algebraicSet;

		public SquareFinder(GNFS sieve, List<Relation> relations)
		{
			RationalSquareRootResidue = -1;

			gnfs = sieve;
			N = gnfs.N;
			polyBase = gnfs.CurrentPolynomial.Base;
			f = (x) => gnfs.CurrentPolynomial.Evaluate(x);


			AlgebraicComplexSet = new List<Complex>();
			RelationsSet = relations.ToArray();
			AlgebraicRelationsSet = RelationsSet;//.Where(rel => rel.B == 1).ToArray();
			PolynomialDerivative = gnfs.CurrentPolynomial.Derivative(gnfs.CurrentPolynomial.Base);
			PolynomialDerivativeSquared = BigInteger.Pow(PolynomialDerivative, 2);
		}

		private static bool IsIrreducible(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}
		
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
			algebraicSet = AlgebraicRelationsSet.Select(rel => rel.AlgebraicNorm);

			AlgebraicProduct = algebraicSet.Product();
			AlgebraicSquare = AlgebraicProduct * PolynomialDerivative;
			AlgebraicProductModF = AlgebraicSquare % f(polyBase);
			AlgebraicSquareResidue = AlgebraicSquare % N;

			IsAlgebraicIrreducible = IsIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();

			AlgebraicComplexSet = RelationsSet.Select(rel => new Complex(rel.A, rel.B)).ToList();
		}
		
		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			/*
			result.AppendLine($"IsRationalIrreducible  ? {IsRationalIrreducible}");
			result.AppendLine($"IsAlgebraicIrreducible ? {IsAlgebraicIrreducible}");

			result.AppendLine("Square finder, Rational:");
			result.AppendLine($"γ² = √(  Sᵣ(m)  *  ƒ'(m)²  )");
			result.AppendLine($"γ² = √( {RationalProduct} * {PolynomialDerivativeSquared} )");
			result.AppendLine($"γ² = √( {RationalSquare} )");
			result.AppendLine($"γ  =    {RationalSquareRoot} mod N");
			result.AppendLine($"γ  =    {RationalSquareRootResidue}"); // δ mod N 

			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Algebraic:");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicProduct} * {PolynomialDerivative}");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicSquare}");
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareResidue}");
			*/

			result.AppendLine($"γ = {RationalSquareRootResidue}");
			result.AppendLine($"χ = {AlgebraicSquareResidue}");

			result.AppendLine($"IsRationalSquare  ? {IsRationalSquare}");
			result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");



			BigInteger min = BigInteger.Min(RationalSquareRoot, AlgebraicSquareResidue);
			BigInteger max = BigInteger.Max(RationalSquareRoot, AlgebraicSquareResidue);

			BigInteger add = max + min;
			BigInteger sub = max - min;

			BigInteger gcdAdd = GCD.FindGCD(N, add);
			BigInteger gcdSub = GCD.FindGCD(N, sub);

			BigInteger answer = BigInteger.Max(gcdAdd, gcdSub);


			result.AppendLine();
			result.AppendLine($"GCD(N, γ+χ) = {gcdAdd}");
			result.AppendLine($"GCD(N, γ-χ) = {gcdSub}");
			result.AppendLine();
			result.AppendLine($"Solution? {(answer != 1).ToString().ToUpper()}");

			if (answer != 1)
			{
				result.AppendLine();
				result.AppendLine();
				result.AppendLine("*********************");
				result.AppendLine();
				result.AppendLine($" SOLUTION = {answer} ");
				result.AppendLine();
				result.AppendLine("*********************");
				result.AppendLine();
				result.AppendLine();
			}

			result.AppendLine();



			return result.ToString();
		}
	}
}
