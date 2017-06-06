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
			tbN.Text = "3218147";// "1807082088687404805951656164405905566278102516769401349170127021450056662540244048387341127590812303371781887966563182013214880557";//"45113"; //"3218147";//"1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139"; //"1001193673991790373"; //"45113";//"3218147"; //"3580430111"
			tbBase.Text = "117";//"12574411168418005980468";//"31";//"29668737024"; //"11875";//"117";//"31";"127";
			tbDegree.Text = "3"; // "5"; //"7";
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

		private void btnGetFactorBases_Click(object sender, EventArgs e)
		{
			BigInteger n = BigInteger.Parse(tbN.Text);
			BigInteger polyBase = BigInteger.Parse(tbBase.Text);
			int degree = int.Parse(tbDegree.Text);

			int valueRange = 300;
			int quantity = 300;

			GNFS gnfs = new GNFS(n, polyBase, degree);

			tbBound.Text = gnfs.PrimeBound.ToString();

			LogOutput($"N = {gnfs.N}");
			LogOutput();

			LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			LogOutput(gnfs.Algebraic.ToString());
			LogOutput();


			LogOutput("Prime Factor Base Bounds:");
			LogOutput($"PrimeBound         : {gnfs.PrimeBound}");
			LogOutput($"RationalFactorBase : {gnfs.RationalFactorBase}");
			LogOutput($"AlgebraicFactorBase: {gnfs.AlgebraicFactorBase}");
			LogOutput($"QuadraticPrimeBase : {gnfs.QuadraticPrimeBase.Last()}");
			LogOutput();


			LogOutput($"Rational Factor Base (RFB):");
			LogOutput(gnfs.RFB.ToString());
			LogOutput();

			LogOutput($"Algebraic Factor Base (AFB):");
			LogOutput(gnfs.AFB.ToString());
			LogOutput();

			LogOutput($"Quadratic Factor Base (QFB):");
			LogOutput(gnfs.QFB.ToString());
			LogOutput();

			// valueRange & quantity 
			Relation[] smoothRelations = gnfs.GenerateRelations(valueRange, quantity);

			LogOutput($"Smooth relations:");
			LogOutput("\t_______________________________________________");
			LogOutput($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tQuantity: {smoothRelations.Count()} Target quantity: {(gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count() + 1).ToString()}"/* Search range: -{relationsRange} to {relationsRange}"*/);
			LogOutput("\t```````````````````````````````````````````````");
			//LogOutput( string.Join(Environment.NewLine, smoothRelations.Select(rel => $"{rel.A},{rel.B}")));
			LogOutput(smoothRelations.FormatString());
			LogOutput();

			//var matrixVectors = smoothRelations.Select(rel => rel.GetMatrixRowVector());
			//BitMatrix smoothRelationsMatrix = new BitMatrix(matrixVectors.ToList());
			//
			//LogOutput($"Smooth relations binary matrix:");
			//LogOutput(smoothRelationsMatrix.ToString());
			//LogOutput();

			BigInteger productC = smoothRelations.Select(rel => rel.C).Where(i => !i.IsZero).ProductMod(n);
			BigInteger gcd = GCD.FindGCD(n, productC % n);

			LogOutput();
			LogOutput($"relations.Select(rel => f(rel.C)).Product(): {productC}");
			LogOutput();
			LogOutput($"Product(C)%N: {productC % n}");
			LogOutput();
			LogOutput($"GCD(N,ProductC): {gcd}");
			LogOutput();


			if (gnfs.IsFactor(gcd))
			{
				LogOutput(
					$@"
****************
* FACTOR FOUND *
*              * 
* {gcd} *
****************
					");
				LogOutput();
			}

		}
	}
}

