using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.SquareRoot
{
	using IntegerMath;

	public partial class SquareFinder
	{
		public BigInteger RationalProduct { get; set; }
		public BigInteger RationalSquare { get; set; }
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

		public Polynomial S { get; set; }
		public Polynomial SRingSquare { get; set; }
		public Polynomial TotalS { get; set; }
		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }
		public List<Polynomial> PolynomialRing { get; set; }
		public BigInteger PolynomialDerivative { get; set; }
		public BigInteger PolynomialDerivativeSquared { get; set; }

		private GNFS gnfs { get; set; }
		private BigInteger N { get; set; }
		private Polynomial poly { get; set; }
		private Polynomial monicPoly { get; set; }
		private BigInteger polyBase { get; set; }
		private IEnumerable<BigInteger> rationalSet { get; set; }
		private IEnumerable<BigInteger> algebraicNormCollection { get; set; }
		private List<Relation> relationsSet { get; set; }
		private Polynomial derivativePolynomial { get; set; }

		public SquareFinder(GNFS sieve, List<Relation> relations)
		{
			RationalSquareRootResidue = -1;

			gnfs = sieve;
			N = gnfs.N;
			poly = gnfs.CurrentPolynomial;
			polyBase = gnfs.PolynomialBase;

			monicPoly = Polynomial.MakeMonic(poly, polyBase);

			RootsOfS = new List<Tuple<BigInteger, BigInteger>>();
			relationsSet = relations;

			derivativePolynomial = Polynomial.GetDerivativePolynomial(poly);

			PolynomialDerivative = derivativePolynomial.Evaluate(gnfs.PolynomialBase);
			PolynomialDerivativeSquared = BigInteger.Pow(PolynomialDerivative, 2);
		}

		private static bool IsPrimitive(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}

		public void CalculateRationalSide()
		{
			rationalSet = relationsSet.Select(rel => rel.RationalNorm);

			RationalProduct = rationalSet.Product();
			RationalSquare = BigInteger.Multiply(RationalProduct, PolynomialDerivativeSquared);
			BigInteger rationalSquareRoot = RationalSquare.SquareRoot();
			RationalSquareRootResidue = rationalSquareRoot.Mod(N);

			IsRationalIrreducible = IsPrimitive(rationalSet);
			IsRationalSquare = RationalSquareRootResidue.IsSquare();
		}

		public Tuple<BigInteger, BigInteger> CalculateAlgebraicSide(CancellationToken cancelToken)
		{
			bool solutionFound = false;

			RootsOfS.AddRange(relationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			PolynomialRing = new List<Polynomial>();
			foreach (Relation rel in relationsSet)
			{
				// poly(x) = A + (B * x)
				Polynomial newPoly =
					new Polynomial(
						new Term[]
						{
							new Term( rel.B, 1),
							new Term( rel.A, 0)
						}
					);

				PolynomialRing.Add(newPoly);
			}

			if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

			BigInteger m = polyBase;
			Polynomial f = monicPoly.Clone();
			int degree = f.Degree;

			Polynomial fd = Polynomial.GetDerivativePolynomial(f);
			Polynomial d3 = Polynomial.Product(PolynomialRing);
			Polynomial derivativeSquared = Polynomial.Square(fd);
			Polynomial d2 = Polynomial.Multiply(d3, derivativeSquared);
			Polynomial dd = Polynomial.Field.Modulus(d2, f);

			// Set the result to S
			S = dd;
			SRingSquare = dd;
			TotalS = d2;

			algebraicNormCollection = relationsSet.Select(rel => rel.AlgebraicNorm);
			AlgebraicProduct = d2.Evaluate(m);
			AlgebraicSquare = dd.Evaluate(m);
			AlgebraicProductModF = dd.Evaluate(m).Mod(N);
			AlgebraicSquareResidue = AlgebraicSquare.Mod(N);

			IsAlgebraicIrreducible = IsPrimitive(algebraicNormCollection); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();

			List<BigInteger> primes = new List<BigInteger>();
			List<Tuple<BigInteger, BigInteger>> resultTuples = new List<Tuple<BigInteger, BigInteger>>();

			BigInteger primeProduct = 1;


			BigInteger lastP = ((N * 3) + 1).NthRoot(3) + 1; //N / N.ToString().Length; //((N * 3) + 1).NthRoot(3); //gnfs.QFB.Select(fp => fp.P).Max();

			int attempts = 7;

			while (!solutionFound && attempts > 0)
			{
				if (primes.Count > 0 && resultTuples.Count > 0)
				{
					primes.Remove(primes.First());
					resultTuples.Remove(resultTuples.First());
				}

				do
				{
					if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

					lastP = PrimeFactory.GetNextPrime(lastP + 1);

					Tuple<BigInteger, BigInteger> lastResult = AlgebraicSquareRoot(f, m, degree, dd, lastP);

					if (lastResult.Item1 != 0)
					{
						primes.Add(lastP);
						resultTuples.Add(lastResult);
					}
				}
				while (primes.Count < degree);


				if (primes.Count > degree)
				{
					primes.Remove(primes.First());
					resultTuples.Remove(resultTuples.First());
				}

				primeProduct = (resultTuples.Select(tup => BigInteger.Min(tup.Item1, tup.Item2)).Product());

				if (primeProduct < N)
				{
					continue;
				}

				if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); ; }

				var temp = resultTuples.Select(tup => new List<BigInteger>() { tup.Item1, tup.Item2 });

				IEnumerable<IEnumerable<BigInteger>> permutations =
					Combinatorics.CartesianProduct(temp);

				BigInteger algebraicSquareRoot = 1;

				BigInteger min;
				BigInteger max;
				BigInteger A;
				BigInteger B;
				BigInteger U;
				BigInteger V;
				BigInteger P = 0;
				BigInteger Q;

				foreach (List<BigInteger> X in permutations)
				{
					if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

					algebraicSquareRoot = FiniteFieldArithmetic.ChineseRemainder(N, X, primes);

					min = BigInteger.Min(RationalSquareRootResidue, algebraicSquareRoot);
					max = BigInteger.Max(RationalSquareRootResidue, algebraicSquareRoot);

					A = max + min;
					B = max - min;

					U = GCD.FindGCD(N, A);
					V = GCD.FindGCD(N, B);

					if (U > 1 && U != N)
					{
						P = U;
						solutionFound = true;
					}
					else if (V > 1 && V != N)
					{
						P = V;
						solutionFound = true;
					}

					if (solutionFound)
					{
						BigInteger rem;
						BigInteger other = BigInteger.DivRem(N, P, out rem);

						if (rem != 0)
						{
							solutionFound = false;
						}
						else
						{
							Q = other;
							AlgebraicResults = X;
							AlgebraicSquareRootResidue = algebraicSquareRoot;
							AlgebraicPrimes = primes;

							return new Tuple<BigInteger, BigInteger>(P, Q);
						}
					}
				}

				if (!solutionFound)
				{
					gnfs.LogFunction($"No solution found amongst the algebraic square roots {{ {string.Join(", ", resultTuples.Select(tup => $"({ tup.Item1}, { tup.Item2})"))} }} mod primes {{ {string.Join(", ", primes.Select(p => p.ToString()))} }}");

					attempts--;
				}
			}

			return new Tuple<BigInteger, BigInteger>(1, 1);
		}

		private static Tuple<BigInteger, BigInteger> AlgebraicSquareRoot(Polynomial f, BigInteger m, int degree, Polynomial dd, BigInteger p)
		{
			Polynomial startPolynomial = Polynomial.Field.Modulus(dd, p);
			Polynomial startInversePolynomial = ModularInverse(startPolynomial, p);

			Polynomial resultPoly1 = FiniteFieldArithmetic.SquareRoot(startPolynomial, f, p, degree, m);
			Polynomial resultPoly2 = ModularInverse(resultPoly1, p);

			Polynomial resultSquared1 = Polynomial.Field.ModMod(Polynomial.Square(resultPoly1), f, p);
			Polynomial resultSquared2 = Polynomial.Field.ModMod(Polynomial.Square(resultPoly2), f, p);

			bool bothResultsAgree = (resultSquared1.CompareTo(resultSquared2) == 0);
			if (bothResultsAgree)
			{
				bool resultSquaredEqualsInput1 = (startPolynomial.CompareTo(resultSquared1) == 0);
				bool resultSquaredEqualsInput2 = (startInversePolynomial.CompareTo(resultSquared1) == 0);

				if (resultSquaredEqualsInput1 || resultSquaredEqualsInput2)
				{
					BigInteger result1 = resultPoly1.Evaluate(m).Mod(p);
					BigInteger result2 = resultPoly2.Evaluate(m).Mod(p);

					if (resultSquaredEqualsInput1)
					{
						return new Tuple<BigInteger, BigInteger>(result1, result2);
					}
					else if (resultSquaredEqualsInput2)
					{
						return new Tuple<BigInteger, BigInteger>(result2, result1);
					}
				}
			}

			return new Tuple<BigInteger, BigInteger>(BigInteger.Zero, BigInteger.Zero);
		}

		private static Polynomial ModularInverse(Polynomial poly, BigInteger mod)
		{
			return new Polynomial(Term.GetTerms(poly.Terms.Select(trm => (mod - trm.CoEfficient).Mod(mod)).ToArray()));
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
			result.AppendLine($"γ  =    {RationalSquareRootResidue} mod N"); // δ mod N 

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

			BigInteger min = BigInteger.Min(RationalSquareRootResidue, AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(RationalSquareRootResidue, AlgebraicSquareRootResidue);

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
