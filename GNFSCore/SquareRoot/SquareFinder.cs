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
	using Factors;

	public partial class SquareFinder
	{
		public List<Relation> RelationsSet;

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
		public List<IPolynomial> PolynomialRing;
		public List<IPolynomial> PolynomialRingModP;

		public BigInteger PrimeP;

		public IPolynomial S;
		public IPolynomial SModP;
		public IPolynomial TotalS;
		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }

		public List<BigInteger> QNorms { get { return RelationsSet.Select(rel => Normal.AlgebraicG(rel, poly.Degree - 1, polyBase, q)).ToList(); } }

		public List<Complex> ComplexFactors { get { return RelationsSet.Select(rel => rel.Complex).ToList(); } }
		public List<Complex> ComplexNorms { get { return RelationsSet.Select(rel => rel.ComplexNorm).ToList(); } }
		public List<BigInteger> RationalNormPairs { get { return RelationsSet.SelectMany(rel => new BigInteger[] { Normal.Rational(rel.A, rel.B, polyBase), Normal.RationalSubtract(rel.A, rel.B, polyBase) }).ToList(); } }
		

		private GNFS gnfs;
		private BigInteger N;
		private IPolynomial poly;
		private IPolynomial monicPoly;
		private BigInteger polyBase;
		private IEnumerable<BigInteger> rationalSet;
		private IEnumerable<BigInteger> algebraicNormCollection;
		private static BigInteger q(BigInteger ab, BigInteger m) { BigInteger m2 = m * m; return BigInteger.Subtract(m2, m); }

		public SquareFinder(GNFS sieve, List<Relation> relations)
		{
			RationalSquareRootResidue = -1;

			gnfs = sieve;
			N = gnfs.N;
			poly = gnfs.CurrentPolynomial;
			polyBase = gnfs.PolynomialBase;

			monicPoly = CommonPolynomial.MakeMonic(poly, polyBase);

			RootsOfS = new List<Tuple<BigInteger, BigInteger>>();
			AlgebraicComplexSet = new List<Complex>();
			RelationsSet = relations;
			PolynomialDerivative = gnfs.CurrentPolynomial.Derivative(gnfs.PolynomialBase);
			PolynomialDerivativeSquared = BigInteger.Pow(PolynomialDerivative, 2);
		}

		private static bool IsPrimitive(IEnumerable<BigInteger> coefficients)
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

			IsRationalIrreducible = IsPrimitive(rationalSet);
			IsRationalSquare = RationalSquareRootResidue.IsSquare();
		}

		public void CalculateAlgebraicSide()
		{
			RootsOfS.AddRange(RelationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			BigInteger leadingCoefficient = poly.Terms[poly.Degree];

			IPolynomial totalPoynomial = null;
			IPolynomial modPoynomial = null;

			PolynomialRing = new List<IPolynomial>();
			foreach (Relation rel in RelationsSet)
			{
				// poly(x) = A + (B * x) // or (A * leadingCoefficient) + (B * x) ??

				IPolynomial newPoly = new AlgebraicPolynomial(new BigInteger[] { rel.A, rel.B });
				PolynomialRing.Add(newPoly);

				if (totalPoynomial == null)
				{
					totalPoynomial = newPoly;
					modPoynomial = newPoly;
				}
				else
				{
					totalPoynomial = CommonPolynomial.Multiply(totalPoynomial, newPoly);
					modPoynomial = CommonPolynomial.Multiply(modPoynomial, newPoly);

					modPoynomial = ReduceIfNeeded(modPoynomial, poly);
				}
			}

			// multiply the totalPoynomial by f'(x)
			// This will guarantee that the square root of totalPoynomial 
			// is an element of the number field defined by the algebraic polynomial

			//IPolynomial fDerivative = CommonPolynomial.GetDerivativePolynomial(poly);
			//totalPoynomial = CommonPolynomial.Multiply(totalPoynomial, fDerivative);
			//modPoynomial = CommonPolynomial.Multiply(modPoynomial, fDerivative);

			// Mod by the algebraic polynomial if degree or leading coefficient is greater
			modPoynomial = ReduceIfNeeded(modPoynomial, poly);

			IPolynomial totalModPoly = ReduceIfNeeded(totalPoynomial, poly);
			IPolynomial totalModMonicPoly = ReduceIfNeeded(totalPoynomial, monicPoly);

			// Set the result to S
			S = modPoynomial;
			TotalS = totalPoynomial;

			//CalculateAlgebraicSquareRoot();

			algebraicNormCollection = RelationsSet.Select(rel => rel.AlgebraicNorm);
			AlgebraicProduct = algebraicNormCollection.Product();
			AlgebraicSquare = AlgebraicProduct * PolynomialDerivative;
			AlgebraicProductModF = AlgebraicSquare % poly.Evaluate(polyBase);
			AlgebraicSquareResidue = AlgebraicSquare % N;

			IsAlgebraicIrreducible = IsPrimitive(algebraicNormCollection); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();

			AlgebraicComplexSet = RelationsSet.Select(rel => new Complex(rel.A, rel.B)).ToList();
		}

		private IPolynomial ReduceIfNeeded(IPolynomial polynomial, IPolynomial mod)
		{
			var pLc = BigInteger.Abs(polynomial.Terms[polynomial.Degree]);
			var mLc = BigInteger.Abs(mod.Terms[mod.Degree]);

			if (polynomial.Degree > mod.Degree || (polynomial.Degree == mod.Degree && pLc >= mLc))
			{
				return CommonPolynomial.Mod(polynomial, mod);
			}
			else
			{
				return polynomial;
			}
		}

		private static BigInteger GenerateRandomPrime(BigInteger n)
		{
			BigInteger rangeMin = n + (n / 2) + StaticRandom.NextBigInteger(1000, 2000);
			BigInteger rangeMax = n * 5;

			bool done = false;
			BigInteger result = 0;
			do
			{
				result = PrimeFactory.GetNextPrime(StaticRandom.NextBigInteger(rangeMin, rangeMax));
				done = (GCD.FindGCD(n, result) == 1);
			}
			while (!done);

			return result;
		}

		private static IPolynomial GenerateRandomPolynomial(int degree, BigInteger maxCoefficientValue)
		{
			List<BigInteger> terms = new List<BigInteger>();

			int counter = degree;
			while (counter-- > 0)
			{
				terms.Add(StaticRandom.NextBigInteger(1, maxCoefficientValue - 1));
			}

			return new AlgebraicPolynomial(terms.ToArray());
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();


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
