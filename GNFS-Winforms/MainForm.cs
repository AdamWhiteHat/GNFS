using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GNFSCore;
using GNFSCore.Prime;
using GNFSCore.FactorBase;

namespace GNFS_Winforms
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			tbN.Text = "3218147";
			tbBase.Text = "117";
			tbDegree.Text = "3";
			tbBound.Text = "60";
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

		private static string FormatTupleCollection(IEnumerable<Tuple<int, int>> tuples)
		{
			return string.Join("\t", tuples.Select(tup => $"({tup.Item1},{tup.Item2})"));

		}

		private void btnGetFactorBases_Click(object sender, EventArgs e)
		{
			BigInteger n = BigInteger.Parse(tbN.Text);
			BigInteger polyBase = BigInteger.Parse(tbBase.Text);
			int degree = int.Parse(tbDegree.Text);
			int bound = int.Parse(tbBound.Text);

			Polynomial p = new Polynomial(n, polyBase, degree);

			int algebraicBound = bound * (10 / 3);
			int quadraticBound = algebraicBound  + bound;

			IEnumerable<Tuple<int, int>> RFB = Rational.GetRationalFactorBase(polyBase, bound);			
			IEnumerable<Tuple<int, int>> AFB = Algebraic.GetAlgebraicFactorBase(p, algebraicBound);
			IEnumerable<Tuple<int, int>> QFB = Quadradic.GetQuadradicFactorBase(p, quadraticBound, quadraticBound+bound);

			LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			LogOutput(p.ToString());
			LogOutput();

			LogOutput($"Rational Factor Base (RFB):");
			LogOutput(FormatTupleCollection(RFB));
			LogOutput();

			LogOutput($"Algebraic Factor Base (AFB):");
			LogOutput(FormatTupleCollection(AFB));
			LogOutput();

			LogOutput($"Quadratic Factor Base (QFB):");
			LogOutput(FormatTupleCollection(QFB));
			LogOutput();

			List<int> potentialFactors = QFB.SelectMany(tup => new int[] { tup.Item1, tup.Item2 }).Distinct().OrderBy(i => i).ToList();
			IEnumerable<string> factorized = potentialFactors.Select(i => $"[{i}:{{{Factorization.GetPrimeFactoriationString(i)}}}]");

			LogOutput($"Factorization of QFB:");
			LogOutput(string.Join(Environment.NewLine, factorized));			
		}

	}
}

