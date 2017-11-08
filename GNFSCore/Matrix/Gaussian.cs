using GNFSCore.FactorBase;
using GNFSCore.IntegerMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Matrix
{
	public class Gaussian
	{
		public List<bool[]> Matrix { get { return M; } }
		public bool[] FreeVariables { get { return freeCols; } }

		public int RowCount { get { return M.Count; } }
		public int ColumnCount { get { return M.Any() ? M.First().Length : 0; } }

		private List<bool[]> M;
		private bool[] freeCols;
		private bool eliminationStep;

		private GNFS _gnfs;
		private Relation[] relations;
		public Dictionary<int, Relation> ColumnIndexRelationDictionary;
		private List<Tuple<Relation, bool[]>> relationMatrixTuple;


		public Gaussian(GNFS gnfs, List<Relation> rels)
		{
			_gnfs = gnfs;
			relationMatrixTuple = new List<Tuple<Relation, bool[]>>();
			eliminationStep = false;
			freeCols = new bool[0];
			M = new List<bool[]>();


			int maxRelationsToSelect = PrimeFactory.GetIndexFromValue(_gnfs.PrimeBase.RationalFactorBase) + PrimeFactory.GetIndexFromValue(_gnfs.PrimeBase.AlgebraicFactorBase) + _gnfs.QFB.Count() + 3;


			relations = rels.ToArray();

			//foreach (Relation rel in relations)
			//{
			//	rel.MatrixInitialize();
			//}

			//relations = relations.Where(rel => rel.AlgebraicWeight != 0 && rel.RationalWeight != 0);
			//relations = relations.OrderByDescending(rel => rel.AlgebraicWeight).ThenByDescending(rel => rel.RationalWeight);


			List<PrimeFactorization> rationalNormFactorizations = new List<PrimeFactorization>();
			foreach (Relation rel in relations)
			{
				rationalNormFactorizations.Add(new PrimeFactorization(rel.RationalNorm, _gnfs.PrimeBase.RationalFactorBase, true));
			}

			List<PrimeFactorization> algebraicNormFactorizations = new List<PrimeFactorization>();
			foreach (Relation rel in relations)
			{
				algebraicNormFactorizations.Add(new PrimeFactorization(rel.AlgebraicNorm, _gnfs.PrimeBase.AlgebraicFactorBase, true));
			}

			BigInteger rationalMaxPrimeFactor = rationalNormFactorizations.Max(lst => lst.Any() ? lst.Max(factor => factor.Prime) : 0);
			BigInteger algebraicMaxPrimeFactor = algebraicNormFactorizations.Max(lst => lst.Any() ? lst.Max(factor => factor.Prime) : 0);


			int maxIndex = relations.Length - 1;

			int index = 0;
			while (index < maxIndex)
			{
				rationalNormFactorizations[index].RestrictFactors(rationalMaxPrimeFactor);
				algebraicNormFactorizations[index].RestrictFactors(algebraicMaxPrimeFactor);

				index++;
			}


			List<Tuple<Relation, GaussianRow>> gaussianTuples = new List<Tuple<Relation, GaussianRow>>();

			index = 0;
			while (index < maxIndex)
			{
				Relation rel = relations[index];
				//var binaryRow = GetMatrixRow(rel, rationalNormFactorizations[index], algebraicNormFactorizations[index], rationalMaxPrimeFactor, algebraicMaxPrimeFactor);

				GaussianRow row = new GaussianRow(rel, rationalNormFactorizations[index], algebraicNormFactorizations[index], _gnfs.QFB, rationalMaxPrimeFactor, algebraicMaxPrimeFactor);

				gaussianTuples.Add(new Tuple<Relation, GaussianRow>(rel, row));

				index++;
			}

			List<GaussianRow> gaussianRows = gaussianTuples.Select(tup => tup.Item2).ToList();

			int maxIndexRat = gaussianRows.Select(row => row.LastIndexOfRational).Max();
			int maxIndexAlg = gaussianRows.Select(row => row.LastIndexOfAlgebraic).Max();
			int maxIndexQua = gaussianRows.Select(row => row.LastIndexOfQuadratic).Max();

			foreach (GaussianRow row in gaussianRows)
			{
				row.ResizeRationalPart(maxIndexRat);
				row.ResizeAlgebraicPart(maxIndexAlg);
				row.ResizeQuadraticPart(maxIndexQua);
			}

			foreach (Tuple<Relation, GaussianRow> tuple in gaussianTuples)
			{
				relationMatrixTuple.Add(new Tuple<Relation, bool[]>(tuple.Item1, tuple.Item2.GetBoolArray()));
			}

			//foreach (Relation rel in relations/*.Take(maxRelationsToSelect)*/)
			//{
			//	relationMatrixTuple.Add(new Tuple<Relation, bool[]>(rel, rel.GetMatrixRow()));
			//}


		}





		//private bool[] GetMatrixRow(Relation rel, PrimeFactorization rationalFactorization, PrimeFactorization algebraicFactorization, BigInteger rationalMaxValue, BigInteger algebraicMaxValue)
		//{
		//	bool sign = false;
		//	if (rel.RationalNorm.Sign == -1)
		//	{
		//		sign = true;
		//	}

		//	bool[] rational = GetVector(rationalFactorization, rationalMaxValue);
		//	bool[] algebraic = GetVector(algebraicFactorization, algebraicMaxValue);
		//	//bool[] quadratic = _gnfs.QFB.Select(qf => QuadraticResidue.GetQuadraticCharacter(rel, qf)).ToArray();

		//	List<bool> result = new List<bool>() { sign };
		//	result.AddRange(rational);
		//	result.AddRange(algebraic);
		//	//result.AddRange(quadratic);
		//	result.Add(true);
		//	return result.ToArray();
		//}


		public class GaussianRow
		{
			public bool Sign { get; set; }

			public List<bool> RationalPart { get; set; }
			public List<bool> AlgebraicPart { get; set; }
			public List<bool> QuadraticPart { get; set; }


			public int LastIndexOfRational { get { return RationalPart.LastIndexOf(true); } }
			public int LastIndexOfAlgebraic { get { return AlgebraicPart.LastIndexOf(true); } }
			public int LastIndexOfQuadratic { get { return QuadraticPart.LastIndexOf(true); } }

			public int RationalLength { get { return RationalPart.Count() - 1; } }
			public int AlgebraicLength { get { return AlgebraicPart.Count() - 1; } }
			public int QuadraticLength { get { return QuadraticPart.Count() - 1; } }

			public GaussianRow(GNFS gnfs, Relation relation)
			{
				if (relation.RationalNorm.Sign == -1)
				{
					Sign = true;
				}
				else
				{
					Sign = false;
				}

				FactorCollection qfb = gnfs.QFB;

				BigInteger rationalMaxValue = gnfs.PrimeBase.RationalFactorBase;
				BigInteger algebraicMaxValue = gnfs.PrimeBase.AlgebraicFactorBase;

				PrimeFactorization rationalFactorization = new PrimeFactorization(relation.RationalNorm, rationalMaxValue, true);
				PrimeFactorization algebraicFactorization = new PrimeFactorization(relation.AlgebraicNorm, algebraicMaxValue, true);

				RationalPart = GetVector(rationalFactorization, rationalMaxValue).ToList();
				AlgebraicPart = GetVector(algebraicFactorization, algebraicMaxValue).ToList();
				QuadraticPart = qfb.Select(qf => QuadraticResidue.GetQuadraticCharacter(relation, qf)).ToList();
			}

			public GaussianRow(Relation rel, PrimeFactorization rationalFactorization, PrimeFactorization algebraicFactorization, FactorCollection qfb, BigInteger rationalMaxValue, BigInteger algebraicMaxValue)
			{
				if (rel.RationalNorm.Sign == -1)
				{
					Sign = true;
				}
				else
				{
					Sign = false;
				}

				RationalPart = GetVector(rationalFactorization, rationalMaxValue).ToList();
				AlgebraicPart = GetVector(algebraicFactorization, algebraicMaxValue).ToList();
				QuadraticPart = qfb.Select(qf => QuadraticResidue.GetQuadraticCharacter(rel, qf)).ToList();
			}

			public bool[] GetBoolArray()
			{
				List<bool> result = new List<bool>() { Sign };
				result.AddRange(RationalPart);
				result.AddRange(AlgebraicPart);
				result.AddRange(QuadraticPart);
				//result.Add(false);
				return result.ToArray();
			}

			public void ResizeRationalPart(int size)
			{
				RationalPart = RationalPart.Take(size + 1).ToList();
			}

			public void ResizeAlgebraicPart(int size)
			{
				AlgebraicPart = AlgebraicPart.Take(size + 1).ToList();
			}

			public void ResizeQuadraticPart(int size)
			{
				QuadraticPart = QuadraticPart.Take(size + 1).ToList();
			}
		}















		protected static bool[] GetVector(PrimeFactorization primeFactorization, BigInteger maxValue)
		{
			int primeIndex = PrimeFactory.GetIndexFromValue(maxValue);

			bool[] result = new bool[primeIndex + 1];
			if (primeFactorization.Any())
			{
				foreach (Factor oddFactor in primeFactorization.Where(f => f.ExponentMod2 == 1))
				{
					if (oddFactor.Prime > maxValue)
					{
						throw new Exception();
					}
					int index = PrimeFactory.GetIndexFromValue(oddFactor.Prime);
					result[index] = true;
				}
			}

			return result.Take(primeIndex).ToArray();
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
				List<bool> newRow = relationMatrixTuple.Select(bv => bv.Item2[index]).ToList();
				newRow.Add(false);
				result.Add(newRow.ToArray());

				index++;
			}

			index = 0;
			int numColumns = relationMatrixTuple.Count;
			while (index < numColumns)
			{
				ColumnIndexRelationDictionary.Add(index, relationMatrixTuple[index].Item1);

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

		public Relation[] GetSolutionSet(int numberOfSolutions)
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

			return result.ToArray();
		}

		public Relation[] GetSolutionSet2()
		{
			int index = 0;
			int max = ColumnIndexRelationDictionary.Count;

			List<Relation> result = new List<Relation>();
			while (index < max)
			{
				if (freeCols[index] == true)
				{
					result.Add(ColumnIndexRelationDictionary[index]);
				}

				index++;
			}

			return result.ToArray();
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
