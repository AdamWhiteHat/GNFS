using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.Numerics;
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

	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			tbN.Text = "3218147";//"45113";//"3218147"; //"3580430111"
			tbBase.Text = "117";//"31";"127";
			tbDegree.Text = "4";
			//tbBound.Text = "35";//"60";
		}

		public void LogOutput(string message = "")
		{
			if (tbOutput.InvokeRequired)
			{
				tbOutput.Invoke(new MethodInvoker(() => LogOutput(message)));
			}
			else
			{
				tbOutput.AppendText(message + Environment.NewLine);
			}
		}

		private void tbOutput_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				if (e.KeyCode == Keys.A)
				{
					tbOutput.SelectAll();
				}
			}
		}

		private static string FormatTupleCollection(IEnumerable<Tuple<int, int>> tuples)
		{
			return string.Join("\t", tuples.Select(tup => $"({tup.Item1},{tup.Item2})"));
		}

		private static string FormatArrayList(IEnumerable<int[]> array)
		{
			return string.Join("\t", array.Select(tup => $"({string.Join(",", tup.Select(i => i.ToString()))})"));

		}

		private static string FormatList(IEnumerable<int> array)
		{
			return $"({string.Join(",", array.Select(i => i.ToString()))})";

		}

		private int _take = 2;
		private void btnGetFactorBases_Click(object sender, EventArgs e)
		{
			BigInteger n = BigInteger.Parse(tbN.Text);
			BigInteger polyBase = BigInteger.Parse(tbBase.Text);
			int degree = int.Parse(tbDegree.Text);

			GNFS gnfs = new GNFS(n, polyBase, degree);

			tbBound.Text = gnfs.PrimeBound.ToString();

			LogOutput($"N = {gnfs.N}");
			LogOutput();

			LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			LogOutput(gnfs.AlgebraicPolynomial.ToString());
			LogOutput();

			LogOutput($"Rational Factor Base (RFB; Smoothness-bound: {gnfs.PrimeBound}):");
			LogOutput(FormatTupleCollection(gnfs.RFB));
			LogOutput();

			LogOutput($"Algebraic Factor Base (AFB):");
			LogOutput(FormatTupleCollection(gnfs.AFB));
			LogOutput();

			LogOutput($"Quadratic Factor Base (QFB):");
			LogOutput(FormatTupleCollection(gnfs.QFB));
			LogOutput();
					
			IEnumerable<Relation> smoothRelations = gnfs.GenerateRelations(200);

			LogOutput($"Smooth relations:");
			LogOutput(string.Join(Environment.NewLine, smoothRelations.Select(rel => rel.ToString())));
			LogOutput();

			List<BigInteger> factoringExample = new List<BigInteger>();
			factoringExample.AddRange(smoothRelations.Select(rel => rel.RationalNorm));
			factoringExample.AddRange(smoothRelations.Select(rel => rel.AlgebraicNorm));
			factoringExample = factoringExample.Distinct().OrderBy(i => i).ToList();
			
			LogOutput($"Prime factorization example:");
			LogOutput(string.Join(Environment.NewLine, factoringExample.Select(i => $"{i}: ".PadRight(5) + Factorization.FormatString.PrimeFactorization(Factorization.GetPrimeFactorizationTuple(i, gnfs.PrimeBound)))));
			LogOutput();



			//int range = 200;
			//var rationalNormsR = Rational.GetRationalNormRelations(gnfs, range);
			//var smoothNormsR = rationalNormsR.Where(t => Rational.IsSmooth(t.Item1, gnfs.Primes.Take(gnfs.PrimeBound)));
			//var orderedNormsR = smoothNormsR.OrderBy(t => t.Item1);

			//LogOutput($"Rational FB Relations:");
			//LogOutput(FormatTupleCollection(orderedNormsR));
			//LogOutput();

			//var polyCycles = Enumerable.Range(1, 15).Select(i => gnfs.AlgebraicPolynomial.Eval(i));
			//var modPoly = polyCycles.Select(i => n % i);

			//LogOutput($"Polynomial roots:");
			//LogOutput(string.Join(Environment.NewLine, polyCycles));
			//LogOutput();

			//LogOutput($"Polynomial roots MOD n:");
			//LogOutput(string.Join(Environment.NewLine, modPoly));
			//LogOutput();

			//var combinations = Combinatorics.GetCombinations(12, 3);
			//var products = combinations.Select(a => a.Prod()).Distinct().OrderBy(i => i);
			//var congruent = products.Where(a => (gnfs.N % a)==0);

			//LogOutput($"Combinatorics.GetCombinations(9, 2):");
			//LogOutput(FormatArrayList(combinations));
			//LogOutput();

			//LogOutput($"Congruent products(p1,p2,pN):");
			//LogOutput(FormatList(congruent));
			//LogOutput();
			/*

			int minRelations = 10;

			var rfbFactors = gnfs.RFB.Select(tup => tup.Item1);
			var afbFactors = gnfs.AFB.Select(tup => tup.Item1);
			var qfbFactors = gnfs.QFB.Select(tup => tup.Item1);
			var potentialFactors = qfbFactors.Union(rfbFactors).Union(afbFactors).Distinct().OrderBy(i => i).Take(30).ToList();
			//var intersection = qfbFactors.Intersect(rfbFactors).Intersect(afbFactors);

			var sqrt = gnfs.N.SquareRoot();
			var primes = gnfs.Primes.Where(p => p < sqrt);
			//potentialFactors = primes.ToList();

			int counter = 0;
			bool done = false;
			var results = new List<int>();
			var congruentProduct = new List<int>();
			while (!done && counter < 4)
			{
				var combination = Combinatorics.GetCombinations(potentialFactors.ToArray(), _take++);
				var product = combination.Select(t => t.Sum()).Distinct();
				var congruent = product.Where(p => (new BigInteger(p).IsPowerOfTwo) || (gnfs.N % p == 0) ); // mod N = 0
				//congruentProduct = congruent.Select(t => t.Sum()).Distinct().ToList();

				LogOutput($"Loop #{counter}:");
				LogOutput($"------------------"); LogOutput();
				//LogOutput($"Extracted Factor Bases:"); LogOutput(FormatList(potentialFactors)); LogOutput();
				//LogOutput($"Products:"); LogOutput(FormatList(product)); LogOutput();
				LogOutput($"Congruent:");
				LogOutput(FormatList(congruent));
				LogOutput();
				LogOutput($"------------------"); LogOutput();

				if (congruent.Any())
				{
					results.AddRange(congruent);
				}

				if (results.Count() >= minRelations)
				{
					done = true;
					continue;
				}

				potentialFactors = product.ToList();

				counter++;
			}


			//var factored = results.Select(ar => $"({string.Join(" \t , \t ", ar.Select(i => $"[{Factorization.GetPrimeFactorizationString(i)}]"))})");
			//var discrete = results.Select(ar => ar.Select(i => Factorization.GetPrimeFactorization(i).ToList()));
			var factorized = results.Select(p => Factorization.GetPrimeFactorizationString(p));

			LogOutput();
			LogOutput($"=================="); LogOutput();
			LogOutput($"RESULTS:");
			LogOutput(string.Join(Environment.NewLine, factorized));
			//LogOutput(string.Join(Environment.NewLine, factored));
			LogOutput();
			LogOutput($"=================="); LogOutput();
			*/

		}
	}
}

