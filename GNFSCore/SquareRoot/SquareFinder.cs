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
	using System.IO;

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

		public BigInteger N { get; set; }
		public Polynomial S { get; set; }
		public Polynomial TotalS { get; set; }
		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }
		public Polynomial PolynomialRing { get; set; }
		public List<Polynomial> PolynomialRingElements { get; set; }

		public BigInteger PolynomialBase { get; set; }
		public Polynomial MonicPolynomial { get; set; }
		public Polynomial PolynomialDerivative { get; set; }
		public Polynomial MonicPolynomialDerivative { get; set; }
		public BigInteger PolynomialDerivativeValue { get; set; }
		public BigInteger PolynomialDerivativeValueSquared { get; set; }
		public Polynomial MonicPolynomialDerivativeSquared { get; set; }

		private GNFS gnfs { get; set; }
		private IEnumerable<BigInteger> rationalNorms { get; set; }
		private IEnumerable<BigInteger> algebraicNormCollection { get; set; }
		private List<Relation> relationsSet { get; set; }

		public SquareFinder(GNFS sieve)
		{
			RationalSquareRootResidue = -1;
			RootsOfS = new List<Tuple<BigInteger, BigInteger>>();

			gnfs = sieve;
			N = gnfs.N;
			PolynomialBase = gnfs.PolynomialBase;

			PolynomialDerivative = Polynomial.GetDerivativePolynomial(gnfs.CurrentPolynomial);
			PolynomialDerivativeValue = PolynomialDerivative.Evaluate(gnfs.PolynomialBase);
			PolynomialDerivativeValueSquared = BigInteger.Pow(PolynomialDerivativeValue, 2);

			MonicPolynomial = Polynomial.MakeMonic(gnfs.CurrentPolynomial, PolynomialBase);
			MonicPolynomialDerivative = Polynomial.GetDerivativePolynomial(MonicPolynomial);
			MonicPolynomialDerivativeSquared = Polynomial.Square(MonicPolynomialDerivative);
		}

		private static bool IsPrimitive(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}

		public static bool Solve(CancellationToken cancelToken, GNFS gnfs)
		{
			List<int> triedFreeRelationIndices = new List<int>();

			BigInteger polyBase = gnfs.PolynomialBase;
			List<List<Relation>> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations;
			SquareFinder squareRootFinder = new SquareFinder(gnfs);

			int freeRelationIndex = 0;
			bool solutionFound = false;

			// Below randomly selects a solution set to try and find a square root of the polynomial in.
			while (!solutionFound)
			{
				if (cancelToken.IsCancellationRequested) { return solutionFound; }

				// Each time this step is stopped and restarted, it will try a different solution set.
				// Previous used sets are tracked with the List<int> triedFreeRelationIndices
				if (triedFreeRelationIndices.Count == freeRelations.Count) // If we have exhausted our solution sets, alert the user. Number wont factor for some reason.
				{
					gnfs.LogMessage("ERROR: ALL RELATION SETS HAVE BEEN TRIED...?");
					gnfs.LogMessage($"If the number of solution sets ({freeRelations.Count}) is low, you may need to sieve some more and then re-run the matrix solving step.");
					gnfs.LogMessage("If there are many solution sets, and you have tried them all without finding non-trivial factors, then something is wrong...");
					gnfs.LogMessage();
					break;
				}

				do
				{
					// Below randomly selects a solution set to try and find a square root of the polynomial in.
					freeRelationIndex = StaticRandom.Next(0, freeRelations.Count);
				}
				while (triedFreeRelationIndices.Contains(freeRelationIndex));

				triedFreeRelationIndices.Add(freeRelationIndex); // Add current selection to our list

				List<Relation> selectedRelationSet = freeRelations[freeRelationIndex]; // Get the solution set

				gnfs.LogMessage($"Selected solution set # {freeRelationIndex + 1}");
				gnfs.LogMessage();
				gnfs.LogMessage($"Selected set (a,b) pairs (count: {selectedRelationSet.Count}): {string.Join(" ", selectedRelationSet.Select(rel => $"({rel.A},{rel.B})"))}");
				gnfs.LogMessage();
				gnfs.LogMessage();
				gnfs.LogMessage();
				gnfs.LogMessage($"ƒ'(m)     = {squareRootFinder.PolynomialDerivativeValue}");
				gnfs.LogMessage($"ƒ'(m)^2   = {squareRootFinder.PolynomialDerivativeValueSquared}");
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Rational Square Root.");
				gnfs.LogMessage("Please wait...");

				squareRootFinder.CalculateRationalSide(selectedRelationSet);

				gnfs.LogMessage("Completed.");
				gnfs.LogMessage();
				gnfs.LogMessage($"γ²        = {squareRootFinder.RationalProduct} IsSquare? {squareRootFinder.RationalProduct.IsSquare()}");
				gnfs.LogMessage($"(γ  · ƒ'(m))^2 = {squareRootFinder.RationalSquare} IsSquare? {squareRootFinder.RationalSquare.IsSquare()}");
				gnfs.LogMessage();
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Algebraic Square Root.");
				gnfs.LogMessage("Please wait...");

				if (cancelToken.IsCancellationRequested) { return solutionFound; }

				squareRootFinder.CalculateAlgebraicSide(cancelToken);

				if (cancelToken.IsCancellationRequested) { return solutionFound; }

				Tuple<BigInteger, BigInteger> foundFactors = squareRootFinder.CalculateSquareRoot(cancelToken);

				BigInteger P = foundFactors.Item1;
				BigInteger Q = foundFactors.Item2;

				bool nonTrivialFactorsFound = (P != 1 || Q != 1);
				if (nonTrivialFactorsFound)
				{
					solutionFound = gnfs.SetFactorizationSolution(P, Q);

					if (solutionFound)
					{
						gnfs.LogMessage("NON-TRIVIAL FACTORS FOUND!");
						gnfs.LogMessage();
						gnfs.LogMessage(squareRootFinder.ToString());
						gnfs.LogMessage();
						gnfs.LogMessage();
						gnfs.LogMessage(gnfs.Factorization.ToString());
						gnfs.LogMessage();
					}
					break;
				}
				else if (cancelToken.IsCancellationRequested)
				{
					gnfs.LogMessage("Aborting square root search.");
					break;
				}
				else
				{
					gnfs.LogMessage();
					gnfs.LogMessage("Unable to locate a square root in solution set!");
					gnfs.LogMessage();
					gnfs.LogMessage("Trying a different solution set...");
					gnfs.LogMessage();
				}
			}

			return solutionFound;
		}

		public void CalculateRationalSide(List<Relation> relations)
		{
			relationsSet = relations;
			rationalNorms = relationsSet.Select(rel => rel.RationalNorm);

			IsRationalIrreducible = IsPrimitive(rationalNorms);
			if (!IsRationalIrreducible)
			{
				// I feel like we need to do something here, but I cannot find in the literature where it even mentions performing such a test,
				// so perhaps we are doing something wrong here, that we are meant to be checking the polynomial for irreducibility instead?
				// Perhaps I inserted this check here just to ensure correctness.
				// Is it an error to have two rational norms share a factor, or merely pointless?
				throw new Exception($"{nameof(IsRationalIrreducible)} evaluated to false.");
			}

			RationalProduct = rationalNorms.Product();
			RationalSquare = BigInteger.Multiply(RationalProduct, PolynomialDerivativeValueSquared);
			BigInteger rationalSquareRoot = RationalSquare.SquareRoot();
			RationalSquareRootResidue = rationalSquareRoot.Mod(N);

			IsRationalSquare = RationalSquareRootResidue.IsSquare();

			if (!IsRationalSquare) // This is an error in implementation. This should never happen, and so must be a bug
			{
				//throw new Exception($"{nameof(IsRationalSquare)} evaluated to false. This is a sign that there is a bug in the implementation, as this should never be the case if the algorithm has been correctly implemented.");
			}
		}

		public void CalculateAlgebraicSide(CancellationToken cancelToken)
		{
			RootsOfS.AddRange(relationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			PolynomialRingElements = new List<Polynomial>();
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

				PolynomialRingElements.Add(newPoly);
			}

			if (cancelToken.IsCancellationRequested) { return; }

			PolynomialRing = Polynomial.Product(PolynomialRingElements);
			// Multiply the product of the polynomial elements by f'(x)^2
			// This will guarantee that the square root of product of polynomials
			// is an element of the number field defined by the algebraic polynomial.
			TotalS = Polynomial.Multiply(PolynomialRing, MonicPolynomialDerivativeSquared);
			S = Polynomial.Field.Modulus(TotalS, MonicPolynomial);

			algebraicNormCollection = relationsSet.Select(rel => rel.AlgebraicNorm);
			AlgebraicProduct = TotalS.Evaluate(PolynomialBase);
			AlgebraicSquare = S.Evaluate(PolynomialBase);
			AlgebraicProductModF = S.Evaluate(PolynomialBase).Mod(N);
			AlgebraicSquareResidue = AlgebraicSquare.Mod(N);

			IsAlgebraicIrreducible = IsPrimitive(algebraicNormCollection); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();
		}

		public Tuple<BigInteger, BigInteger> CalculateSquareRoot(CancellationToken cancelToken)
		{
			bool solutionFound = false;
			int degree = MonicPolynomial.Degree;

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

					Tuple<BigInteger, BigInteger> lastResult = AlgebraicSquareRoot(MonicPolynomial, PolynomialBase, degree, S, lastP);

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

			result.AppendLine($"∏ Sᵢ =");
			result.AppendLine($"{TotalS}");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ (mod ƒ) =");
			result.AppendLine($"{S}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Rational:");
			result.AppendLine($"γ² = √(  Sᵣ(m)  *  ƒ'(m)²  )");
			result.AppendLine($"γ² = √( {RationalProduct} * {PolynomialDerivativeValueSquared} )");
			result.AppendLine($"γ² = √( {RationalSquare} )");
			result.AppendLine($"IsRationalSquare  ? {IsRationalSquare}");
			result.AppendLine($"γ  =    {RationalSquareRootResidue} mod N"); // δ mod N 
			result.AppendLine($"IsRationalIrreducible  ? {IsRationalIrreducible}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Algebraic:");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicProduct} * {PolynomialDerivativeValue}");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicSquare}");
			result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareRootResidue}");
			result.AppendLine($"IsAlgebraicIrreducible ? {IsAlgebraicIrreducible}");
			result.AppendLine();
			result.AppendLine($"X² / ƒ(m) = {AlgebraicProductModF}  IsSquare? {AlgebraicProductModF.IsSquare()}");
			result.AppendLine($"S (x)       = {AlgebraicSquareResidue}  IsSquare? {AlgebraicSquareResidue.IsSquare()}");
			result.AppendLine($"AlgebraicResults:");
			result.AppendLine($"{AlgebraicResults.FormatString(false)}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Polynomial ring:");
			result.AppendLine($"({string.Join(") * (", PolynomialRingElements.Select(ply => ply.ToString()))})");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Primes:");
			result.AppendLine($"{string.Join(" * ", AlgebraicPrimes)}"); // .RelationsSet.Select(rel => rel.B).Distinct().OrderBy(relB => relB))
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Roots of S(x):");
			result.AppendLine($"{{{string.Join(", ", RootsOfS.Select(tup => (tup.Item2 > 1) ? $"{tup.Item1}/{tup.Item2}" : $"{tup.Item1}"))}}}");
			result.AppendLine();
			result.AppendLine();
			//result.AppendLine($"∏(a + mb) = {squareRootFinder.RationalProduct}");
			//result.AppendLine($"∏ƒ(a/b)   = {squareRootFinder.AlgebraicProduct}");
			//result.AppendLine();

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
