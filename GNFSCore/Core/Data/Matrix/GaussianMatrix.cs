using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Core.Data.Matrix
{
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.RelationSieve;
	using Algorithm.IntegerMath;

	public partial class GaussianMatrix : List<bool[]>
	{
		public bool IsSolved;

		public bool[] FreeVariables { get { return freeCols; } }

		public int RowCount { get { return this.Count; } }
		public int ColumnCount { get { return this.Any() ? this.First().Length : 0; } }

		//private List<bool[]> M;
		public bool[] freeCols;


		private GNFS _gnfs;
		private List<Relation> relations;
		public Dictionary<int, Relation> ColumnIndexRelationDictionary;
		private List<Tuple<Relation, bool[]>> relationMatrixTuple;

		public GaussianMatrix(GNFS gnfs, List<Relation> rels)
			: base()
		{
			_gnfs = gnfs;
			relationMatrixTuple = new List<Tuple<Relation, bool[]>>();
			IsSolved = false;
			freeCols = new bool[0];

			relations = rels;

			List<GaussianRow> relationsAsRows = new List<GaussianRow>();

			foreach (Relation rel in relations)
			{
				GaussianRow row = new GaussianRow(_gnfs, rel);

				relationsAsRows.Add(row);
			}

			//List<GaussianRow> orderedRows = relationsAsRows.OrderBy(row1 => row1.LastIndexOfAlgebraic).ThenBy(row2 => row2.LastIndexOfQuadratic).ToList();

			List<GaussianRow> selectedRows = relationsAsRows.Take(_gnfs.CurrentRelationsProgress.SmoothRelationsRequiredForMatrixStep).ToList();

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

			TransposeAppend();
		}

		private void TransposeAppend()
		{
			ColumnIndexRelationDictionary = new Dictionary<int, Relation>();

			int index = 0;
			int numRows = relationMatrixTuple[0].Item2.Length;
			while (index < numRows)
			{
				ColumnIndexRelationDictionary.Add(index, relationMatrixTuple[index].Item1);

				List<bool> newRow = relationMatrixTuple.Select(bv => bv.Item2[index]).ToList();
				newRow.Add(false);
				Add(newRow.ToArray());

				index++;
			}

			freeCols = new bool[Count];
		}

		public static string VectorToString(bool[] vector)
		{
			return string.Join(",", vector.Select(b => b ? '1' : '0'));
		}

		public override string ToString()
		{
			return string.Join(Environment.NewLine, this.Select(i => VectorToString(i)));
		}
	}
}