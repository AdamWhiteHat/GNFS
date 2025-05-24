using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFSCore.Algorithm.Matrix
{
	using Data;
	using Data.Matrix;
	using Data.RelationSieve;
	using ExtensionMethods;

	public static class MatrixSolver
	{
		public static void GaussianSolve(CancellationToken cancelToken, GNFS gnfs)
		{
			Serialization.Save.Relations.Smooth.Append(gnfs); // Persist any relations not already persisted to disk

			// Because some operations clear this collection after persisting unsaved relations (to keep memory usage light)...
			// We completely reload the entire relations collection from disk.
			// This ensure that all the smooth relations are available for the matrix solving step.
			Serialization.Load.Relations.Smooth(ref gnfs);


			List<Relation> smoothRelations = gnfs.CurrentRelationsProgress.SmoothRelations.ToList();

			int smoothCount = smoothRelations.Count;

			BigInteger requiredRelationsCount = gnfs.CurrentRelationsProgress.SmoothRelationsRequiredForMatrixStep;

			GNFS.LogFunction($"Total relations count: {smoothCount}");
			GNFS.LogFunction($"Relations required to proceed: {requiredRelationsCount}");

			while (smoothRelations.Count >= requiredRelationsCount)
			{
				// Randomly select n relations from smoothRelations
				List<Relation> selectedRelations = new List<Relation>();
				while (
						selectedRelations.Count < requiredRelationsCount
						||
						selectedRelations.Count % 2 != 0 // Force number of relations to be even
					)
				{
					int randomIndex = StaticRandom.Next(0, smoothRelations.Count);
					selectedRelations.Add(smoothRelations[randomIndex]);
					smoothRelations.RemoveAt(randomIndex);
				}

				GaussianMatrix gaussianReduction = new GaussianMatrix(gnfs, selectedRelations);
				Elimination(gaussianReduction);

				int number = 1;
				int solutionCount = gaussianReduction.FreeVariables.Count(b => b) - 1;
				List<List<Relation>> solution = new List<List<Relation>>();
				while (number <= solutionCount)
				{
					List<Relation> relations = GetSolutionSet(gaussianReduction, number);
					number++;

					BigInteger algebraic = relations.Select(rel => rel.AlgebraicNorm).Product();
					BigInteger rational = relations.Select(rel => rel.RationalNorm).Product();

					CountDictionary algCountDict = new CountDictionary();
					foreach (var rel in relations)
					{
						algCountDict.Combine(rel.AlgebraicFactorization);
					}

					bool isAlgebraicSquare = algebraic.IsSquare();
					bool isRationalSquare = rational.IsSquare();

					//gnfs.LogFunction("---");
					//gnfs.LogFunction($"Relations count: {relations.Count}");
					//gnfs.LogFunction($"(a,b) pairs: {string.Join(" ", relations.Select(rel => $"({rel.A},{rel.B})"))}");
					//gnfs.LogFunction($"Rational  ∏(a+mb): IsSquare? {isRationalSquare} : {rational}");
					//gnfs.LogFunction($"Algebraic ∏ƒ(a/b): IsSquare? {isAlgebraicSquare} : {algebraic}");
					//gnfs.LogFunction($"Algebraic (factorization): {algCountDict.FormatStringAsFactorization()}");

					if (isAlgebraicSquare && isRationalSquare)
					{
						solution.Add(relations);
						gnfs.CurrentRelationsProgress.AddFreeRelationSolution(relations);
					}

					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
				}

				if (cancelToken.IsCancellationRequested)
				{
					break;
				}
			}
		}


		public static void Elimination(GaussianMatrix matrix)
		{
			if (matrix.IsSolved)
			{
				return;
			}

			int numRows = matrix.RowCount;
			int numCols = matrix.ColumnCount;

			matrix.freeCols = Enumerable.Repeat(false, numCols).ToArray();

			int h = 0;

			for (int i = 0; i < numRows && h < numCols; i++)
			{
				bool next = false;

				if (matrix[i][h] == false)
				{
					int t = i + 1;

					while (t < numRows && matrix[t][h] == false)
					{
						t++;
					}

					if (t < numRows)
					{
						//swap rows M[i] and M[t]

						bool[] temp = matrix[i];
						matrix[i] = matrix[t];
						matrix[t] = temp;
						temp = null;
					}
					else
					{
						matrix.freeCols[h] = true;
						i--;
						next = true;
					}
				}
				if (next == false)
				{
					for (int j = i + 1; j < numRows; j++)
					{
						if (matrix[j][h] == true)
						{
							// Add rows
							// M [j] ← M [j] + M [i]

							matrix[j] = Add(matrix[j], matrix[i]);
						}
					}
					for (int j = 0; j < i; j++)
					{
						if (matrix[j][h] == true)
						{
							// Add rows
							// M [j] ← M [j] + M [i]

							matrix[j] = Add(matrix[j], matrix[i]);
						}
					}
				}
				h++;
			}

			matrix.IsSolved = true;
		}

		public static List<Relation> GetSolutionSet(GaussianMatrix matrix, int numberOfSolutions)
		{
			bool[] solutionSet = GetSolutionFlags(matrix, numberOfSolutions);

			int index = 0;
			int max = matrix.ColumnIndexRelationDictionary.Count;

			List<Relation> result = new List<Relation>();
			while (index < max)
			{
				if (solutionSet[index] == true)
				{
					result.Add(matrix.ColumnIndexRelationDictionary[index]);
				}

				index++;
			}

			return result;
		}

		public static bool[] GetSolutionFlags(GaussianMatrix matrix, int numSolutions)
		{
			if (!matrix.IsSolved)
			{
				throw new Exception("Must call Elimination() method first!");
			}

			if (numSolutions < 1)
			{
				throw new ArgumentException($"{nameof(numSolutions)} must be greater than 1.");
			}

			int numRows = matrix.RowCount;
			int numCols = matrix.ColumnCount;

			if (numSolutions >= numCols)
			{
				throw new ArgumentException($"{nameof(numSolutions)} must be less than the column count.");
			}

			bool[] result = new bool[numCols];

			int j = -1;
			int i = numSolutions;

			while (i > 0)
			{
				j++;

				while (matrix.freeCols[j] == false)
				{
					j++;
				}

				i--;
			}

			result[j] = true;

			for (i = 0; i < numRows - 1; i++)
			{
				if (matrix[i][j] == true)
				{
					int h = i;
					while (h < j)
					{
						if (matrix[i][h] == true)
						{
							result[h] = true;
							break;
						}
						h++;
					}
				}
			}

			return result;
		}

		public static bool[] Add(bool[] left, bool[] right)
		{
			if (left.Length != right.Length) throw new ArgumentException($"Both vectors must have the same length.");

			int length = left.Length;
			bool[] result = new bool[length];

			int index = 0;
			while (index < length)
			{
				result[index] = left[index] ^ right[index];
				index++;
			}

			return result;
		}
	}
}
