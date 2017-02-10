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
			
		}
	}
}

