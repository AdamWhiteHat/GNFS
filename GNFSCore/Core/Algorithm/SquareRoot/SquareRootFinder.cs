using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Core.Algorithm.SquareRoot
{
	using GNFSCore.Core.Algorithm;

	using GNFSCore.Core.Algorithm.IntegerMath;
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.RelationSieve;
	using System.ComponentModel.DataAnnotations;
	using static GNFSCore.Core.Data.GNFS;

	public static class SquareRootFinder
	{
		public static bool Solve(CancellationToken cancelToken, GNFS gnfs)
		{
			List<int> triedFreeRelationIndices = new List<int>();

			BigInteger polyBase = gnfs.PolynomialBase;
			List<List<Relation>> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations;
			SquareRoot sqrt = new SquareRoot(gnfs);
			gnfs.SquareRoot = sqrt;

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

				gnfs.LogMessage();
				gnfs.LogMessage($"Selected solution set index # {freeRelationIndex + 1}");
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Rational Square Root β ∈ ℤ[θ] ...");
				gnfs.LogMessage();
				BigInteger rationalSquareRoot = SquareRootFinder.CalculateRationalSide(cancelToken, selectedRelationSet, gnfs);

				if (cancelToken.IsCancellationRequested) { gnfs.LogMessage("Abort: Task canceled by user!"); break; }

				gnfs.LogMessage("SquareFinder.CalculateRationalSide() Completed.");
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Algebraic Square Root...");
				gnfs.LogMessage("                    y ∈ ℤ ...");
				gnfs.LogMessage("δ in a finite field 𝔽ᵨ(θᵨ) ...");
				gnfs.LogMessage();

				foreach (BigInteger algebraicSquareRoot in SquareRootFinder.CalculateAlgebraicSide(cancelToken, gnfs))
				{
					gnfs.LogMessage("SquareFinder.CalculateAlgebraicSide() Completed.");

					BigInteger min = BigInteger.Min(rationalSquareRoot, algebraicSquareRoot);
					BigInteger max = BigInteger.Max(rationalSquareRoot, algebraicSquareRoot);

					BigInteger A = max + min;
					BigInteger B = max - min;

					BigInteger C = GCD.FindGCD(gnfs.N, A);
					BigInteger D = GCD.FindGCD(gnfs.N, B);

					if ((C > 1 && C != gnfs.N) || (D > 1 && D != gnfs.N))
					{

						BigInteger P = 1;
						if (C > 1)
						{
							P = C;
						}
						else if (D > 1)
						{
							P = D;
						}

						BigInteger Q = gnfs.N / P;
						if (Q != 1 && Q != gnfs.N)
						{
							solutionFound = true;
						}

						gnfs.SetFactorizationSolution(P, Q);
						break;
					}

					if (cancelToken.IsCancellationRequested) { gnfs.LogMessage("Abort: Task canceled by user!"); break; }
				}

				if (solutionFound)
				{
					gnfs.LogMessage();
					gnfs.LogMessage($"{gnfs.SquareRoot.AlgebraicSquareRootResidue}² ≡ {gnfs.SquareRoot.RationalSquareRootResidue}² (mod {gnfs.N})");
					gnfs.LogMessage();


					gnfs.LogMessage("NON-TRIVIAL FACTORS FOUND!");
					gnfs.LogMessage();
					gnfs.LogMessage(gnfs.SquareRoot.ToString());
					gnfs.LogMessage();
					gnfs.LogMessage();
					gnfs.LogMessage(gnfs.Factorization.ToString());
					gnfs.LogMessage();

					break;
				}
				else if (cancelToken.IsCancellationRequested)
				{
					gnfs.LogMessage("Abort: Task canceled by user!");
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

		public static BigInteger CalculateRationalSide(CancellationToken cancelToken, List<Relation> relations, GNFS gnfs)
		{
			gnfs.SquareRoot.RelationsSet = relations;
			gnfs.SquareRoot.RationalNormCollection = gnfs.SquareRoot.RelationsSet.Select(rel => rel.RationalNorm).ToList();

			CountDictionary rationalSquareFactorization = new CountDictionary();
			foreach (var rel in gnfs.SquareRoot.RelationsSet)
			{
				rationalSquareFactorization.Combine(rel.RationalFactorization);
			}

			string rationalSquareFactorizationString = rationalSquareFactorization.FormatStringAsFactorization();
			Logging.WriteLine();
			Logging.WriteLine("Rational Square Dependency:");
			Logging.WriteLine(rationalSquareFactorizationString);

			if (cancelToken.IsCancellationRequested) { return -1; }

			gnfs.SquareRoot.RationalProduct = gnfs.SquareRoot.RationalNormCollection.Product();
			gnfs.SquareRoot.RationalSquare = gnfs.SquareRoot.RationalProduct;

			Logging.WriteLine();
			Logging.WriteLine($"δᵣ = {gnfs.SquareRoot.RationalProduct} = {string.Join(" * ", gnfs.SquareRoot.RationalNormCollection)}");

			BigInteger RationalProductSquareRoot = gnfs.SquareRoot.RationalProduct.SquareRoot();

			var product = gnfs.SquareRoot.PolynomialDerivativeValue * RationalProductSquareRoot;

			gnfs.SquareRoot.RationalSquareRootResidue = product.Mod(gnfs.N);

			Logging.WriteLine();
			Logging.WriteLine($"δᵣ = {RationalProductSquareRoot}^2 = {gnfs.SquareRoot.RationalProduct}");
			Logging.WriteLine($"χ  = {gnfs.SquareRoot.RationalSquareRootResidue} ≡ {gnfs.SquareRoot.PolynomialDerivativeValue} * {RationalProductSquareRoot} (mod {gnfs.N})");
			Logging.WriteLine();

			gnfs.SquareRoot.IsRationalSquare = gnfs.SquareRoot.RationalProduct.IsSquare();
			if (!gnfs.SquareRoot.IsRationalSquare) // This is an error in implementation. This should never happen, and so must be a bug
			{
				throw new Exception($"{nameof(gnfs.SquareRoot.IsRationalSquare)} evaluated to false. This is a sign that there is a bug in the implementation, as this should never be the case if the algorithm has been correctly implemented.");
			}

			return gnfs.SquareRoot.RationalSquareRootResidue;
		}

		public static IEnumerable<BigInteger> CalculateAlgebraicSide(CancellationToken cancelToken, GNFS gnfs)
		{
			gnfs.SquareRoot.RootsOfS.AddRange(gnfs.SquareRoot.RelationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			if (cancelToken.IsCancellationRequested) { yield break; }

			gnfs.SquareRoot.PolynomialRingElements_PrimeIdeals = new List<Polynomial>();
			foreach (Relation rel in gnfs.SquareRoot.RelationsSet)
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

				gnfs.SquareRoot.PolynomialRingElements_PrimeIdeals.Add(newPoly);
			}

			if (cancelToken.IsCancellationRequested) { yield break; }

			gnfs.SquareRoot.PolynomialRing = Polynomial.Product(gnfs.SquareRoot.PolynomialRingElements_PrimeIdeals);
			Polynomial PolynomialRingInField = Polynomial.Field.Modulus(gnfs.SquareRoot.PolynomialRing, gnfs.SquareRoot.MonicPolynomial);

			gnfs.SquareRoot.AlgebraicProduct = gnfs.SquareRoot.PolynomialRing.Evaluate(gnfs.PolynomialBase);
			gnfs.SquareRoot.AlgebraicProductModF = PolynomialRingInField.Evaluate(gnfs.PolynomialBase);

			Logging.WriteLine();
			Logging.WriteLine($"∏ Sᵢ = {gnfs.SquareRoot.PolynomialRing}");
			Logging.WriteLine();
			Logging.WriteLine($"∏ Sᵢ = {PolynomialRingInField}");
			Logging.WriteLine(" in ℤ");
			Logging.WriteLine();

			if (cancelToken.IsCancellationRequested) { yield break; }

			// Multiply the product of the polynomial elements by f'(x)^2
			// This will guarantee that the square root of product of polynomials
			// is an element of the number field defined by the algebraic polynomial.
			gnfs.SquareRoot.TotalS = Polynomial.Multiply(gnfs.SquareRoot.PolynomialRing, gnfs.SquareRoot.MonicPolynomialDerivativeSquared);
			gnfs.SquareRoot.S = Polynomial.Field.Modulus(gnfs.SquareRoot.TotalS, gnfs.SquareRoot.MonicPolynomial);

			gnfs.SquareRoot.AlgebraicSquare = gnfs.SquareRoot.TotalS.Evaluate(gnfs.PolynomialBase);
			gnfs.SquareRoot.AlgebraicSquareResidue = gnfs.SquareRoot.S.Evaluate(gnfs.PolynomialBase);

			Logging.WriteLine();
			Logging.WriteLine($"δᵨ = {gnfs.SquareRoot.TotalS}");
			Logging.WriteLine($"δᵨ = {gnfs.SquareRoot.S}");
			Logging.WriteLine(" in ℤ");

			int degree = gnfs.SquareRoot.MonicPolynomial.Degree;
			Polynomial f = gnfs.SquareRoot.MonicPolynomial;// gnfs.CurrentPolynomial;

			BigInteger lastP = gnfs.QuadraticFactorPairCollection.Last().P; //quadraticPrimes.First(); //BigInteger.Max(fromRoot, fromQuadraticFactorPairs); //N / N.ToString().Length; //((N * 3) + 1).NthRoot(3); //gnfs.QFB.Select(fp => fp.P).Max();

			lastP = BigInteger.Max(lastP, gnfs.N.SquareRoot() * 2);

			BigInteger p;

			if (cancelToken.IsCancellationRequested) { yield break; }

			List<(BigInteger prime, BigInteger value)> compatableFields = new List<(BigInteger prime, BigInteger value)>();

			do
			{
				p = PrimeFactory.GetNextPrime(lastP + 1);

				if (compatableFields.Count == degree)
				{
					compatableFields.RemoveAt(0);
				}

				Polynomial g = Polynomial.Parse($"X^{lastP} - X");
				Polynomial h = Polynomial.Field.ModMod(g, f, lastP);
				Polynomial gcd = Polynomial.Field.GCD(h, f, lastP);

				bool isIrreducible = gcd.CompareTo(Polynomial.One) == 0;
				if (isIrreducible)
				{
					Polynomial sqrtOfS = FiniteFieldArithmetic.SquareRoot(gnfs.SquareRoot.S, gnfs.CurrentPolynomial, p, gnfs.PolynomialDegree, gnfs.PolynomialBase);

					BigInteger eval = sqrtOfS.Evaluate(gnfs.PolynomialBase);
					BigInteger x = BigIntegerExtensionMethods.Mod(eval, p);

					//Polynomial inverse = ModularInverse(sqrtOfS, p);
					//BigInteger inverseEval = inverse.Evaluate(gnfs.PolynomialBase);
					//BigInteger inverseX = GNFSCore.BigIntegerExtensionMethods.Mod(inverseEval, p);

					compatableFields.Add(new(p, x));
				}

				if (compatableFields.Count == degree)
				{
					gnfs.SquareRoot.AlgebraicPrimes = compatableFields.Select(cf => cf.prime).ToList();
					gnfs.SquareRoot.AlgebraicResults = compatableFields.Select(cf => cf.value).ToList();

					gnfs.SquareRoot.AlgebraicSquareRootResidue = FiniteFieldArithmetic.ChineseRemainder(gnfs.N, gnfs.SquareRoot.AlgebraicPrimes, gnfs.SquareRoot.AlgebraicResults);
					yield return gnfs.SquareRoot.AlgebraicSquareRootResidue;
				}

				lastP = p;
			}
			while (true);
		}

		private static Tuple<BigInteger, BigInteger> AlgebraicSquareRoot(Polynomial f, BigInteger m, int degree, Polynomial dd, BigInteger p)
		{
			Polynomial startPolynomial = Polynomial.Field.Modulus(dd, p);
			Polynomial startInversePolynomial = ModularInverse(startPolynomial, p);

			Polynomial startSquared1 = FiniteFieldArithmetic.ModMod(Polynomial.Square(startPolynomial), f, p);
			Polynomial startSquared2 = FiniteFieldArithmetic.ModMod(Polynomial.Square(startInversePolynomial), f, p);

			Polynomial resultPoly1 = FiniteFieldArithmetic.SquareRoot(startPolynomial, f, p, degree, m);
			Polynomial resultPoly2 = ModularInverse(resultPoly1, p);

			Polynomial resultSquared1 = FiniteFieldArithmetic.ModMod(Polynomial.Square(resultPoly1), f, p);
			Polynomial resultSquared2 = FiniteFieldArithmetic.ModMod(Polynomial.Square(resultPoly2), f, p);

			bool bothResultsAgree = resultSquared1.CompareTo(resultSquared2) == 0;

			bool resultSquaredEqualsInput1 = startPolynomial.CompareTo(resultSquared1) == 0;
			bool resultSquaredEqualsInput2 = startInversePolynomial.CompareTo(resultSquared1) == 0;

			BigInteger result1 = resultPoly1.Evaluate(m).Mod(p);
			BigInteger result2 = resultPoly2.Evaluate(m).Mod(p);

			BigInteger inversePrime = p - result1;
			bool testEvaluationsAreModularInverses = inversePrime == result2;

			if (bothResultsAgree && testEvaluationsAreModularInverses)
			{
				return new Tuple<BigInteger, BigInteger>(BigInteger.Min(result1, result2), BigInteger.Max(result1, result2));
			}

			return new Tuple<BigInteger, BigInteger>(BigInteger.Zero, BigInteger.Zero);
		}

		private static Polynomial ModularInverse(Polynomial poly, BigInteger mod)
		{
			return new Polynomial(Term.GetTerms(poly.Terms.Select(trm => (mod - trm.CoEfficient).Mod(mod)).ToArray()));
		}

	}

}
