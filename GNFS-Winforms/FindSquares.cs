using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Interfaces;
	using GNFSCore.SquareRoot;
	using GNFSCore.IntegerMath;

	public partial class GnfsUiBridge
	{
		private BigInteger N = -1;

		public GNFS FindSquares(GNFS gnfs, CancellationToken cancelToken)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			N = gnfs.N;
			BigInteger polyBase = gnfs.PolynomialBase;

			List<List<Relation>> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations;


			SquareFinder squareRootFinder = new SquareFinder(gnfs, freeRelations.First()); // relationSet

			squareRootFinder.CalculateRationalSide();
			squareRootFinder.CalculateAlgebraicSide();

			IPolynomial S = squareRootFinder.S;
			IPolynomial SRingSquare = squareRootFinder.SRingSquare;

			BigInteger prodS = SRingSquare.Evaluate(polyBase);

			IPolynomial reducedS = Polynomial.Modulus(S, N);

			BigInteger totalProdS = squareRootFinder.TotalS.Evaluate(polyBase) * squareRootFinder.PolynomialDerivative;
			BigInteger totalProdModN = totalProdS % N;
			BigInteger prodSmodN = prodS % N;

			List<BigInteger> algebraicNumberFieldSquareRoots = squareRootFinder.AlgebraicResults;

			BigInteger rationalSquareRoot = squareRootFinder.RationalSquareRootResidue;
			BigInteger algebraicSquareRoot = squareRootFinder.AlgebraicSquareRootResidue;

			Logging.LogMessage();
			Logging.LogMessage($"# of solution sets: {freeRelations.Count}");
			Logging.LogMessage();
			Logging.LogMessage();
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
			Logging.LogMessage($"ƒ'(m)     = {squareRootFinder.PolynomialDerivative}");
			Logging.LogMessage($"ƒ'(m)^2   = {squareRootFinder.PolynomialDerivativeSquared}");
			Logging.LogMessage();
			Logging.LogMessage($"γ²        = {squareRootFinder.RationalProduct} IsSquare? {squareRootFinder.RationalProduct.IsSquare()}");
			Logging.LogMessage($"(γ  · ƒ'(m))^2 = {squareRootFinder.RationalSquare} IsSquare? {squareRootFinder.RationalSquare.IsSquare()}");
			Logging.LogMessage($"");
			Logging.LogMessage($"");
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


			BigInteger min = BigInteger.Min(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);

			BigInteger A = max + min;
			BigInteger B = max - min;

			BigInteger C = GCD.FindGCD(N, A);
			BigInteger D = GCD.FindGCD(N, B);


			Logging.LogMessage($"GCD(N, A) = {C}");
			Logging.LogMessage($"GCD(N, B) = {D}");
			Logging.LogMessage();

			return gnfs;
		}
	}
}
