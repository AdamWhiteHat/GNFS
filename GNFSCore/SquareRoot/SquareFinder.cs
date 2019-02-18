using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Collections.Generic;

namespace GNFSCore.SquareRoot
{
	using Polynomials;
	using IntegerMath;

	public partial class SquareFinder
	{
		public List<Relation> RelationsSet { get; set; }

		public BigInteger PolynomialDerivative { get; set; }
		public BigInteger PolynomialDerivativeSquared { get; set; }

		public IPolynomial DerivativePolynomial { get; set; }
		public IPolynomial DerivativePolynomialSquared { get; set; }

		public BigInteger RationalProduct { get; set; }
		public BigInteger RationalSquare { get; set; }
		public BigInteger RationalSquareRoot { get; set; }
		public BigInteger RationalSquareRootResidue { get; set; }
		public bool IsRationalSquare { get; set; }
		public bool IsRationalIrreducible { get; set; }

		public BigInteger AlgebraicProduct { get; set; }
		public BigInteger AlgebraicSquare { get; set; }
		public BigInteger AlgebraicProductModF { get; set; }
		public BigInteger AlgebraicSquareResidue { get; set; }
		public BigInteger AlgebraicSquareRootResidue { get; set; }
		public List<BigInteger> AlgebraicPrimes { get; set; }
		public List<BigInteger> AlgebraicResults { get; set; }
		public bool IsAlgebraicSquare { get; set; }
		public bool IsAlgebraicIrreducible { get; set; }

		public List<Complex> AlgebraicComplexSet { get; set; }
		public List<IPolynomial> PolynomialRing { get; set; }

		public IPolynomial S { get; set; }
		public IPolynomial SRingSquare { get; set; }
		public IPolynomial TotalS { get; set; }

		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }

		private GNFS gnfs { get; set; }
		private BigInteger N { get; set; }
		private IPolynomial poly { get; set; }
		private IPolynomial monicPoly { get; set; }
		private BigInteger polyBase { get; set; }
		private IEnumerable<BigInteger> rationalSet { get; set; }
		private IEnumerable<BigInteger> algebraicNormCollection { get; set; }

		public SquareFinder(GNFS sieve, List<Relation> relations)
		{
			RationalSquareRootResidue = -1;

			gnfs = sieve;
			N = gnfs.N;
			poly = gnfs.CurrentPolynomial;
			polyBase = gnfs.PolynomialBase;

			monicPoly = Polynomial.MakeMonic(poly, polyBase);

			RootsOfS = new List<Tuple<BigInteger, BigInteger>>();
			AlgebraicComplexSet = new List<Complex>();
			RelationsSet = relations;

			DerivativePolynomial = Polynomial.GetDerivativePolynomial(poly);
			DerivativePolynomialSquared = Polynomial.Mod(Polynomial.Square(DerivativePolynomial), poly);

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
			RationalSquareRootResidue = RationalSquareRoot.Mod(N);

			IsRationalIrreducible = IsPrimitive(rationalSet);
			IsRationalSquare = RationalSquareRootResidue.IsSquare();
		}

		public bool CalculateAlgebraicSide()
		{
			RootsOfS.AddRange(RelationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			PolynomialRing = new List<IPolynomial>();
			foreach (Relation rel in RelationsSet)
			{
				// poly(x) = A + (B * x)
				IPolynomial newPoly =
					new Polynomial(
						new Term[]
						{
							new Term( rel.B, 1),
							new Term( rel.A, 0)
						}
					);

				PolynomialRing.Add(newPoly);
			}

			BigInteger m = polyBase;
			IPolynomial f = (Polynomial)monicPoly.Clone();
			int degree = f.Degree;

			IPolynomial fd = Polynomial.GetDerivativePolynomial(f);
			IPolynomial d3 = Polynomial.Product(PolynomialRing);
			IPolynomial derivativeSquared = Polynomial.Square(fd);
			IPolynomial d2 = Polynomial.Multiply(d3, derivativeSquared);
			IPolynomial dd = Polynomial.Mod(d2, f);

			// Set the result to S
			S = dd;
			SRingSquare = dd;
			TotalS = d2;

			algebraicNormCollection = RelationsSet.Select(rel => rel.AlgebraicNorm);
			AlgebraicProduct = d2.Evaluate(m);
			AlgebraicSquare = dd.Evaluate(m);
			AlgebraicProductModF = dd.Evaluate(m).Mod(N);
			AlgebraicSquareResidue = AlgebraicSquare.Mod(N);

			IsAlgebraicIrreducible = IsPrimitive(algebraicNormCollection); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();

			List<BigInteger> primes = new List<BigInteger>();
			List<Tuple<BigInteger, BigInteger>> resultTuples = new List<Tuple<BigInteger, BigInteger>>();

			BigInteger primeProduct = 1;



			BigInteger lastP = N / N.ToString().Length; //((N * 3) + 1).NthRoot(3); //gnfs.QFB.Select(fp => fp.P).Max();
			do
			{
				lastP = PrimeFactory.GetNextPrime(lastP + 1);

				Tuple<BigInteger, BigInteger> lastResult = AlgebraicSquareRoot(f, m, degree, dd, lastP);

				if (lastResult.Item1 != 0)
				{
					primes.Add(lastP);
					resultTuples.Add(lastResult);
					primeProduct *= BigInteger.Min(lastResult.Item1, lastResult.Item2);
				}
			}
			while (primeProduct < N || primes.Count < degree);
			AlgebraicPrimes = primes;

			IEnumerable<IEnumerable<BigInteger>> permutations =
				Combinatorics.CartesianProduct(resultTuples.Select(tup => new List<BigInteger>() { tup.Item1, tup.Item2 }));

			BigInteger rationalSquareRoot = RationalSquareRootResidue;
			BigInteger algebraicSquareRoot = 1;

			bool solutionFound = false;
			foreach (List<BigInteger> X in permutations)
			{
				algebraicSquareRoot = FiniteFieldArithmetic.ChineseRemainder(N, X, primes);

				BigInteger min = BigInteger.Min(rationalSquareRoot, algebraicSquareRoot);
				BigInteger max = BigInteger.Max(rationalSquareRoot, algebraicSquareRoot);

				BigInteger A = max + min;
				BigInteger B = max - min;

				BigInteger U = GCD.FindGCD(N, A);
				BigInteger V = GCD.FindGCD(N, B);

				if (U > 1 && V > 1)
				{
					solutionFound = true;
					AlgebraicResults = X;
					AlgebraicSquareRootResidue = algebraicSquareRoot;

					break;
				}
			}

			return solutionFound;
		}

		public static Tuple<BigInteger, BigInteger> AlgebraicSquareRoot(IPolynomial f, BigInteger m, int degree, IPolynomial dd, BigInteger p)
		{
			IPolynomial startPolynomial = Polynomial.Modulus(dd, p);
			IPolynomial startInversePolynomial = Polynomial.ModularInverse(startPolynomial, p);

			IPolynomial resultPoly1 = FiniteFieldArithmetic.SquareRoot(startPolynomial, f, p, degree, m);
			IPolynomial resultPoly2 = Polynomial.ModularInverse(resultPoly1, p);

			BigInteger result1 = resultPoly1.Evaluate(m).Mod(p);
			BigInteger result2 = resultPoly2.Evaluate(m).Mod(p);

			IPolynomial resultSquared1 = Polynomial.ModMod(Polynomial.Square(resultPoly1), f, p);
			IPolynomial resultSquared2 = Polynomial.ModMod(Polynomial.Square(resultPoly2), f, p);

			bool bothResultsAgree = (resultSquared1.CompareTo(resultSquared2) == 0);
			if (bothResultsAgree)
			{
				bool resultSquaredEqualsInput1 = (startPolynomial.CompareTo(resultSquared1) == 0);
				bool resultSquaredEqualsInput2 = (startInversePolynomial.CompareTo(resultSquared1) == 0);
				
				if (resultSquaredEqualsInput1)
				{
					return new Tuple<BigInteger, BigInteger>(result1, result2);
				}
				else if (resultSquaredEqualsInput2)
				{
					return new Tuple<BigInteger, BigInteger>(result2, result1);
				}
			}
			
			return new Tuple<BigInteger, BigInteger>(BigInteger.Zero, BigInteger.Zero);			
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
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareRootResidue}");


			result.AppendLine($"γ = {RationalSquareRootResidue}");
			result.AppendLine($"χ = {AlgebraicSquareRootResidue}");

			result.AppendLine($"IsRationalSquare  ? {IsRationalSquare}");
			result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");

			BigInteger min = BigInteger.Min(RationalSquareRoot, AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(RationalSquareRoot, AlgebraicSquareRootResidue);

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
