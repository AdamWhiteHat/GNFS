using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Matrix
{
	using IntegerMath;

	public class GaussianMatrix
	{
		public List<bool[]> Matrix { get { return M; } }
		public bool[] FreeVariables { get { return freeCols; } }

		public int RowCount { get { return M.Count; } }
		public int ColumnCount { get { return M.Any() ? M.First().Length : 0; } }

		private List<bool[]> M;
		private bool[] freeCols;
		private bool eliminationStep;

		private GNFS _gnfs;
		private List<Relation> relations;
		public Dictionary<int, Relation> ColumnIndexRelationDictionary;
		private List<Tuple<Relation, bool[]>> relationMatrixTuple;
		
		public GaussianMatrix(GNFS gnfs, List<Relation> rels)
		{
			_gnfs = gnfs;
			relationMatrixTuple = new List<Tuple<Relation, bool[]>>();
			eliminationStep = false;
			freeCols = new bool[0];
			M = new List<bool[]>();


			int maxRelationsToSelect = PrimeFactory.GetIndexFromValue(_gnfs.PrimeFactorBase.RationalFactorBaseMax) + PrimeFactory.GetIndexFromValue(_gnfs.PrimeFactorBase.AlgebraicFactorBaseMax) + _gnfs.QuadradicFactorPairCollection.Count + 3;

			relations = rels;

			List<GaussianRow> relationsAsRows = new List<GaussianRow>();

			foreach (Relation rel in relations)
			{
				GaussianRow row = new GaussianRow(_gnfs, rel);

				relationsAsRows.Add(row);
			}

			//List<GaussianRow> orderedRows = relationsAsRows.OrderBy(row1 => row1.LastIndexOfAlgebraic).ThenBy(row2 => row2.LastIndexOfQuadratic).ToList();

			List<GaussianRow> selectedRows = relationsAsRows.Take(maxRelationsToSelect).ToList();

			int maxIndexRat = selectedRows.Select(row => row.LastIndexOfRational).Max();
			int maxIndexAlg = selectedRows.Select(row => row.LastIndexOfAlgebraic).Max();
			int maxIndexQua = selectedRows.Select(row => row.LastIndexOfQuadratic).Max();

			foreach (GaussianRow row in selectedRows)
			{
				row.ResizeRationalPart(maxIndexRat);
				row.ResizeAlgebraicPart(maxIndexAlg);
				row.ResizeQuadraticPart(maxIndexQua);
			}

			GaussianRow exampleRow = selectedRows.First();
			int newLength = exampleRow.GetBoolArray().Length;

			newLength++;

			selectedRows = selectedRows.Take(newLength).ToList();


			foreach (GaussianRow row in selectedRows)
			{
				relationMatrixTuple.Add(new Tuple<Relation, bool[]>(row.SourceRelation, row.GetBoolArray()));
			}
		}
		
		public void DontTransposeAppend()
		{
			List<bool[]> result = new List<bool[]>();
			ColumnIndexRelationDictionary = new Dictionary<int, Relation>();

			int index = 0;
			int numRows = relationMatrixTuple.Count; //[0].Item2.Length;

			while (index < numRows)
			{
				result.Add(relationMatrixTuple[index].Item2);
				ColumnIndexRelationDictionary.Add(index, relationMatrixTuple[index].Item1);

				index++;
			}

			M = result;
			freeCols = new bool[M.Count];
		}
		
		public void TransposeAppend()
		{
			List<bool[]> result = new List<bool[]>();
			ColumnIndexRelationDictionary = new Dictionary<int, Relation>();

			int index = 0;
			int numRows = relationMatrixTuple[0].Item2.Length;
			while (index < numRows)
			{
				ColumnIndexRelationDictionary.Add(index, relationMatrixTuple[index].Item1);

				List<bool> newRow = relationMatrixTuple.Select(bv => bv.Item2[index]).ToList();
				newRow.Add(false);
				result.Add(newRow.ToArray());

				index++;
			}

			M = result;
			freeCols = new bool[M.Count];
		}
				
		public void Elimination()
		{
			if (eliminationStep)
			{
				return;
			}

			int numRows = RowCount;
			int numCols = ColumnCount;

			freeCols = Enumerable.Repeat(false, numCols).ToArray();

			int h = 0;

			for (int i = 0; i < numRows && h < numCols; i++)
			{
				bool next = false;

				if (M[i][h] == false)
				{
					int t = i + 1;

					while (t < numRows && M[t][h] == false)
					{
						t++;
					}

					if (t < numRows)
					{
						//swap rows M[i] and M[t]

						bool[] temp = M[i];
						M[i] = M[t];
						M[t] = temp;
						temp = null;
					}
					else
					{
						freeCols[h] = true;
						i--;
						next = true;
					}
				}
				if (next == false)
				{
					for (int j = i + 1; j < numRows; j++)
					{
						if (M[j][h] == true)
						{
							// Add rows
							// M [j] ← M [j] + M [i]

							M[j] = Add(M[j], M[i]);
						}
					}
					for (int j = 0; j < i; j++)
					{
						if (M[j][h] == true)
						{
							// Add rows
							// M [j] ← M [j] + M [i]

							M[j] = Add(M[j], M[i]);
						}
					}
				}
				h++;
			}

			eliminationStep = true;
		}

		public List<Relation> GetSolutionSet(int numberOfSolutions)
		{
			bool[] solutionSet = GetSolutionFlags(numberOfSolutions);

			int index = 0;
			int max = ColumnIndexRelationDictionary.Count;

			List<Relation> result = new List<Relation>();
			while (index < max)
			{
				if (solutionSet[index] == true)
				{
					result.Add(ColumnIndexRelationDictionary[index]);
				}

				index++;
			}

			return result;
		}

		public bool[] GetSolutionFlags(int numSolutions)
		{
			if (!eliminationStep)
			{
				throw new Exception("Must call Elimination() method first!");
			}

			if (numSolutions < 1)
			{
				throw new ArgumentException($"{nameof(numSolutions)} must be greater than 1.");
			}

			int numRows = M.Count;
			int numCols = M.First().Length;

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

				while (freeCols[j] == false)
				{
					j++;
				}

				i--;
			}

			result[j] = true;

			for (i = 0; i < numRows - 1; i++)
			{
				if (M[i][j] == true)
				{
					int h = i;
					while (h < j)
					{
						if (M[i][h] == true)
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
		
		public static string VectorToString(bool[] vector)
		{
			return string.Join(",", vector.Select(b => b ? '1' : '0'));
		}

		public static string MatrixToString(List<bool[]> matrix)
		{
			return string.Join(Environment.NewLine, matrix.Select(i => VectorToString(i)));
		}

		public override string ToString()
		{
			return MatrixToString(M);
		}
	}
}