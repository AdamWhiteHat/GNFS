using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.SquareRoot
{
	using ExtendedNumerics.Internal;
	using GNFSCore.Factors;
	using IntegerMath;
	using System.IO;
	using System.Net.Http.Headers;
	using System.Text.RegularExpressions;
	using static GNFSCore.GNFS;

	public partial class SquareFinder
	{
		public BigInteger RationalProduct { get; set; }
		public BigInteger RationalSquare { get; set; }
		public BigInteger RationalSquareRootResidue { get; set; }
		public bool IsRationalSquare { get; set; }

		public BigInteger AlgebraicProduct { get; set; }
		public BigInteger AlgebraicSquare { get; set; }
		public BigInteger AlgebraicProductModF { get; set; }
		public BigInteger AlgebraicSquareResidue { get; set; }
		public BigInteger AlgebraicSquareRootResidue { get; set; }
		public List<BigInteger> AlgebraicPrimes { get; set; }
		public List<BigInteger> AlgebraicResults { get; set; }

		public BigInteger N { get; set; }
		public Polynomial S { get; set; }
		public Polynomial TotalS { get; set; }
		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }
		public Polynomial PolynomialRing { get; set; }
		public List<Polynomial> PolynomialRingElements_PrimeIdeals { get; set; }

		public BigInteger PolynomialBase { get; set; }
		public Polynomial MonicPolynomial { get; set; }
		public Polynomial PolynomialDerivative { get; set; }
		public Polynomial MonicPolynomialDerivative { get; set; }

		public Polynomial PolynomialDerivativeSquared { get; set; }
		public Polynomial PolynomialDerivativeSquaredInField { get; set; }

		public BigInteger PolynomialDerivativeValue { get; set; }
		public BigInteger PolynomialDerivativeValueSquared { get; set; }

		public Polynomial MonicPolynomialDerivativeSquared { get; set; }
		public Polynomial MonicPolynomialDerivativeSquaredInField { get; set; }

		public BigInteger MonicPolynomialDerivativeValue { get; set; }
		public BigInteger MonicPolynomialDerivativeValueSquared { get; set; }

		private GNFS gnfs { get; set; }
		private List<BigInteger> rationalNorms { get; set; }
		private List<Relation> relationsSet { get; set; }

		private LogMessageDelegate LogFunction;

		public SquareFinder(GNFS sieve)
		{
			LogFunction = sieve.LogMessage;

			RationalSquareRootResidue = -1;
			RootsOfS = new List<Tuple<BigInteger, BigInteger>>();

			gnfs = sieve;
			N = gnfs.N;
			PolynomialBase = gnfs.PolynomialBase;

			PolynomialDerivative = Polynomial.GetDerivativePolynomial(gnfs.CurrentPolynomial);
			PolynomialDerivativeSquared = Polynomial.Square(PolynomialDerivative);
			PolynomialDerivativeSquaredInField = Polynomial.Field.Modulus(PolynomialDerivativeSquared, gnfs.CurrentPolynomial);

			Logging.WriteLine();
			Logging.WriteLine($"ƒ'(θ) = {PolynomialDerivative}");
			Logging.WriteLine($"ƒ'(θ)² = {PolynomialDerivativeSquared}");
			Logging.WriteLine($"ƒ'(θ)² ∈ ℤ[θ] = {PolynomialDerivativeSquaredInField}");

			PolynomialDerivativeValue = PolynomialDerivative.Evaluate(gnfs.PolynomialBase);
			PolynomialDerivativeValueSquared = BigInteger.Pow(PolynomialDerivativeValue, 2);

			Logging.WriteLine();
			Logging.WriteLine($"ƒ'(m) = {PolynomialDerivativeValue}");
			Logging.WriteLine($"ƒ'(m)² = {PolynomialDerivativeValueSquared}");


			MonicPolynomial = Polynomial.MakeMonic(gnfs.CurrentPolynomial, PolynomialBase);
			MonicPolynomialDerivative = Polynomial.GetDerivativePolynomial(MonicPolynomial);
			MonicPolynomialDerivativeSquared = Polynomial.Square(MonicPolynomialDerivative);
			MonicPolynomialDerivativeSquaredInField = Polynomial.Field.Modulus(MonicPolynomialDerivativeSquared, MonicPolynomial);

			MonicPolynomialDerivativeValue = MonicPolynomialDerivative.Evaluate(gnfs.PolynomialBase);
			MonicPolynomialDerivativeValueSquared = MonicPolynomialDerivativeSquared.Evaluate(gnfs.PolynomialBase);

			Logging.WriteLine();
			Logging.WriteLine($"MonicPolynomial: {MonicPolynomial}");
			Logging.WriteLine($"MonicPolynomialDerivative: {MonicPolynomialDerivative}");
			Logging.WriteLine($"MonicPolynomialDerivativeSquared: {MonicPolynomialDerivativeSquared}");
			Logging.WriteLine($"MonicPolynomialDerivativeSquaredInField: {MonicPolynomialDerivativeSquaredInField}");
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

				gnfs.LogMessage();
				gnfs.LogMessage($"Selected solution set index # {freeRelationIndex + 1}");
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Rational Square Root β ∈ ℤ[θ] ...");
				gnfs.LogMessage();
				BigInteger rationalSquareRoot = squareRootFinder.CalculateRationalSide(cancelToken, selectedRelationSet);

				if (cancelToken.IsCancellationRequested) { gnfs.LogMessage("Abort: Task canceled by user!"); break; }

				gnfs.LogMessage("SquareFinder.CalculateRationalSide() Completed.");
				gnfs.LogMessage();
				gnfs.LogMessage("Calculating Algebraic Square Root...");
				gnfs.LogMessage("                    y ∈ ℤ ...");
				gnfs.LogMessage("δ in a finite field 𝔽ᵨ(θᵨ) ...");
				gnfs.LogMessage();

				foreach (BigInteger algebraicSquareRoot in squareRootFinder.CalculateAlgebraicSide(cancelToken))
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
					gnfs.LogMessage($"{squareRootFinder.AlgebraicSquareRootResidue}² ≡ {squareRootFinder.RationalSquareRootResidue}² (mod {squareRootFinder.N})");
					gnfs.LogMessage();


					gnfs.LogMessage("NON-TRIVIAL FACTORS FOUND!");
					gnfs.LogMessage();
					gnfs.LogMessage(squareRootFinder.ToString());
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

		public BigInteger CalculateRationalSide(CancellationToken cancelToken, List<Relation> relations)
		{
			relationsSet = relations;
			rationalNorms = relationsSet.Select(rel => rel.RationalNorm).ToList();

			CountDictionary rationalSquareFactorization = new CountDictionary();
			foreach (var rel in relationsSet)
			{
				rationalSquareFactorization.Combine(rel.RationalFactorization);
			}

			string rationalSquareFactorizationString = rationalSquareFactorization.FormatStringAsFactorization();

			Logging.WriteLine();
			Logging.WriteLine("Rational Square Dependency:");
			Logging.WriteLine(rationalSquareFactorizationString);

			if (cancelToken.IsCancellationRequested) { return -1; }

			RationalProduct = rationalNorms.Product();
			RationalSquare = RationalProduct;

			Logging.WriteLine();
			Logging.WriteLine($"δᵣ = {RationalProduct} = {string.Join(" * ", rationalNorms)}");

			BigInteger RationalProductSquareRoot = RationalProduct.SquareRoot();

			var product = PolynomialDerivativeValue * RationalProductSquareRoot;

			RationalSquareRootResidue = product.Mod(N);

			Logging.WriteLine();
			Logging.WriteLine($"δᵣ = {RationalProductSquareRoot}^2 = {RationalProduct}");
			Logging.WriteLine($"χ  = {RationalSquareRootResidue} ≡ {PolynomialDerivativeValue} * {RationalProductSquareRoot} (mod {N})");
			Logging.WriteLine();

			IsRationalSquare = RationalProduct.IsSquare();
			if (!IsRationalSquare) // This is an error in implementation. This should never happen, and so must be a bug
			{
				throw new Exception($"{nameof(IsRationalSquare)} evaluated to false. This is a sign that there is a bug in the implementation, as this should never be the case if the algorithm has been correctly implemented.");
			}

			return RationalSquareRootResidue;
		}

		public IEnumerable<BigInteger> CalculateAlgebraicSide(CancellationToken cancelToken)
		{
			RootsOfS.AddRange(relationsSet.Select(rel => new Tuple<BigInteger, BigInteger>(rel.A, rel.B)));

			if (cancelToken.IsCancellationRequested) { yield break; }

			PolynomialRingElements_PrimeIdeals = new List<Polynomial>();
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

				PolynomialRingElements_PrimeIdeals.Add(newPoly);
			}

			if (cancelToken.IsCancellationRequested) { yield break; }

			PolynomialRing = Polynomial.Product(PolynomialRingElements_PrimeIdeals);
			Polynomial PolynomialRingInField = Polynomial.Field.Modulus(PolynomialRing, MonicPolynomial);

			AlgebraicProduct = PolynomialRing.Evaluate(gnfs.PolynomialBase);
			AlgebraicProductModF = PolynomialRingInField.Evaluate(gnfs.PolynomialBase);

			Logging.WriteLine();
			Logging.WriteLine($"∏ Sᵢ = {PolynomialRing}");
			Logging.WriteLine();
			Logging.WriteLine($"∏ Sᵢ = {PolynomialRingInField}");
			Logging.WriteLine(" in ℤ");
			Logging.WriteLine();

			if (cancelToken.IsCancellationRequested) { yield break; }

			// Multiply the product of the polynomial elements by f'(x)^2
			// This will guarantee that the square root of product of polynomials
			// is an element of the number field defined by the algebraic polynomial.
			TotalS = Polynomial.Multiply(PolynomialRing, MonicPolynomialDerivativeSquared);
			S = Polynomial.Field.Modulus(TotalS, MonicPolynomial);

			AlgebraicSquare = TotalS.Evaluate(gnfs.PolynomialBase);
			AlgebraicSquareResidue = S.Evaluate(gnfs.PolynomialBase);

			Logging.WriteLine();
			Logging.WriteLine($"δᵨ = {TotalS}");
			Logging.WriteLine($"δᵨ = {S}");
			Logging.WriteLine(" in ℤ");

			int degree = MonicPolynomial.Degree;
			Polynomial f = MonicPolynomial;// gnfs.CurrentPolynomial;

			BigInteger lastP = gnfs.QuadraticFactorPairCollection.Last().P; //quadraticPrimes.First(); //BigInteger.Max(fromRoot, fromQuadraticFactorPairs); //N / N.ToString().Length; //((N * 3) + 1).NthRoot(3); //gnfs.QFB.Select(fp => fp.P).Max();

			lastP = BigInteger.Max(lastP, N.SquareRoot() * 2);

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
					Polynomial sqrtOfS = FiniteFieldArithmetic.SquareRoot(S, gnfs.CurrentPolynomial, p, gnfs.PolynomialDegree, gnfs.PolynomialBase);

					BigInteger eval = sqrtOfS.Evaluate(gnfs.PolynomialBase);
					BigInteger x = GNFSCore.BigIntegerExtensionMethods.Mod(eval, p);

					//Polynomial inverse = ModularInverse(sqrtOfS, p);
					//BigInteger inverseEval = inverse.Evaluate(gnfs.PolynomialBase);
					//BigInteger inverseX = GNFSCore.BigIntegerExtensionMethods.Mod(inverseEval, p);

					compatableFields.Add(new(p, x));
				}

				if (compatableFields.Count == degree)
				{
					AlgebraicPrimes = compatableFields.Select(cf => cf.prime).ToList();
					AlgebraicResults = compatableFields.Select(cf => cf.value).ToList();

					AlgebraicSquareRootResidue = FiniteFieldArithmetic.ChineseRemainder(N, AlgebraicPrimes, AlgebraicResults);
					yield return AlgebraicSquareRootResidue;
				}

				lastP = p;
			}
			while (true);

			yield break;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Polynomial ring:");
			result.AppendLine($"({string.Join(") * (", PolynomialRingElements_PrimeIdeals.Select(ply => ply.ToString()))})");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ =");
			result.AppendLine($"{PolynomialRing}");
			result.AppendLine();
			result.AppendLine($"ƒ         = {gnfs.CurrentPolynomial}");
			result.AppendLine($"ƒ(m)      = {MonicPolynomial}");
			result.AppendLine($"ƒ'(m)     = {MonicPolynomialDerivative}");
			result.AppendLine($"ƒ'(m)^2   = {MonicPolynomialDerivativeSquared}");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ(m)  *  ƒ'(m)² =");
			result.AppendLine($"{TotalS}");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ(m)  *  ƒ'(m)² (mod ƒ) =");
			result.AppendLine($"{S}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Rational:");
			result.AppendLine($"γ² = √(  Sᵣ(m)  *  ƒ'(m)²  )");
			result.AppendLine($"γ² = √( {RationalProduct} * {PolynomialDerivativeValueSquared} )");
			result.AppendLine($"γ² = √( {RationalSquare} )");
			result.AppendLine($"IsRationalSquare  ? {IsRationalSquare}");
			result.AppendLine($"γ  =    {RationalSquareRootResidue} mod N"); // δ mod N 
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Algebraic:");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicProduct} * {PolynomialDerivativeValue}");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicSquare}");
			result.AppendLine($"IsAlgebraicSquare ? {AlgebraicSquare.IsSquare()}");
			result.AppendLine();
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareRootResidue}");
			result.AppendLine();
			result.AppendLine($"X² / ƒ(m) = {AlgebraicProductModF}  IsSquare? {AlgebraicProductModF.IsSquare()}");
			result.AppendLine($"S (x)       = {AlgebraicSquareResidue}  IsSquare? {AlgebraicSquareResidue.IsSquare()}");
			result.AppendLine();
			result.AppendLine($"AlgebraicResults:");
			result.AppendLine($"{AlgebraicResults.FormatString(false)}");
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
			result.AppendLine($"∏(a + mb) = {RationalProduct}");
			result.AppendLine($"∏ƒ(a/b)   = {AlgebraicProduct}");
			result.AppendLine();

			BigInteger min = BigInteger.Min(RationalSquareRootResidue, AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(RationalSquareRootResidue, AlgebraicSquareRootResidue);

			BigInteger add = max + min;
			BigInteger sub = max - min;

			BigInteger gcdAdd = GCD.FindGCD(N, add);
			BigInteger gcdSub = GCD.FindGCD(N, sub);

			BigInteger answer = BigInteger.Max(gcdAdd, gcdSub);

			if (gnfs.IsFactored)
			{
				result.AppendLine();
				result.AppendLine($"Solution? YES");

				if (answer != 1)
				{
					result.AppendLine();
					result.AppendLine();
					result.AppendLine("*********************");
					result.AppendLine();
					result.AppendLine($" P = {gnfs.Factorization.P} ");
					result.AppendLine($" Q = {gnfs.Factorization.Q} ");
					result.AppendLine();
					result.AppendLine("*********************");
					result.AppendLine();
					result.AppendLine();
				}
			}

			result.AppendLine();
			return result.ToString();
		}
	}
}
