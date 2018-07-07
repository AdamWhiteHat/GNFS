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
	using GNFSCore.Polynomial;
	using GNFSCore.Factors;
	using GNFSCore.IntegerMath;
	using GNFSCore.Matrix;
	using GNFSCore.Polynomial.Internal;
	using GNFSCore.SquareRoot;

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
			IPolynomial poly = gnfs.CurrentPolynomial;
			BigInteger polyBase = gnfs.PolynomialBase;

			Func<BigInteger, BigInteger> f = poly.Evaluate;
			Func<BigInteger, BigInteger> fD = poly.Derivative;

			List<List<Relation>> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations;

			mainForm.LogOutput();
			mainForm.LogOutput($"# of solution sets: {freeRelations.Count}");
			mainForm.LogOutput();

			foreach (List<Relation> relationSet in freeRelations)
			{
				SquareFinder squareRootFinder = new SquareFinder(gnfs, relationSet);

				squareRootFinder.CalculateRationalSide();
				squareRootFinder.CalculateAlgebraicSide();

				List<BigInteger> qNorms = squareRootFinder.QNorms;
				BigInteger qNormsProduct = squareRootFinder.QNorms.Product();
				BigInteger qNormsProductModN = qNormsProduct % N;
				
				
				IPolynomial S = squareRootFinder.S;

				BigInteger prodS = S.Evaluate(polyBase);

				IPolynomial reducedS = CommonPolynomial.Modulus(S, N);

				BigInteger reducedProdS = (reducedS.Evaluate(polyBase) * squareRootFinder.PolynomialDerivative) % N;

				BigInteger totalProdS = squareRootFinder.TotalS.Evaluate(polyBase) * squareRootFinder.PolynomialDerivative;
				BigInteger totalProdModN = totalProdS % N;

				BigInteger X = prodS % N;
				BigInteger Y = squareRootFinder.RationalSquareRootResidue;


				BigInteger[] elements = new BigInteger[]
				{
					X, Y, prodS, totalProdS, reducedProdS, totalProdModN, qNormsProduct, qNormsProductModN,
					squareRootFinder.AlgebraicProduct, squareRootFinder.RationalProduct,
					squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicProductModF
				};

				foreach (BigInteger a in elements)
				{
					foreach (BigInteger b in elements)
					{
						if (a == b) continue;

						BigInteger gcdAdd = GCD.FindGCD(N, a + b);
						BigInteger gcdSub = GCD.FindGCD(N, a - b);

						if (DoesGcdFactor(gcdAdd) || DoesGcdFactor(gcdSub))
						{

							mainForm.LogOutput();
							mainForm.LogOutput($"∏ Sᵢ =");
							mainForm.LogOutput($"{squareRootFinder.TotalS}");
							//mainForm.LogOutput();
							//mainForm.LogOutput($"∏ Sᵢ (mod ƒ) =");
							//mainForm.LogOutput($"{reducedTotalS}");
							mainForm.LogOutput();
							mainForm.LogOutput("Polynomial ring:");
							mainForm.LogOutput($"({string.Join(") * (", squareRootFinder.PolynomialRing.Select(ply => ply.ToString()))})");
							mainForm.LogOutput();
							mainForm.LogOutput("Primes:");
							mainForm.LogOutput($"{string.Join(" * ", squareRootFinder.RelationsSet.Select(rel => rel.B).Distinct().OrderBy(relB => relB))}");
							mainForm.LogOutput();
							mainForm.LogOutput();
							mainForm.LogOutput("Q norms:");
							mainForm.LogOutput($"{String.Join(" * ", qNorms.Select(n => $"{n}"))}");
							mainForm.LogOutput($"= {qNormsProduct} IsSquare? {qNormsProduct.IsSquare()}");
							mainForm.LogOutput($"% N = {qNormsProductModN} IsSquare? {qNormsProductModN.IsSquare()}");
							mainForm.LogOutput();
							mainForm.LogOutput();
							mainForm.LogOutput($"ƒ'(m)     = {squareRootFinder.PolynomialDerivative}");
							mainForm.LogOutput($"ƒ'(m)^2   = {squareRootFinder.PolynomialDerivativeSquared}");
							mainForm.LogOutput();
							mainForm.LogOutput($"γ²        = {squareRootFinder.RationalProduct} IsSquare? {squareRootFinder.RationalProduct.IsSquare()}");
							mainForm.LogOutput($"(γ  · ƒ'(m))^2 = {squareRootFinder.RationalSquare} IsSquare? {squareRootFinder.RationalSquare.IsSquare()}");
							mainForm.LogOutput($"");
							mainForm.LogOutput($"");
							mainForm.LogOutput($"γ² / ƒ(m) = {squareRootFinder.AlgebraicProductModF}");
							mainForm.LogOutput($"γ         = {squareRootFinder.RationalSquareRootResidue}  IsSquare? {squareRootFinder.RationalSquareRootResidue.IsSquare()}");
							mainForm.LogOutput();
							mainForm.LogOutput($"S(a,b)    = {S}");
							mainForm.LogOutput($"S % ƒ     = {reducedS}");
							mainForm.LogOutput($"S % ƒ % N = {reducedProdS}  IsSquare? {reducedProdS.IsSquare()}");
							mainForm.LogOutput($"Sₐ(m)     = {prodS} IsSquare? {prodS.IsSquare()}");
							mainForm.LogOutput($"S (x)     = {X}  IsSquare? {X.IsSquare()}");//{squareRootFinder.AlgebraicSquareResidue}");
							mainForm.LogOutput();
							mainForm.LogOutput("Roots of S(x):");
							mainForm.LogOutput($"{{{string.Join(", ", squareRootFinder.RootsOfS.Select(tup => (tup.Item2 > 1) ? $"{tup.Item1}/{tup.Item2}" : $"{tup.Item1}"))}}}");
							mainForm.LogOutput();
							mainForm.LogOutput();
							mainForm.LogOutput($"TTL: {totalProdS} IsSquare? {totalProdS.IsSquare()}");
							mainForm.LogOutput($"TTL%N: {totalProdModN}  IsSquare? {totalProdModN.IsSquare()}");
							mainForm.LogOutput();
							mainForm.LogOutput($"∏(a + mb) = {squareRootFinder.RationalProduct}");
							mainForm.LogOutput($"∏ƒ(a/b)   = {squareRootFinder.AlgebraicProduct}");


							mainForm.LogOutput();
							mainForm.LogOutput("Add GCD: " + gcdAdd);
							mainForm.LogOutput("Sub GCD: " + gcdSub);
							mainForm.LogOutput();
							mainForm.LogOutput("################");
							mainForm.LogOutput("##  SUCCESS!  ##");
							mainForm.LogOutput("################");
							mainForm.LogOutput();

							return gnfs;
						}
					}
				}
			}

			return gnfs;
		}

		private bool DoesGcdFactor(BigInteger gcd)
		{
			return (gcd != 1 && gcd != N);
		}
	}
}
