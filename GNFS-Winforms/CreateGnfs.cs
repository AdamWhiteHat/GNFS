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
	using GNFSCore.FactorBase;
	using GNFSCore.IntegerMath;
	using GNFSCore.PrimeSignature;
	using GNFSCore.Polynomial.Internal;

	public partial class GnfsUiBridge
	{
		private MainForm mainForm;

		public GnfsUiBridge(MainForm form)
		{
			mainForm = form;
		}

		public GNFS CreateGnfs(BigInteger n, BigInteger polyBase, int degree, CancellationToken cancelToken)
		{
			GNFS gnfs = null;

			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			if (gnfs == null)
			{
				gnfs = new GNFS(cancelToken, n, polyBase, degree);
			}

			if (gnfs.SmoothRelations.Any())
			{

				mainForm.BridgeButtonSquares.SetControlEnabledState(true);
			}

			mainForm.BridgeButtonBound.SetControlText(gnfs.PrimeBound.ToString());

			mainForm.LogOutput($"N = {gnfs.N}");
			mainForm.LogOutput();

			mainForm.LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			mainForm.LogOutput(gnfs.CurrentPolynomial.ToString());
			mainForm.LogOutput();


			mainForm.LogOutput("Prime Factor Base Bounds:");
			mainForm.LogOutput($"PrimeBound         : {gnfs.PrimeBound}");
			mainForm.LogOutput($"RationalFactorBase : {gnfs.RationalFactorBase}");
			mainForm.LogOutput($"AlgebraicFactorBase: {gnfs.AlgebraicFactorBase}");
			mainForm.LogOutput($"QuadraticPrimeBase : {gnfs.QuadraticPrimeBase.Last()}");
			mainForm.LogOutput();

			mainForm.LogOutput($"Rational Factor Base (RFB):");
			mainForm.LogOutput(gnfs.RFB.ToString(20));
			mainForm.LogOutput();

			mainForm.LogOutput($"Algebraic Factor Base (AFB):");
			mainForm.LogOutput(gnfs.AFB.ToString(20));
			mainForm.LogOutput();

			mainForm.LogOutput($"Quadratic Factor Base (QFB):");
			mainForm.LogOutput(gnfs.QFB.ToString(20));
			mainForm.LogOutput();

			mainForm.BridgeButtonRelation.SetControlEnabledState(true);
			mainForm.BridgeButtonRelation.SetControlText(MainForm.CancelButtonText);
			mainForm.BridgeButtonGnfs.SetControlEnabledState(false);

			// valueRange & quantity 
			IEnumerable<Relation> smoothRelations = gnfs.GenerateRelations(cancelToken);//, quantity);

			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			mainForm.BridgeButtonSquares.SetControlEnabledState(true);

			//IEnumerable<Relation> zeroRelations = smoothRelations.Where(r => r.D == 0);
			//zeroRelations = zeroRelations.Where(r => r.G == 0);

			mainForm.LogOutput($"Smooth relations:");
			mainForm.LogOutput("\t_______________________________________________");
			mainForm.LogOutput($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tQuantity: {smoothRelations.Count()} Target quantity: {(gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count() + 1).ToString()}"/* Search range: -{relationsRange} to {relationsRange}"*/);
			mainForm.LogOutput("\t```````````````````````````````````````````````");
			//mainForm.LogOutput( string.Join(Environment.NewLine, smoothRelations.Select(rel => $"{rel.A},{rel.B}")));
			mainForm.LogOutput(smoothRelations.OrderByDescending(rel => rel.A * rel.B).Take(5).FormatString());
			mainForm.LogOutput("(restricted result set to top 5)");
			mainForm.LogOutput();
			/*
			var roughGroups = GNFS.GroupRoughNumbers(gnfs.RoughRelations);
			if (roughGroups.Any())
			{
				List<Relation> newRelations = GNFS.MultiplyLikeRoughNumbers(gnfs, roughGroups);
				Tuple<List<Relation>, List<RoughPair>> newSievedRelations = GNFS.SieveRelations(gnfs, newRelations);

				var stillRough = newSievedRelations.Item2;
				var newSmooth = newSievedRelations.Item1;

				int max = roughGroups.Count;
				int c2 = newRelations.Count;
				int c3 = stillRough.Count();

				max = Math.Min(max, Math.Min(c2, c3));

				mainForm.LogOutput($"COUNT: {roughGroups.Count} ({newSmooth.Count}) / {roughGroups.Count} / {gnfs.RoughRelations.Count}");
				mainForm.LogOutput();

				max = 4;

				int counter = 0;
				while (counter < max)
				{
					mainForm.LogOutput(
						$"{string.Join(" ; ", roughGroups[counter].Select(rg => rg.ToString()))}" + Environment.NewLine +
						$"{newRelations[counter]}" + Environment.NewLine +
						$"{stillRough[counter]}" + Environment.NewLine + Environment.NewLine
					);
					counter++;
				}

				//mainForm.LogOutput($"Rough numbers (Relations with remainders, i.e. not fully factored)");
				//mainForm.LogOutput($"Count: {roughGroups.Count}");
				//mainForm.LogOutput(roughGroups.FormatString());
				//mainForm.LogOutput();

				//mainForm.LogOutput($"New relations (Like rough relations multiplied together)");
				//mainForm.LogOutput($"Count: {newRelations.Count}");
				//mainForm.LogOutput(newRelations.FormatString());
				//mainForm.LogOutput();

				mainForm.LogOutput($"New smooth relations (from sieving rough relations)");
				mainForm.LogOutput($"Count: {newSmooth.Count}");
				mainForm.LogOutput(newSmooth.Take(5).FormatString());
				mainForm.LogOutput();
			}
			//mainForm.LogOutput($"Still rough relations (from sieving rough relations)");
			//mainForm.LogOutput($"Count: {newSievedRelations.Item2.Count}");
			//mainForm.LogOutput(newSievedRelations.Item2.FormatString());
			mainForm.LogOutput();
			mainForm.LogOutput();
			*/

			/*
			FactorCollection gFactors = FactorCollection.Factory.BuildGFactorBase(gnfs);

			mainForm.LogOutput($"g(x) factors: {gFactors.Count}");
			mainForm.LogOutput(gFactors.FormatString());
			mainForm.LogOutput();
			*/

			/*
			var matrixVectors = smoothRelations.Select(rel => rel.GetMatrixRowVector());
			var rationalVectors = matrixVectors.Select(tup => tup.Item1);
			var algebraicVectors = matrixVectors.Select(tup => tup.Item2);
			
			BitMatrix rationalVectorsMatrix = new BitMatrix(rationalVectors);
			BitMatrix algebraicVectorsMatrix = new BitMatrix(algebraicVectors);
			
			IEnumerable<BigInteger[]> rationalCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			IEnumerable<BigInteger[]> algebraicCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			
			mainForm.LogOutput($"*** BINARY MATRIX ***");
			mainForm.LogOutput();
			mainForm.LogOutput("Rational matrix:");
			mainForm.LogOutput(rationalVectorsMatrix.ToString());
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput("Algebraic matrix:");
			mainForm.LogOutput(algebraicVectorsMatrix.ToString());
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput("Rational squares:");
			mainForm.LogOutput(MatrixSolver.FormatCombinations(rationalCombinations));
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput("Algebraic squares:");
			mainForm.LogOutput(MatrixSolver.FormatCombinations(algebraicCombinations));
			mainForm.LogOutput();
			mainForm.LogOutput();

			//List<BigInteger> squares = MatrixSolver.GetCombinationsProduct(rationalCombinations);
			//squares.AddRange(MatrixSolver.GetCombinationsProduct(algebraicCombinations));

			*/
			BigInteger productC = smoothRelations.Select(rel => rel.C).Where(i => !i.IsZero).ProductMod(n);
			BigInteger gcd = GCD.FindGCD(n, productC % n);

			mainForm.LogOutput();
			mainForm.LogOutput($"relations.Select(rel => f(rel.C)).Product(): {productC}");
			mainForm.LogOutput();
			mainForm.LogOutput($"Product(C)%N: {productC % n}");
			mainForm.LogOutput();
			mainForm.LogOutput($"GCD(N,ProductC): {gcd}");
			mainForm.LogOutput();


			if (gcd > 1)
			{
				mainForm.LogOutput(
					$@"
****************
* FACTOR FOUND *
*              * 
* {gcd} *
****************
					");
				mainForm.LogOutput();
			}

			return gnfs;
		}

	}
}
