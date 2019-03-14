using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Matrix;
	using GNFSCore.IntegerMath;

	public partial class GnfsUiBridge
	{
		public GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			List<Relation> smoothRelations = gnfs.CurrentRelationsProgress.SmoothRelations.ToList();

			int smoothCount = smoothRelations.Count;

			int maxRelationsToSelect =
				PrimeFactory.GetIndexFromValue(gnfs.PrimeFactorBase.RationalFactorBaseMax)
				+ PrimeFactory.GetIndexFromValue(gnfs.PrimeFactorBase.AlgebraicFactorBaseMax)
				+ gnfs.QuadradicFactorPairCollection.Count
				+ 3;


			Logging.LogMessage($"Total relations: {smoothCount}");
			Logging.LogMessage($"MaxRelationsToSelect: {maxRelationsToSelect}");
			Logging.LogMessage($"ttl / max = {smoothCount / maxRelationsToSelect}");

			List<List<Relation>> allSolutionGroups = new List<List<Relation>>();
			List<Tuple<BigInteger, BigInteger>> allSolutionTuples = new List<Tuple<BigInteger, BigInteger>>();
			while (smoothRelations.Count >= maxRelationsToSelect)
			{

				// Randomly select n relations from smoothRelations
				List<Relation> selectedRelations = new List<Relation>();
				while (selectedRelations.Count != maxRelationsToSelect)
				{
					int randomIndex = StaticRandom.Next(0, smoothRelations.Count);
					selectedRelations.Add(smoothRelations[randomIndex]);
					smoothRelations.RemoveAt(randomIndex);
				}

				// Force number of relations to be even
				if (selectedRelations.Count % 2 != 0)
				{
					selectedRelations.RemoveAt(selectedRelations.Count - 1);
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

					Logging.LogMessage("---");
					Logging.LogMessage($"Relations count: {relations.Count}");
					Logging.LogMessage($"(a,b) pairs: {string.Join(" ", relations.Select(rel => $"({rel.A},{rel.B})"))}");
					Logging.LogMessage($"Rational  ∏(a+mb): IsSquare? {isRationalSquare} : {rational}");
					Logging.LogMessage($"Algebraic ∏ƒ(a/b): IsSquare? {isAlgebraicSquare} : {algebraic}");
					Logging.LogMessage($"Algebraic (factorization): {algCountDict.FormatStringAsFactorization()}");

					if (isAlgebraicSquare && isRationalSquare)
					{
						solution.Add(relations);
					}

					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
				}

				var productTuples =
					solution
						.Select(relList =>
							new Tuple<BigInteger, BigInteger>(
								relList.Select(rel => rel.AlgebraicNorm).Product(),
								relList.Select(rel => rel.RationalNorm).Product()
							)
						)
						.ToList();


				allSolutionGroups.AddRange(solution);
				allSolutionTuples.AddRange(productTuples);

				if (cancelToken.IsCancellationRequested)
				{
					break;
				}
			}

			gnfs.CurrentRelationsProgress.AddFreeRelations(allSolutionGroups);

			Logging.LogMessage();
			
			return gnfs;
		}

	}
}
