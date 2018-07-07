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
	using GNFSCore.Factors;
	using GNFSCore.IntegerMath;

	public partial class GnfsUiBridge
	{

		public GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			List<Relation> smoothRelations = gnfs.CurrentRelationsProgress.SmoothRelations.ToList();

			int smoothCount = smoothRelations.Count;

			int maxRelationsToSelect =
				PrimeFactory.GetIndexFromValue(gnfs.PrimeFactorBase.MaxRationalFactorBase)
				+ PrimeFactory.GetIndexFromValue(gnfs.PrimeFactorBase.MaxAlgebraicFactorBase)
				+ gnfs.QFB.Count
				+ 3;


			mainForm.LogOutput($"Total relations: {smoothCount}");
			mainForm.LogOutput($"MaxRelationsToSelect: {maxRelationsToSelect}");
			mainForm.LogOutput($"ttl / max = {smoothCount / maxRelationsToSelect}");

			List<BigInteger> allAlgebraicProducts = new List<BigInteger>();
			List<BigInteger> allRationalProducts = new List<BigInteger>();

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
					//BigInteger algebraicFactorization = relations.Select(rel => rel.AlgebraicFactorization.ToDictionary().Select(kvp => BigInteger.Pow(kvp.Key, (int)kvp.Value)).Product()).Product();
					BigInteger rational = relations.Select(rel => rel.RationalNorm).Product();

					CountDictionary algCountDict = new CountDictionary();
					foreach (var rel in relations)
					{
						algCountDict.Combine(rel.AlgebraicFactorization);
					}

					bool isAlgebraicSquare = algebraic.IsSquare();
					bool isRationalSquare = rational.IsSquare();

					mainForm.LogOutput("---");
					mainForm.LogOutput($"Relations count: {relations.Count}");
					mainForm.LogOutput($"(a,b) pairs: {string.Join(" ", relations.Select(rel => $"({rel.A},{rel.B})"))}");
					mainForm.LogOutput($"Rational  ∏(a+mb): IsSquare? {isRationalSquare} : {rational}");
					mainForm.LogOutput($"Algebraic ∏ƒ(a/b): IsSquare? {isAlgebraicSquare} : {algebraic}");
					mainForm.LogOutput($"Algebraic (factorization): {algCountDict.FormatStringAsFactorization()}");

					if (isAlgebraicSquare && isRationalSquare)
					{
						solution.Add(relations);
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
			}

			gnfs.CurrentRelationsProgress.SetFreeRelations(allSolutionGroups);

			//mainForm.LogOutput();
			//mainForm.LogOutput("All solution set products:");
			//mainForm.LogOutput(allSolutionTuples.Select(p => $"IsSquare: {p.Item1.IsSquare().ToString().PadLeft(5)} {p.Item1}{Environment.NewLine}IsSquare: {p.Item2.IsSquare().ToString().PadLeft(5)} {p.Item2}{Environment.NewLine}").FormatString(true, 0));
			//mainForm.LogOutput();
			//var reducedSolutionTuples = allSolutionTuples.Select(p => new Tuple<BigInteger, BigInteger>(p.Item1 % gnfs.N, p.Item2 % gnfs.N)).ToList();
			//mainForm.LogOutput();
			//mainForm.LogOutput("All products mod N:");
			//mainForm.LogOutput(string.Join(Environment.NewLine, reducedSolutionTuples.Select(p => $"{p.Item1}{Environment.NewLine}{p.Item2}")));

			mainForm.LogOutput();
			mainForm.LogOutput();

			return gnfs;
		}

	}
}
