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
	using GNFSCore.Polynomial;
	using GNFSCore.Factors;
	using GNFSCore.IntegerMath;
	using GNFSCore.Polynomial.Internal;

	public partial class GnfsUiBridge
	{

		public GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			List<Relation> orderedSmoothRelations = gnfs.CurrentRelationsProgress.SmoothRelations.ToList();

			
			if (orderedSmoothRelations.Count % 2 != 0)
			{
				orderedSmoothRelations.RemoveAt(orderedSmoothRelations.Count - 1);
			}

			Gaussian gaussianReduction = new Gaussian(gnfs, orderedSmoothRelations);
			gaussianReduction.DontTransposeAppend();

			string matrixBeforeTranspose = gaussianReduction.ToString();

			mainForm.LogOutput("Matrix BEFORE transpose:");
			mainForm.LogOutput($"  rows: {gaussianReduction.RowCount}");
			mainForm.LogOutput($"  cols: {gaussianReduction.ColumnCount}");
			mainForm.LogOutput(matrixBeforeTranspose);
			mainForm.LogOutput();

			gaussianReduction = new Gaussian(gnfs, orderedSmoothRelations);
			gaussianReduction.TransposeAppend();

			string matrixAfterTranspose = gaussianReduction.ToString();

			mainForm.LogOutput("Matrix after transpose:");
			mainForm.LogOutput($"  rows: {gaussianReduction.RowCount}");
			mainForm.LogOutput($"  cols: {gaussianReduction.ColumnCount}");
			mainForm.LogOutput(matrixAfterTranspose);
			mainForm.LogOutput();

			gaussianReduction.Elimination();

			string matrixAfterElimination = gaussianReduction.ToString();
			string freeVariables = Gaussian.VectorToString(gaussianReduction.FreeVariables);

			mainForm.LogOutput("Matrix after elimination:");
			mainForm.LogOutput(matrixAfterElimination);
			mainForm.LogOutput();
			mainForm.LogOutput("Free variables:");
			mainForm.LogOutput(freeVariables);
			mainForm.LogOutput();



			int number = 1;
			int max = gaussianReduction.FreeVariables.Where(fv => fv == true).Count();
			List<string> solutionFlagStrings = new List<string>();

			while (number <= max)
			{
				solutionFlagStrings.Add(Gaussian.VectorToString(gaussianReduction.GetSolutionFlags(number)));

				number++;
			}

			mainForm.LogOutput("Solution flags:");
			mainForm.LogOutput(string.Join(Environment.NewLine, solutionFlagStrings));
			mainForm.LogOutput();


			List<Relation> result = gaussianReduction.GetSolutionSet(1).ToList();

			BigInteger currentPolynomialDerivative = gnfs.CurrentPolynomial.Derivative(gnfs.CurrentPolynomial.Base);

			BigInteger rationalNormProduct = result.Select(rel => rel.RationalNorm).Product();
			BigInteger algebraicNormProduct = result.Select(rel => rel.AlgebraicNorm).Product();


			mainForm.LogOutput();
			mainForm.LogOutput("Square relations (Solution):");
			mainForm.LogOutput(result.FormatString());
			mainForm.LogOutput();

			mainForm.LogOutput($"Algebraic Product: {algebraicNormProduct}");
			mainForm.LogOutput($"Is square? {algebraicNormProduct.IsSquare()}");
			mainForm.LogOutput();
			mainForm.LogOutput($"Rational Product: {rationalNormProduct}");
			mainForm.LogOutput($"Is square? {rationalNormProduct.IsSquare()}");
			mainForm.LogOutput();
			mainForm.LogOutput();

			if (algebraicNormProduct.IsSquare() && rationalNormProduct.IsSquare())
			{
				mainForm.LogOutput("CurrentPolynomial.Derivative(gnfs.CurrentPolynomial.Base):");
				mainForm.LogOutput(currentPolynomialDerivative.ToString());
				mainForm.LogOutput();

				BigInteger rationalSquare = rationalNormProduct * currentPolynomialDerivative;
				BigInteger algebraicSquare = algebraicNormProduct * currentPolynomialDerivative;

				BigInteger rationalSquareRt = rationalSquare.SquareRoot();
				BigInteger algebraicSquareRt = algebraicSquare.SquareRoot();

				mainForm.LogOutput("rationalNormProduct* currentPolynomialDerivative:");
				mainForm.LogOutput(rationalSquare.ToString());
				mainForm.LogOutput("algebraidNormProduct* currentPolynomialDerivative:");
				mainForm.LogOutput(algebraicSquare.ToString());
				mainForm.LogOutput();

				mainForm.LogOutput("rationalSquareRt:");
				mainForm.LogOutput(rationalSquareRt.ToString());
				mainForm.LogOutput("algebraicSquareRt:");
				mainForm.LogOutput(algebraicSquareRt.ToString());
				mainForm.LogOutput();
			}

			return gnfs;
		}

	}
}
