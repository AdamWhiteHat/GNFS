using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore;
using GNFSCore.FactorBase;
using GNFSCore.IntegerMath;
using GNFSCore.Polynomial;

namespace GNFS_Winforms
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			tbN.Text = "45113";//"3218147";
			tbBase.Text = "31";//"117";
			tbDegree.Text = "3";
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

		private void btnGetFactorBases_Click(object sender, EventArgs e)
		{
			BigInteger n = BigInteger.Parse(tbN.Text);
			BigInteger polyBase = BigInteger.Parse(tbBase.Text);
			int degree = int.Parse(tbDegree.Text);

			GNFS gnfs = new GNFS(n, polyBase, degree);

			tbBound.Text = gnfs.PrimeBound.ToString();


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




			//var rationalSieve = Sieve.LineSieve(gnfs, 100);

			//LogOutput($"Rational sieve relations:");
			//LogOutput(FormatTupleCollection(rationalSieve));
			//LogOutput();

			//var smooth = Sieve.Smooth(gnfs, rationalSieve);

			//LogOutput($"Relations after smooth:");
			//LogOutput(FormatTupleCollection(smooth));
			//LogOutput();

			//IEnumerable<Tuple<int, int>> relationsFound = Sieve.LineSieveForRelations(poly, 200, relationsNeeded, bound);

			//LogOutput($"Relations found after sieve:");
			//LogOutput(FormatTupleCollection(relationsFound));
			//LogOutput();

			var rfbFactors = gnfs.RFB.SelectMany(tup => new int[] { tup.Item2 });
			var afbFactors = gnfs.AFB.SelectMany(tup => new int[] { tup.Item2 });
			var qfbFactors = gnfs.QFB.SelectMany(tup => new int[] { tup.Item1, tup.Item2 });
			var potentialRFactors = rfbFactors.Distinct().OrderByDescending(i => i).ToList();
			var potentialAFactors = afbFactors.Distinct().OrderByDescending(i => i).ToList();
			var potentialQFactors = qfbFactors.Distinct().OrderByDescending(i => i).ToList();

			var potentialFactors = qfbFactors.Union(rfbFactors).Union(afbFactors).Distinct().OrderByDescending(i => i);

			var powers = potentialFactors.Select(i => new Tuple<int, int>(i, Factorization.LargestFactorPower(i))).Distinct().OrderByDescending(tup => tup.Item2);
			var mediumPowers = powers.Where(tup => tup.Item2 > 1);

			//List<string> factorized = new string[] { "-- R --" };
			//factorized.AddRange(potentialRFactors.Select(i => $"[{i}:{{{Factorization.GetPrimeFactoriationString(i)}}}]").ToList());
			//factorized.Add("-- A --");
			//factorized.AddRange(potentialAFactors.Select(i => $"[{i}:{{{Factorization.GetPrimeFactoriationString(i)}}}]").ToList());
			//factorized.Add("-- Q --");
			//factorized.AddRange(potentialQFactors.Select(i => $"[{i}:{{{Factorization.GetPrimeFactoriationString(i)}}}]").ToList());
			//factorized.Add("-- fin --");

			LogOutput($"Prime factorization of factor bases:");
			LogOutput(string.Join(Environment.NewLine, string.Join(Environment.NewLine, mediumPowers.Select(tup => $"[{tup.Item1}: {tup.Item2}]")))); 
			LogOutput();

			//var enumenumTuple = potentialQFactors.Select(q => Factorization.GetPrimeFactoriationTuple(q).Where(tup => (tup.Item2 % 2 == 0)));
			//qfbFactors.Select(q => Factorization.GetPrimeFactoriationTuple(q)/*.Where(tup => (tup.Item2%2==0))*/);
			
			LogOutput($"Prime factor exponents:");
			LogOutput(string.Join(Environment.NewLine, potentialFactors.Select(i => $"{i}: {Factorization.GetPrimeFactoriationString(i)}")));
			LogOutput();
		}


	}
}

