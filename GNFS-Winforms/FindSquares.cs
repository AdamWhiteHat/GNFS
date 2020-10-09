using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.SquareRoot;

	public partial class GnfsUiBridge
	{
		private static List<int> triedFreeRelationIndices = new List<int>();

		public static GNFS FindSquares(CancellationToken cancelToken, GNFS gnfs)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			Logging.LogMessage();
			Logging.LogMessage($"# of solution sets: {gnfs.CurrentRelationsProgress.FreeRelations.Count}");
			Logging.LogMessage();


			BigInteger polyBase = gnfs.PolynomialBase;
			List<List<Relation>> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations;

			int freeRelationIndex = 0;
			bool solutionFound = false;

			// Below randomly selects a solution set to try and find a square root of the polynomial in.

			// Each time this step is stopped and restarted, it will try a different solution set.
			// Previous used sets are tracked with the List<int> triedFreeRelationIndices

			while (!solutionFound)
			{
				if (triedFreeRelationIndices.Count == freeRelations.Count) // If we have exhausted our solution sets, alert the user. Number wont factor for some reason.
				{
					Logging.LogMessage("ERROR: ALL RELATION SETS HAVE BEEN TRIED...?");
					Logging.LogMessage($"If the number of solution sets ({freeRelations.Count}) is low, you may need to sieve some more and then re-run the matrix solving step.");
					Logging.LogMessage("If there are many solution sets, and you have tried them all without finding non-trivial factors, then something is wrong...");
					Logging.LogMessage();
					return gnfs;
				}

				do
				{
					freeRelationIndex = StaticRandom.Next(0, freeRelations.Count);
				}
				while (triedFreeRelationIndices.Contains(freeRelationIndex));

				triedFreeRelationIndices.Add(freeRelationIndex); // Add current selection to our list

				List<Relation> selectedRelationSet = freeRelations[freeRelationIndex]; // Get the solution set

				SquareFinder squareRootFinder = new SquareFinder(gnfs, selectedRelationSet); // If you want to solve for a new solution set, create a new instance

				Logging.LogMessage($"Selected solution set # {freeRelationIndex + 1}");
				Logging.LogMessage();
				Logging.LogMessage($"Selected set (a,b) pairs (count: {selectedRelationSet.Count}): {string.Join(" ", selectedRelationSet.Select(rel => $"({rel.A},{rel.B})"))}");
				Logging.LogMessage();
				Logging.LogMessage();
				Logging.LogMessage();
				Logging.LogMessage($"ƒ'(m)     = {squareRootFinder.PolynomialDerivative}");
				Logging.LogMessage($"ƒ'(m)^2   = {squareRootFinder.PolynomialDerivativeSquared}");
				Logging.LogMessage();
				Logging.LogMessage("Calculating Rational Square Root.");
				Logging.LogMessage("Please wait...");

				squareRootFinder.CalculateRationalSide();

				Logging.LogMessage("Completed.");
				Logging.LogMessage();
				Logging.LogMessage($"γ²        = {squareRootFinder.RationalProduct} IsSquare? {squareRootFinder.RationalProduct.IsSquare()}");
				Logging.LogMessage($"(γ  · ƒ'(m))^2 = {squareRootFinder.RationalSquare} IsSquare? {squareRootFinder.RationalSquare.IsSquare()}");
				Logging.LogMessage();
				Logging.LogMessage();
				Logging.LogMessage("Calculating Algebraic Square Root.");
				Logging.LogMessage("Please wait...");

				Tuple<BigInteger, BigInteger> foundFactors = squareRootFinder.CalculateAlgebraicSide(cancelToken);
				BigInteger P = foundFactors.Item1;
				BigInteger Q = foundFactors.Item2;

				if (cancelToken.IsCancellationRequested && P == 1 && Q == 1)
				{
					Logging.LogMessage("Square root search aborted!");
					return gnfs;
				}

				bool nonTrivialFactorsFound = (P != 1 || Q != 1);
				if (!nonTrivialFactorsFound)
				{
					Logging.LogMessage();
					Logging.LogMessage("Unable to locate a square root in solution set!");
					Logging.LogMessage();
					Logging.LogMessage("Trying a different solution set...");
					Logging.LogMessage();
					continue;
				}
				else
				{
					solutionFound = true;
				}

				Logging.LogMessage("NON-TRIVIAL FACTORS FOUND!");
				Logging.LogMessage();

				Polynomial S = squareRootFinder.S;
				Polynomial SRingSquare = squareRootFinder.SRingSquare;

				BigInteger prodS = SRingSquare.Evaluate(polyBase);

				Polynomial reducedS = Polynomial.Field.Modulus(S, gnfs.N);

				BigInteger totalProdS = squareRootFinder.TotalS.Evaluate(polyBase) * squareRootFinder.PolynomialDerivative;
				BigInteger totalProdModN = totalProdS % gnfs.N;
				BigInteger prodSmodN = prodS % gnfs.N;

				List<BigInteger> algebraicNumberFieldSquareRoots = squareRootFinder.AlgebraicResults;

				BigInteger rationalSquareRoot = squareRootFinder.RationalSquareRootResidue;
				BigInteger algebraicSquareRoot = squareRootFinder.AlgebraicSquareRootResidue;


				Logging.LogMessage($"∏ Sᵢ =");
				Logging.LogMessage($"{squareRootFinder.TotalS}");
				Logging.LogMessage();
				Logging.LogMessage($"∏ Sᵢ (mod ƒ) =");
				Logging.LogMessage($"{reducedS}");
				Logging.LogMessage();
				Logging.LogMessage("Polynomial ring:");
				Logging.LogMessage($"({string.Join(") * (", squareRootFinder.PolynomialRing.Select(ply => ply.ToString()))})");
				Logging.LogMessage();
				Logging.LogMessage("Primes:");
				Logging.LogMessage($"{string.Join(" * ", squareRootFinder.AlgebraicPrimes)}"); // .RelationsSet.Select(rel => rel.B).Distinct().OrderBy(relB => relB))
				Logging.LogMessage();
				Logging.LogMessage();
				Logging.LogMessage($"X² / ƒ(m) = {squareRootFinder.AlgebraicProductModF}  IsSquare? {squareRootFinder.AlgebraicProductModF.IsSquare()}");
				Logging.LogMessage();
				Logging.LogMessage($"");
				Logging.LogMessage($"AlgebraicPrimes: {squareRootFinder.AlgebraicPrimes.FormatString(false)}");
				Logging.LogMessage($"AlgebraicResults: {squareRootFinder.AlgebraicResults.FormatString(false)}");
				Logging.LogMessage($"");
				Logging.LogMessage($"*****************************");
				Logging.LogMessage($"");
				Logging.LogMessage($"AlgebraicSquareRootResidue: {squareRootFinder.AlgebraicSquareRootResidue}");
				Logging.LogMessage($"");
				Logging.LogMessage($"AlgebraicNumberFieldSquareRoots: {algebraicNumberFieldSquareRoots.FormatString(false)}");
				Logging.LogMessage($"");
				Logging.LogMessage($" RationalSquareRoot : {rationalSquareRoot}");
				Logging.LogMessage($" AlgebraicSquareRoot: {algebraicSquareRoot} ");
				Logging.LogMessage($"");
				Logging.LogMessage($"*****************************");
				Logging.LogMessage($"S (x)       = {prodSmodN}  IsSquare? {prodSmodN.IsSquare()}");
				Logging.LogMessage();
				Logging.LogMessage("Roots of S(x):");
				Logging.LogMessage($"{{{string.Join(", ", squareRootFinder.RootsOfS.Select(tup => (tup.Item2 > 1) ? $"{tup.Item1}/{tup.Item2}" : $"{tup.Item1}"))}}}");
				Logging.LogMessage();
				Logging.LogMessage();
				Logging.LogMessage($"TTL: {totalProdS} IsSquare? {totalProdS.IsSquare()}");
				Logging.LogMessage($"TTL%N: {totalProdModN}  IsSquare? {totalProdModN.IsSquare()}");
				//Logging.LogMessage();
				//Logging.LogMessage($"∏(a + mb) = {squareRootFinder.RationalProduct}");
				//Logging.LogMessage($"∏ƒ(a/b)   = {squareRootFinder.AlgebraicProduct}");
				Logging.LogMessage("-------------------------------------------");
				Logging.LogMessage();
				Logging.LogMessage();
				Logging.LogMessage($"RationalSquareRootResidue:        = {squareRootFinder.RationalSquareRootResidue}   IsSquare? {squareRootFinder.RationalSquareRootResidue.IsSquare()}");
				Logging.LogMessage($"AlgebraicSquareRootResidue:       = {squareRootFinder.AlgebraicSquareRootResidue}  IsSquare? {squareRootFinder.RationalSquareRootResidue.IsSquare()}");
				Logging.LogMessage();
				Logging.LogMessage();

				/*	Non-trivial factors also recoverable by doing the following:

				BigInteger min = BigInteger.Min(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);
				BigInteger max = BigInteger.Max(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);
				BigInteger A = max + min;
				BigInteger B = max - min;
				BigInteger P = GCD.FindGCD(gnfs.N, A);
				BigInteger Q = GCD.FindGCD(gnfs.N, B);

				*/

				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"N = {gnfs.N}");
				sb.AppendLine();
				sb.AppendLine($"P = {BigInteger.Max(P, Q)}");
				sb.AppendLine($"Q = {BigInteger.Min(P, Q)}");
				sb.AppendLine();

				string path = Path.Combine(gnfs.SaveLocations.SaveDirectory, "Solution.txt");

				File.WriteAllText(path, sb.ToString());

				Logging.LogMessage();
				Logging.LogMessage(Environment.NewLine + sb.ToString());
			}

			return gnfs;
		}
	}
}
