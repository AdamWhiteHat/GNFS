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

		public IPolynomial DerivativePolynomial;
		public IPolynomial DerivativePolynomialSquared;

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
		public BigInteger AlgebraicSquareRootResidue;
		public List<BigInteger> AlgebraicPrimes;
		public List<BigInteger> AlgebraicResults;
		public bool IsAlgebraicSquare;
		public bool IsAlgebraicIrreducible;

		public List<Complex> AlgebraicComplexSet;
		public List<IPoly> PolynomialRing;
		public List<IPolynomial> QuotientRingModP;

		public BigInteger PrimeP;

		public IPoly S;
		public IPoly SRingSquare;
		public IPoly TotalS;

		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }

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

			DerivativePolynomial = CommonPolynomial.GetDerivativePolynomial(poly);
			DerivativePolynomialSquared = CommonPolynomial.Mod(CommonPolynomial.Multiply(DerivativePolynomial, DerivativePolynomial), poly);

			PolynomialDerivative = DerivativePolynomial.Evaluate(gnfs.PolynomialBase);
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

			PolynomialRing = new List<IPoly>();
			foreach (Relation rel in RelationsSet)
			{
				// poly(x) = A + (B * x) // or (A * leadingCoefficient) + (B * x) ??

				IPoly newPoly =
					new SparsePolynomial(
						new PolyTerm[]
						{
							new PolyTerm( rel.B, 1),
							new PolyTerm( rel.A, 0)
						}
					);

				PolynomialRing.Add(newPoly);
			}

			BigInteger m = polyBase;
			SparsePolynomial f = new SparsePolynomial(PolyTerm.GetTerms(monicPoly.Terms));
			int degree = f.Degree;

			IPoly fd = SparsePolynomial.GetDerivativePolynomial(f);
			IPoly d3 = SparsePolynomial.Product(PolynomialRing);
			IPoly derivativeSquared = SparsePolynomial.Square(fd);
			IPoly d2 = SparsePolynomial.Multiply(d3, derivativeSquared);
			IPoly dd = SparsePolynomial.Mod(d2, f);

			// Set the result to S
			S = dd;
			SRingSquare = dd;
			TotalS = d2;

			algebraicNormCollection = RelationsSet.Select(rel => rel.AlgebraicNorm);
			AlgebraicProduct = d2.Evaluate(m);
			AlgebraicSquare = dd.Evaluate(m);
			AlgebraicProductModF = dd.Evaluate(m).Mod(N);
			AlgebraicSquareResidue = AlgebraicSquare % N;

			IsAlgebraicIrreducible = IsPrimitive(algebraicNormCollection); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();
						
			List<BigInteger> primes = new List<BigInteger>();
			List<BigInteger> results = new List<BigInteger>();
			List<Tuple<BigInteger, BigInteger>> resultTuples = new List<Tuple<BigInteger, BigInteger>>();

			BigInteger lastP = (N * 3) + 1;
			do
			{
				lastP = PrimeFactory.GetNextPrime(lastP + 1);

				Tuple<BigInteger, BigInteger> lastResult = AlgebraicSquareRoot(N, f, m, degree, dd, PolynomialRing, lastP);

				if (lastResult.Item1 != 0)
				{
					primes.Add(lastP);
					resultTuples.Add(lastResult);
					results.Add(PickEven(lastResult.Item1, lastResult.Item2));
				}
			}			
			while (primes.Count < degree);
			AlgebraicPrimes = primes;
			AlgebraicResults = results;

			AlgebraicSquareRootResidue = FiniteFieldArithmetic.ChineseRemainder(N, results, primes);
		}

		public static Tuple<BigInteger, BigInteger> AlgebraicSquareRoot(BigInteger N, SparsePolynomial f, BigInteger m, int degree, IPoly dd, List<IPoly> ideals, BigInteger p)
		{
			IPoly startPolynomial = SparsePolynomial.Modulus(dd, p);

			IPoly resultPoly1 = FiniteFieldArithmetic.SquareRoot(startPolynomial, f, p, degree, m);
			IPoly resultPoly2 = SparsePolynomial.ModularInverse(resultPoly1, p);

			BigInteger result1 = resultPoly1.Evaluate(m).Mod(p);
			BigInteger result2 = resultPoly2.Evaluate(m).Mod(p);

			IPoly resultSquared = SparsePolynomial.ModMod(SparsePolynomial.Square(resultPoly2), f, p);

			return new Tuple<BigInteger, BigInteger>(result1, result2);
		}

		public static BigInteger PickEven(BigInteger a, BigInteger b)
		{
			if (a % 2 == 0)
			{
				return a;
			}
			else if (b % 2 == 0)
			{
				return b;
			}
			else
			{
				throw new Exception("Was expecting either a or b to be even");
			}
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
