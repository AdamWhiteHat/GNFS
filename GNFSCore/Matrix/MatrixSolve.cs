using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFSCore.Matrix
{
	using IntegerMath;

	public static class MatrixSolve
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

			gnfs.LogFunction($"Total relations count: {smoothCount}");
			gnfs.LogFunction($"Relations required to proceed: {requiredRelationsCount}");

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
				gaussianReduction.TransposeAppend();
				gaussianReduction.Elimination();

				int number = 1;
				int solutionCount = gaussianReduction.FreeVariables.Count(b => b) - 1;
				List<List<Relation>> solution = new List<List<Relation>>();
				while (number <= solutionCount)
				{
					List<Relation> relations = gaussianReduction.GetSolutionSet(number);
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
	}
}
