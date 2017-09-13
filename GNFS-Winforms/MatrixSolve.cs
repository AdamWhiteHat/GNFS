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
	using GNFSCore.FactorBase;
	using GNFSCore.IntegerMath;
	using GNFSCore.Polynomial.Internal;

	public partial class GnfsUiBridge
	{
		public GNFS GODDAMNMATRIXBULLSHITFFFUUUUUUUUU(CancellationToken cancelToken, GNFS gnfs)
		{
			BitMatrix matrix = gnfs.CurrentRelationsProgress.GetRelationsMatrix();

			mainForm.LogOutput("Relation matrix:");
			mainForm.LogOutput(matrix.ToString());
			mainForm.LogOutput();

			mainForm.LogOutput("RPB");
			mainForm.LogOutput(gnfs.PrimeBase.RationalPrimeBase.FormatString(false, 3));
			mainForm.LogOutput();

			mainForm.LogOutput("APB");
			mainForm.LogOutput(gnfs.PrimeBase.AlgebraicPrimeBase.FormatString(false, 3));
			mainForm.LogOutput();


			return gnfs;
		}

		public GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			List<Relation> orderedSmoothRelations = gnfs.CurrentRelationsProgress.SmoothRelations.ToList();

			Gaussian gaussianReduction = new Gaussian(orderedSmoothRelations);

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

			int freeVarCount = gaussianReduction.FreeVariables.Count(b => b == true);

			Relation[] solution1 = gaussianReduction.GetSolutionSet2();//gaussianReduction.GetSolutionSet(1);

			string solutionSet1 = string.Join(Environment.NewLine, solution1.Select(rel => rel.ToString()));

			mainForm.LogOutput();
			mainForm.LogOutput("Solution set #1:");
			mainForm.LogOutput(solutionSet1);
			mainForm.LogOutput();
			mainForm.LogOutput("All relations:");
			mainForm.LogOutput(string.Join(Environment.NewLine, gnfs.CurrentRelationsProgress.SmoothRelations.OrderBy(rel => rel.A).Select(rel => rel.ToString())));

			return gnfs;
		}

		public GNFS MatrixSolve5(CancellationToken cancelToken, GNFS gnfs)
		{
			BigInteger algebraicMax = gnfs.CurrentRelationsProgress.PrimeBase.AlgebraicPrimeBase.Max();
			BigInteger rationalMax = gnfs.CurrentRelationsProgress.PrimeBase.RationalPrimeBase.Max();

			List<BitVector> algebraicMatrix = new List<BitVector>();
			List<BitVector> rationalMatrix = new List<BitVector>();
			foreach (Relation rel in gnfs.CurrentRelationsProgress.SmoothRelations)
			{
				BitVector alg = new BitVector(rel.AlgebraicNorm, algebraicMax);
				BitVector rat = new BitVector(rel.RationalNorm, rationalMax);
				algebraicMatrix.Add(alg);
				rationalMatrix.Add(rat);
			}


			//IEnumerable<BitVector> algSingles = algebraicMatrix.Where(bv => bv.RowSum == 1);
			//algebraicMatrix = algebraicMatrix.Except(algSingles).ToList();

			//IEnumerable<BitVector> ratSingles = rationalMatrix.Where(bv => bv.RowSum == 1);
			//rationalMatrix = rationalMatrix.Except(ratSingles).ToList();

			algebraicMatrix = algebraicMatrix.OrderBy(bv => bv.RowSum).ToList();
			rationalMatrix = rationalMatrix.OrderBy(bv => bv.RowSum).ToList();

			BitMatrix algM = new BitMatrix(algebraicMatrix);
			BitMatrix ratM = new BitMatrix(rationalMatrix);

			mainForm.LogOutput(algM.ToString());
			mainForm.LogOutput("			        - - - - - - - - - - - - - - - - - - - - - - - - -  " + Environment.NewLine);
			mainForm.LogOutput(string.Join(" ", algM.ColumnSums));

			mainForm.LogOutput(ratM.ToString());
			mainForm.LogOutput("			        - - - - - - - - - - - - - - - - - - - - - - - - -  " + Environment.NewLine);
			mainForm.LogOutput(string.Join(" ", ratM.ColumnSums));

			return gnfs;
		}

		public GNFS MatrixSolve4(CancellationToken cancelToken, GNFS gnfs)
		{
			BigInteger max = BigInteger.Max(gnfs.CurrentRelationsProgress.PrimeBase.AlgebraicPrimeBase.Max(), gnfs.CurrentRelationsProgress.PrimeBase.RationalPrimeBase.Max());

			List<BitVector> vectors = gnfs.CurrentRelationsProgress.SmoothRelations
			   .SelectMany(rel => new BigInteger[] { rel.AlgebraicNorm, rel.RationalNorm })
			   .Select(bi => new BitVector(bi, max))
			   .ToList();

			BitMatrix matrix = new BitMatrix(vectors);

			List<BitVector> singulars = matrix.Rows.Where(bv => bv.RowSum == 1).ToList();

			bool[] mod2 = matrix.GetColumnsMod2();

			int counter = 0;

			foreach (bool b in mod2)
			{
				if (b)
				{
					var toRemove = singulars.Where(bv => bv.Elements[counter] == true);
					if (toRemove.Any())
					{
						matrix.Remove(toRemove);
					}
				}

				counter++;
			}

			return gnfs;
		}

		public GNFS MatrixSolve3(CancellationToken cancelToken, GNFS gnfs)
		{
			BigInteger algebraicMax = gnfs.CurrentRelationsProgress.PrimeBase.AlgebraicPrimeBase.Max();
			BigInteger rationalMax = gnfs.CurrentRelationsProgress.PrimeBase.RationalPrimeBase.Max();

			List<BitVector> matrix = new List<BitVector>();
			foreach (Relation rel in gnfs.CurrentRelationsProgress.SmoothRelations)
			{
				BitVector alg = new BitVector(rel.AlgebraicNorm, algebraicMax);
				BitVector rat = new BitVector(rel.RationalNorm, rationalMax);
				matrix.Add(alg);
				matrix.Add(rat);
			}

			IEnumerable<BitVector> squares = matrix.Where(bv => bv.IsSquare());
			matrix = matrix.Except(squares).ToList();
			matrix = matrix.OrderBy(bv => bv.RowSum).ToList();

			if (squares.Any())
			{
				mainForm.LogOutput(string.Join(Environment.NewLine, squares.Select(bv => $"{{ {bv.ToString()} }}")));
				mainForm.LogOutput(Environment.NewLine);
			}

			BitMatrix linearMatrixSparse = ExtractLinearEquation(ref matrix);

			matrix = matrix.OrderByDescending(bv => bv.RowSum).ToList();

			BitMatrix linearMatrixDense = ExtractLinearEquation(ref matrix);

			matrix = matrix.OrderByDescending(bv => bv.RowSum).ThenBy(bv => bv.IndexOfLeftmostElement()).ToList();



			BitMatrix denseMatrix = new BitMatrix(linearMatrixDense.Rows);
			bool[] denseColumnBits = denseMatrix.GetColumnsMod2();
			BitPattern denseBitPattern = new BitPattern(denseColumnBits);

			BitMatrix miscMatrix = new BitMatrix(matrix);

			bool[] afterColumnBits = new bool[0];
			BitVector bestMatch = miscMatrix.FindBestPartialMatch(denseBitPattern);
			if (bestMatch != null)
			{
				denseMatrix.Rows.Add(bestMatch);
				denseMatrix.Sort(SortByProperty.Leftmost);
				afterColumnBits = denseMatrix.GetColumnsMod2();
			}

			//BitMatrix sparseMatrix = new BitMatrix(linearMatrixSparse.Rows);
			//BitMatrix transposedMatrix = sparseMatrix.GetTransposeMatrix();
			//mainForm.LogOutput(transposedMatrix.ToString());
			//mainForm.LogOutput(Environment.NewLine);

			mainForm.LogOutput(linearMatrixSparse.ToString());
			mainForm.LogOutput(Environment.NewLine);

			mainForm.LogOutput(linearMatrixSparse.ToString());
			mainForm.LogOutput(string.Join(" ", denseMatrix.ColumnSums));
			mainForm.LogOutput(BitVector.FormatElements(denseColumnBits));


			if (afterColumnBits.Any())
			{
				mainForm.LogOutput("After match:");
				mainForm.LogOutput(string.Join(" ", BitVector.FormatElements(afterColumnBits)));
			}

			mainForm.LogOutput(Environment.NewLine);


			List<BitMatrix> grouped = matrix.GroupBy(bv => bv.RowSum).OrderBy(g => g.Key).Select(g => new BitMatrix(g.ToList())).ToList();

			foreach (BitMatrix bmatrix in grouped)
			{
				mainForm.LogOutput(bmatrix.ToString());
				mainForm.LogOutput();
			}

			return gnfs;
		}








		private BitMatrix ExtractLinearEquation(ref List<BitVector> input)
		{
			if (input == null || !input.Any()) { return new BitMatrix(new List<BitVector>()); }

			int columnIndex = 0;
			int maxIndex = input.First().Elements.Length;

			//var notSingular = input.Where(bv => bv.RowSum != 1).ToList();

			IEnumerable<IGrouping<int, BitVector>> grouped = input.GroupBy(bv => bv.RowSum).OrderBy(g => g.Key).ToList();

			//List<BitVector> orderedGrouped = grouped.OrderBy(g => g.Key).SelectMany(g => g).ToList();


			//int len = grouped.Select(g => g.Key).Max() + 1;

			Dictionary<int, List<BitVector>> dict = grouped.ToDictionary(g => g.Key, g => g.ToList());
			//List<BitVector> twoBits = dict[2];
			//List<BitVector> threeBits = dict[3];

			int maxGrouping = dict.Keys.Max();

			BitMatrix result = new BitMatrix(new List<BitVector>());
			while (columnIndex < maxIndex)
			{

				int counter = maxGrouping;
				IEnumerable<BitVector> clearOnLeft = new List<BitVector>();
				while (!clearOnLeft.Any() && counter >= 0)
				{
					if (!dict.ContainsKey(counter))
					{
						break;
					}

					clearOnLeft = SelectZeroTillIndex(dict[counter], columnIndex);
					counter--;
				}

				BitVector selectedVector = clearOnLeft.FirstOrDefault();

				if (selectedVector != default(BitVector))
				{
					input.Remove(selectedVector);
					result.Rows.Add(selectedVector);
				}
				columnIndex++;
			}

			return result;
		}




		private IEnumerable<BitVector> SelectZeroTillIndex(List<BitVector> input, int index)
		{
			return input.Where(
					bv =>
						bv.Elements[index] == true
						&&
						bv.Elements.Take(index).All(elm => elm == false)
					);
		}



		public GNFS MatrixSolve2(CancellationToken cancelToken, GNFS gnfs)
		{
			BigInteger polyBase = gnfs.CurrentPolynomial.Base;
			int factorBase = 117;

			var primes = PrimeFactory.GetPrimesTo(factorBase);

			//var rationalNorms = gnfs.CurrentRelationsProgress.SmoothRelations.Select(rel => rel.RationalNorm);

			var pCollection = gnfs.AFB.SelectMany(fb => new int[] { fb.P });
			var rCollection = gnfs.AFB.SelectMany(fb => new int[] { fb.R });

			var pRationalCollection = gnfs.RFB.SelectMany(fb => new int[] { fb.P });
			var rRationalCollection = gnfs.RFB.SelectMany(fb => new int[] { fb.R });

			List<int> pMatrixCollection =
				pCollection
				.Distinct()
				.Where(n => n > 3 && n < 1000 && !primes.Contains(n))
				.OrderByDescending(o => o)
				/*.Take(1000)*/
				.ToList();

			List<int> rMatrixCollection =
				rCollection
				.Distinct()
				.Where(n => n > 3 && n < 1000 && !primes.Contains(n))
				.OrderByDescending(o => o)
				/*.Take(1000)*/
				.ToList();

			List<int> pRationalMatrixCollection =
				pRationalCollection
				.Distinct()
				.Where(n => n > 3 && n < 1000 && !primes.Contains(n))
				.OrderByDescending(o => o)
				/*.Take(1000)*/
				.ToList();

			List<int> rRationalMatrixCollection =
				rRationalCollection
				.Distinct()
				.Where(n => n > 3 && n < 1000 && !primes.Contains(n))
				.OrderByDescending(o => o)
				/*.Take(1000)*/
				.ToList();

			List<int> toMatrixCollection =
				pMatrixCollection
				.Concat(rMatrixCollection)
				.Concat(pRationalMatrixCollection)
				.Concat(rRationalMatrixCollection)
				.Distinct()
				.ToList();


			List<BitVector> vectors1 = toMatrixCollection.Select(rn => new BitVector(rn, factorBase)).Where(bv => bv.Length != 0).ToList();

			List<BitVector> squares = new List<BitVector>(); //vectors1.Where(bv => bv.IsSquare()).ToList();

			List<BitVector> singles = new List<BitVector>(); //vectors1.Where(bv => bv.Elements.Count(bit => bit==true)==1).ToList();

			List<BitVector> twins = new List<BitVector>(); //vectors1.Where(bv => bv.Elements.Count(bit => bit == true) == 2).ToList();

			List<BitVector> unsorted = new List<BitVector>();

			foreach (BitVector bv in vectors1)
			{
				if (bv.IsSquare())
				{
					squares.Add(bv);
					continue;
				}

				int count = bv.RowSum;

				if (count == 1)
				{
					singles.Add(bv);
				}
				else if (count == 2)
				{
					twins.Add(bv);
				}
				else
				{
					unsorted.Add(bv);
				}
			}

			squares = squares.OrderBy(bv => bv.Number).ToList();
			singles = singles.OrderBy(bv => BitVector.GetWeight(bv)).ToList();
			twins = twins.OrderBy(bv => BitVector.GetWeight(bv)).ToList();
			unsorted = unsorted.OrderBy(bv => bv.RowSum).ToList();

			int vectorLength = vectors1[0].Length;

			List<BitVector> unsortedSquare = unsorted.Take(vectorLength).ToList();
			BitMatrix matrix = new BitMatrix(unsortedSquare);
			//BitMatrix transposedMatrix = matrix.GetTransposeMatrix();
			//mainForm.LogOutput("Matrix (transposed):");
			//mainForm.LogOutput(transposedMatrix.ToString());
			mainForm.LogOutput();
			mainForm.LogOutput("Matrix (squares):");
			mainForm.LogOutput(string.Join(Environment.NewLine, squares.Select(bs => bs.ToString())));
			mainForm.LogOutput();
			mainForm.LogOutput("Matrix (singles):");
			mainForm.LogOutput(string.Join(Environment.NewLine, singles.Select(bs => bs.ToString())));
			mainForm.LogOutput();
			mainForm.LogOutput("Matrix (twins):");
			mainForm.LogOutput(string.Join(Environment.NewLine, twins.Select(bs => bs.ToString())));
			mainForm.LogOutput();
			mainForm.LogOutput("Matrix (others):");
			mainForm.LogOutput(string.Join(Environment.NewLine, unsorted.Select(bs => bs.ToString())));
			mainForm.LogOutput();

			return gnfs;
		}

		public void MatrixSolve1(CancellationToken cancelToken, BitMatrix matrix)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}

			//IEnumerable<BitVector> rationalVectors = matrixVectors.Select(tup => tup.Item1);
			//IEnumerable<BitVector> algebraicVectors = matrixVectors.Select(tup => tup.Item2);

			//BitMatrix rationalVectorsMatrix = new BitMatrix(rationalVectors);
			//BitMatrix algebraicVectorsMatrix = new BitMatrix(algebraicVectors);

			//BitMatrix rationalTransposeMatrix = matrix.GetTransposeMatrix();
			//BitMatrix algebraicTransposeMatrix = algebraicVectorsMatrix.GetTransposeMatrix();

			//rationalTransposeMatrix.Sort();
			//algebraicTransposeMatrix.Sort();

			//rationalVectorsMatrix.Sort();
			//algebraicVectorsMatrix.Sort();

			mainForm.LogOutput($"*** BINARY MATRIX ***");
			mainForm.LogOutput();
			//mainForm.LogOutput("Rational matrix (transposed):");
			//mainForm.LogOutput(rationalTransposeMatrix.ToString());
			mainForm.LogOutput();
			mainForm.LogOutput();
			//mainForm.LogOutput("Algebraic matrix (transposed):");
			//mainForm.LogOutput(algebraicTransposeMatrix.ToString());
			//mainForm.LogOutput();
			//mainForm.LogOutput();
			//mainForm.LogOutput();

			//IEnumerable<BigInteger[]> rationalCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			//IEnumerable<BigInteger[]> algebraicCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			//mainForm.LogOutput("Rational squares:");
			//mainForm.LogOutput(MatrixSolver.FormatCombinations(rationalCombinations));
			//mainForm.LogOutput();
			//mainForm.LogOutput();
			//mainForm.LogOutput("Algebraic squares:");
			//mainForm.LogOutput(MatrixSolver.FormatCombinations(algebraicCombinations));
			//mainForm.LogOutput();
			//mainForm.LogOutput();
		}
	}
}
