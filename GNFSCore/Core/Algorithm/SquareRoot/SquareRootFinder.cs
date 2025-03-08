using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Core.Algorithm.SquareRoot
{
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Algorithm;
	using GNFSCore.Core.Algorithm.IntegerMath;
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.RelationSieve;
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
				SquareRootFinder.CalculateRationalSide(cancelToken, selectedRelationSet, gnfs);

				if (cancelToken.IsCancellationRequested) { gnfs.LogMessage("Abort: Task canceled by user!"); break; }

				gnfs.LogMessage("SquareFinder.CalculateRationalSide() Completed.");
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Algebraic Square Root...");
				gnfs.LogMessage("                    y ∈ ℤ ...");
				gnfs.LogMessage("δ in a finite field 𝔽ᵨ(θᵨ) ...");
				gnfs.LogMessage();

				Tuple<BigInteger, BigInteger> foundFactors = SquareRootFinder.CalculateAlgebraicSide(cancelToken, gnfs);

				if (cancelToken.IsCancellationRequested) { gnfs.LogMessage("Abort: Task canceled by user!"); break; }

				gnfs.LogMessage("SquareFinder.CalculateAlgebraicSide() Completed.");

				gnfs.LogMessage();
				gnfs.LogMessage($"{sqrt.AlgebraicSquareRootResidue}² ≡ {sqrt.RationalSquareRootResidue}² (mod {sqrt.N})");
				gnfs.LogMessage();

				BigInteger P = foundFactors.Item1;
				BigInteger Q = foundFactors.Item2;

				bool nonTrivialFactorsFound = P != 1 || Q != 1;
				if (nonTrivialFactorsFound)
				{
					solutionFound = gnfs.SetFactorizationSolution(P, Q);

					gnfs.LogMessage($"Selected solution set index # {freeRelationIndex + 1}");
					gnfs.LogMessage();

					if (solutionFound)
					{
						gnfs.LogMessage("NON-TRIVIAL FACTORS FOUND!");
						gnfs.LogMessage();
						gnfs.LogMessage(gnfs.SquareRoot.ToString());
						gnfs.LogMessage();
						gnfs.LogMessage();
						gnfs.LogMessage(gnfs.Factorization.ToString());
						gnfs.LogMessage();
					}
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

		public static void CalculateRationalSide(CancellationToken cancelToken, List<Relation> relations, GNFS gnfs)
		{
			gnfs.SquareRoot.RelationsSet = relations;
			gnfs.SquareRoot.RationalNorms = gnfs.SquareRoot.RelationsSet.Select(rel => rel.RationalNorm).ToList();

			CountDictionary rationalSquareFactorization = new CountDictionary();
			foreach (var rel in gnfs.SquareRoot.RelationsSet)
			{
				rationalSquareFactorization.Combine(rel.RationalFactorization);
			}

			string rationalSquareFactorizationString = rationalSquareFactorization.FormatStringAsFactorization();

			LogFunction.Invoke("");
			LogFunction.Invoke("Rational Square Dependency:");
			LogFunction.Invoke(rationalSquareFactorizationString);

			if (cancelToken.IsCancellationRequested) { return; }

			gnfs.SquareRoot.RationalProduct = gnfs.SquareRoot.RationalNorms.Product();

			LogFunction.Invoke("");
			LogFunction.Invoke($"δᵣ = {gnfs.SquareRoot.RationalProduct} = {string.Join(" * ", gnfs.SquareRoot.RationalNorms)}");

			BigInteger RationalProductSquareRoot = gnfs.SquareRoot.RationalProduct.SquareRoot();

			var product = gnfs.SquareRoot.PolynomialDerivativeValue * RationalProductSquareRoot;

			gnfs.SquareRoot.RationalSquareRootResidue = product.Mod(gnfs.SquareRoot.N);

			LogFunction.Invoke("");
			LogFunction.Invoke($"δᵣ = {RationalProductSquareRoot}^2 = {gnfs.SquareRoot.RationalProduct}");
			LogFunction.Invoke($"χ  = {gnfs.SquareRoot.RationalSquareRootResidue} ≡ {gnfs.SquareRoot.PolynomialDerivativeValue} * {RationalProductSquareRoot} (mod {gnfs.SquareRoot.N})");
			LogFunction.Invoke("");

			gnfs.SquareRoot.IsRationalSquare = gnfs.SquareRoot.RationalProduct.IsSquare();
			if (!gnfs.SquareRoot.IsRationalSquare) // This is an error in implementation. This should never happen, and so must be a bug
			{
				throw new Exception($"{nameof(gnfs.SquareRoot.IsRationalSquare)} evaluated to false. This is a sign that there is a bug in the implementation, as this should never be the case if the algorithm has been correctly implemented.");
			}
		}

		public static Tuple<BigInteger, BigInteger> CalculateAlgebraicSide(CancellationToken cancelToken, GNFS gnfs)
		{
			gnfs.SquareRoot.RootsOfS.AddRange(gnfs.SquareRoot.RelationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

			gnfs.SquareRoot.PolynomialRingElements = new List<Polynomial>();
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

				gnfs.SquareRoot.PolynomialRingElements.Add(newPoly);
			}

			if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

			gnfs.SquareRoot.PolynomialRing = Polynomial.Product(gnfs.SquareRoot.PolynomialRingElements);
			Polynomial PolynomialRingInField = Polynomial.Field.Modulus(gnfs.SquareRoot.PolynomialRing, gnfs.SquareRoot.MonicPolynomial);


			LogFunction.Invoke("");
			LogFunction.Invoke($"∏ Sᵢ = {gnfs.SquareRoot.PolynomialRing}");
			LogFunction.Invoke("");
			LogFunction.Invoke($"∏ Sᵢ = {PolynomialRingInField}");
			LogFunction.Invoke(" in ℤ");
			LogFunction.Invoke("");

			if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

			// Multiply the product of the polynomial elements by f'(x)^2
			// This will guarantee that the square root of product of polynomials
			// is an element of the number field defined by the algebraic polynomial.
			gnfs.SquareRoot.TotalS = Polynomial.Multiply(gnfs.SquareRoot.PolynomialRing, gnfs.SquareRoot.MonicPolynomialDerivativeSquared);
			gnfs.SquareRoot.S = Polynomial.Field.Modulus(gnfs.SquareRoot.TotalS, gnfs.SquareRoot.MonicPolynomial);

			LogFunction.Invoke("");
			LogFunction.Invoke($"δᵨ = {gnfs.SquareRoot.TotalS}");
			LogFunction.Invoke($"δᵨ = {gnfs.SquareRoot.S}");
			LogFunction.Invoke(" in ℤ");

			bool solutionFound = false;

			int degree = gnfs.SquareRoot.MonicPolynomial.Degree;
			Polynomial f = gnfs.SquareRoot.MonicPolynomial;// gnfs.CurrentPolynomial;

			Func<GNFS, BigInteger> GetNextInertPrime = (g) =>
			{
				BigInteger temp = PrimeFactory.GetNextPrime(g.SquareRoot_Progress_InertPrime_LastValue + 1);
				g.SquareRoot_Progress_InertPrime_LastValue = temp;
				return temp;
			};

			BigInteger lastP = gnfs.SquareRoot_Progress_InertPrime_LastValue; //quadraticPrimes.First(); //BigInteger.Max(fromRoot, fromQuadraticFactorPairs); //N / N.ToString().Length; //((N * 3) + 1).NthRoot(3); //gnfs.QFB.Select(fp => fp.P).Max();

			List<BigInteger> primes = new List<BigInteger>();
			List<BigInteger> values = new List<BigInteger>();

			int attempts = 7;
			while (!solutionFound && attempts > 0)
			{
				if (primes.Count > 0 && values.Count > 0)
				{
					primes.Clear();
					values.Clear();
				}

				do
				{
					if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

					lastP = GetNextInertPrime(gnfs);

					Polynomial g = Polynomial.Parse($"X^{lastP} - X");
					Polynomial h = FiniteFieldArithmetic.ModMod(g, f, lastP);

					Polynomial gcd = Polynomial.Field.GCD(h, f, lastP);

					bool isIrreducible = gcd.CompareTo(Polynomial.One) == 0;
					if (!isIrreducible)
					{
						continue;
					}

					primes.Add(lastP);
				}
				while (primes.Count < degree);

				if (primes.Count > degree)
				{
					primes.Remove(primes.First());
					if (values.Count > degree)
					{
						values.Remove(values.First());
					}
				}

				BigInteger primeProduct = primes.Product();

				if (primeProduct < gnfs.SquareRoot.N)
				{
					continue;
				}

				if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); ; }

				bool takeInverse = false;
				foreach (BigInteger p in primes)
				{
					Polynomial choosenPoly = FiniteFieldArithmetic.SquareRoot(gnfs.SquareRoot.S, f, p, degree, gnfs.PolynomialBase);
					BigInteger choosenX;

					//if (takeInverse)
					//{
					//	Polynomial inverse = ModularInverse(choosenPoly, p);
					//	BigInteger inverseEval = inverse.Evaluate(gnfs.PolynomialBase);
					//	BigInteger inverseX = inverseEval.Mod(p);
					//
					//	choosenPoly = inverse;
					//	choosenX = inverseX;
					//}
					//else
					//{
					BigInteger eval = choosenPoly.Evaluate(gnfs.PolynomialBase);
					BigInteger x = eval.Mod(p);

					choosenX = x;
					//}

					values.Add(choosenX);

					LogFunction.Invoke("");
					LogFunction.Invoke($" β = {choosenPoly}");
					LogFunction.Invoke($"xi = {choosenX}");
					LogFunction.Invoke($" p = {p}");
					LogFunction.Invoke($"{primeProduct / p}");
					LogFunction.Invoke("");

					takeInverse = !takeInverse;
				}

				BigInteger commonModulus = Polynomial.Algorithms.ChineseRemainderTheorem(primes.ToArray(), values.ToArray()); //FiniteFieldArithmetic.ChineseRemainder(primes, values);
				gnfs.SquareRoot.AlgebraicSquareRootResidue = commonModulus.Mod(gnfs.SquareRoot.N);

				LogFunction.Invoke("");

				int index = -1;
				while (++index < primes.Count)
				{
					var tp = primes[index];
					var tv = values[index];

					LogFunction.Invoke($"{tp} ≡ {tv} (mod {gnfs.SquareRoot.AlgebraicSquareRootResidue})");
				}



				LogFunction.Invoke("");
				LogFunction.Invoke($"γ = {gnfs.SquareRoot.AlgebraicSquareRootResidue}"); // δ mod N 

				BigInteger algebraicSquareRoot = 1;

				BigInteger min;
				BigInteger max;
				BigInteger A;
				BigInteger B;
				BigInteger U;
				BigInteger V;
				BigInteger P = 0;
				BigInteger Q;

				if (cancelToken.IsCancellationRequested) { return new Tuple<BigInteger, BigInteger>(1, 1); }

				min = BigInteger.Min(gnfs.SquareRoot.RationalSquareRootResidue, gnfs.SquareRoot.AlgebraicSquareRootResidue);
				max = BigInteger.Max(gnfs.SquareRoot.RationalSquareRootResidue, gnfs.SquareRoot.AlgebraicSquareRootResidue);

				A = max + min;
				B = max - min;

				U = GCD.FindGCD(gnfs.SquareRoot.N, A);
				V = GCD.FindGCD(gnfs.SquareRoot.N, B);

				if (U > 1 && U != gnfs.SquareRoot.N)
				{
					P = U;
					solutionFound = true;
				}
				else if (V > 1 && V != gnfs.SquareRoot.N)
				{
					P = V;
					solutionFound = true;
				}

				if (solutionFound)
				{
					BigInteger rem;
					BigInteger other = BigInteger.DivRem(gnfs.SquareRoot.N, P, out rem);

					if (rem != 0)
					{
						solutionFound = false;
					}
					else
					{
						Q = other;
						gnfs.SquareRoot.AlgebraicResults = values;
						//AlgebraicSquareRootResidue = AlgebraicSquareRootResidue;
						gnfs.SquareRoot.AlgebraicPrimes = primes;

						return new Tuple<BigInteger, BigInteger>(P, Q);
					}
				}

				if (!solutionFound)
				{
					GNFS.LogFunction($"No solution found amongst the algebraic square roots {{ {string.Join(", ", values.Select(v => v.ToString()))} }} mod primes {{ {string.Join(", ", primes.Select(p => p.ToString()))} }}");

					attempts--;
				}
			}

			return new Tuple<BigInteger, BigInteger>(1, 1);
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
