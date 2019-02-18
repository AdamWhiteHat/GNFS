using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Matrix;
	using GNFSCore.Factors;
	using GNFSCore.Polynomials;
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

			mainForm.LogOutput();
			mainForm.LogOutput($"# of solution sets: {freeRelations.Count}");
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput($"∏ Sᵢ =");
			mainForm.LogOutput($"{squareRootFinder.TotalS}");
			mainForm.LogOutput();
			mainForm.LogOutput($"∏ Sᵢ (mod ƒ) =");
			mainForm.LogOutput($"{reducedS}");
			mainForm.LogOutput();
			mainForm.LogOutput("Polynomial ring:");
			mainForm.LogOutput($"({string.Join(") * (", squareRootFinder.PolynomialRing.Select(ply => ply.ToString()))})");
			mainForm.LogOutput();
			mainForm.LogOutput("Primes:");
			mainForm.LogOutput($"{string.Join(" * ", squareRootFinder.AlgebraicPrimes)}"); // .RelationsSet.Select(rel => rel.B).Distinct().OrderBy(relB => relB))
			mainForm.LogOutput();
			mainForm.LogOutput($"ƒ'(m)     = {squareRootFinder.PolynomialDerivative}");
			mainForm.LogOutput($"ƒ'(m)^2   = {squareRootFinder.PolynomialDerivativeSquared}");
			mainForm.LogOutput();
			mainForm.LogOutput($"γ²        = {squareRootFinder.RationalProduct} IsSquare? {squareRootFinder.RationalProduct.IsSquare()}");
			mainForm.LogOutput($"(γ  · ƒ'(m))^2 = {squareRootFinder.RationalSquare} IsSquare? {squareRootFinder.RationalSquare.IsSquare()}");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"X² / ƒ(m) = {squareRootFinder.AlgebraicProductModF}  IsSquare? {squareRootFinder.AlgebraicProductModF.IsSquare()}");
			mainForm.LogOutput();
			mainForm.LogOutput($"");
			mainForm.LogOutput($"AlgebraicPrimes: {squareRootFinder.AlgebraicPrimes.FormatString(false)}");
			mainForm.LogOutput($"AlgebraicResults: {squareRootFinder.AlgebraicResults.FormatString(false)}");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"*****************************");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"AlgebraicSquareRootResidue: {squareRootFinder.AlgebraicSquareRootResidue}");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"AlgebraicNumberFieldSquareRoots: {algebraicNumberFieldSquareRoots.FormatString(false)}");
			mainForm.LogOutput($"");
			mainForm.LogOutput($" RationalSquareRoot : {rationalSquareRoot}");
			mainForm.LogOutput($" AlgebraicSquareRoot: {algebraicSquareRoot} ");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"*****************************");
			mainForm.LogOutput($"S (x)       = {prodSmodN}  IsSquare? {prodSmodN.IsSquare()}");
			mainForm.LogOutput();
			mainForm.LogOutput("Roots of S(x):");
			mainForm.LogOutput($"{{{string.Join(", ", squareRootFinder.RootsOfS.Select(tup => (tup.Item2 > 1) ? $"{tup.Item1}/{tup.Item2}" : $"{tup.Item1}"))}}}");
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput($"TTL: {totalProdS} IsSquare? {totalProdS.IsSquare()}");
			mainForm.LogOutput($"TTL%N: {totalProdModN}  IsSquare? {totalProdModN.IsSquare()}");
			//mainForm.LogOutput();
			//mainForm.LogOutput($"∏(a + mb) = {squareRootFinder.RationalProduct}");
			//mainForm.LogOutput($"∏ƒ(a/b)   = {squareRootFinder.AlgebraicProduct}");
			mainForm.LogOutput("-------------------------------------------");
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput($"RationalSquareRootResidue:        = {squareRootFinder.RationalSquareRootResidue}   IsSquare? {squareRootFinder.RationalSquareRootResidue.IsSquare()}");
			mainForm.LogOutput($"AlgebraicSquareRootResidue:       = {squareRootFinder.AlgebraicSquareRootResidue}  IsSquare? {squareRootFinder.RationalSquareRootResidue.IsSquare()}");
			mainForm.LogOutput();
			mainForm.LogOutput();


			BigInteger min = BigInteger.Min(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);

			BigInteger A = max + min;
			BigInteger B = max - min;

			BigInteger C = GCD.FindGCD(N, A);
			BigInteger D = GCD.FindGCD(N, B);


			mainForm.LogOutput($"GCD(N, A) = {C}");
			mainForm.LogOutput($"GCD(N, B) = {D}");
			mainForm.LogOutput();
			
			return gnfs;
		}
	}
}
