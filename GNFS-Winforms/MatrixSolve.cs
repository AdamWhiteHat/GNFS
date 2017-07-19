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
		public void MatrixSolve(CancellationToken cancelToken, IEnumerable<Tuple<BitVector, BitVector>> matrixVectors)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}

			IEnumerable<BitVector> rationalVectors = matrixVectors.Select(tup => tup.Item1);
			IEnumerable<BitVector> algebraicVectors = matrixVectors.Select(tup => tup.Item2);

			BitMatrix rationalVectorsMatrix = new BitMatrix(rationalVectors);
			BitMatrix algebraicVectorsMatrix = new BitMatrix(algebraicVectors);

			rationalVectorsMatrix.Sort();
			algebraicVectorsMatrix.Sort();

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

			IEnumerable<BigInteger[]> rationalCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			IEnumerable<BigInteger[]> algebraicCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			mainForm.LogOutput("Rational squares:");
			mainForm.LogOutput(MatrixSolver.FormatCombinations(rationalCombinations));
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput("Algebraic squares:");
			mainForm.LogOutput(MatrixSolver.FormatCombinations(algebraicCombinations));
			mainForm.LogOutput();
			mainForm.LogOutput();
		}
	}
}
