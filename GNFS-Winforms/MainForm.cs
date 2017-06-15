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
	using System.Threading;

	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			cancellationSource = new CancellationTokenSource();
			cancellationSource.Cancel();
			tbN.Text = "1807082088687404805951656164405905566278102516769401349170127021450056662540244048387341127590812303371781887966563182013214880557";//"9035410443437024029758280822029527831390512583847006745850635107250283312701220241936705637954061516858909439832815910066074402785";   // "40815183453689876308460096333405025830273709373822334818010625964698700067207"; // "3218147"; //"45113"; //"1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139"; //"1001193673991790373"; //"45113";//"3218147"; //"3580430111"
			tbBase.Text = "3845520700308425278140"; //"12574411168418005980468"; //"3845520700308425278207"; // <- prime //"5867732301053"; //"12574411168418005980468"; //"31";//"29668737024"; //"11875";//"117";//"31";"127";
			tbDegree.Text = "5"; //"6"; //"7" //"3";
		}

		private GNFS gnfs;
		private int degree;
		private BigInteger n;
		private BigInteger polyBase;
		private CancellationTokenSource cancellationSource;

		private static string FindSquaresButtonText = "Find Squares";
		private static string FindRelationsButtonText = "Find Relations";
		private static string CreateGnfsButtonText = "Create/Load";
		private static string CancelButtonText = "Cancel";

		private void HaultAllProcessing()
		{
			if (!cancellationSource.IsCancellationRequested)
			{
				cancellationSource.Cancel();
				RestoreAllButtons();
			}

			if (gnfs != null && gnfs.Relations.Any())
			{
				SetControlEnabledState(btnFindSquares, true);
			}
		}

		private void RestoreAllButtons()
		{
			SetControlText(btnCreateGnfs, CreateGnfsButtonText);
			SetControlText(btnFindRelations, FindRelationsButtonText);
			SetControlText(btnFindSquares, FindSquaresButtonText);


		}

		private void btnFindSquares_Click(object sender, EventArgs e)
		{
			if (cancellationSource.IsCancellationRequested)
			{
				cancellationSource = new CancellationTokenSource();
				btnFindSquares.Text = CancelButtonText;

				CancellationToken token = cancellationSource.Token;
				new Thread(() =>
				{
					FindSquares(token);
					HaultAllProcessing();
				}).Start();
			}
			else
			{
				HaultAllProcessing();
			}
		}

		private void btnFindRelations_Click(object sender, EventArgs e)
		{
			if (cancellationSource.IsCancellationRequested)
			{
				cancellationSource = new CancellationTokenSource();
				btnFindRelations.Text = CancelButtonText;

				CancellationToken token = cancellationSource.Token;
				new Thread(() =>
				{
					while (!token.IsCancellationRequested)
					{
						IEnumerable<Relation> relations = gnfs.GenerateRelations(token);
						LogOutput($"Generated relations:");
						LogOutput(relations.Take(5).FormatString());
						LogOutput("(restricted result set to top 5)");
					}
					HaultAllProcessing();

				}).Start();
			}
			else
			{
				HaultAllProcessing();
			}
		}

		private void btnCreateGnfs_Click(object sender, EventArgs e)
		{
			if (cancellationSource.IsCancellationRequested)
			{
				cancellationSource = new CancellationTokenSource();
				btnCreateGnfs.Text = CancelButtonText;

				n = BigInteger.Parse(tbN.Text);
				polyBase = BigInteger.Parse(tbBase.Text);
				degree = int.Parse(tbDegree.Text);

				CancellationToken token = cancellationSource.Token;
				new Thread(() =>
				{
					CreateGnfs(token);
					HaultAllProcessing();

				}).Start();
			}
			else
			{
				HaultAllProcessing();
			}
		}

		private void CreateGnfs(CancellationToken cancelToken)
		{
			gnfs = new GNFS(cancelToken, n, polyBase, degree);

			if (cancelToken.IsCancellationRequested)
			{
				return;
			}

			if (gnfs.Relations.Any())
			{
				SetControlEnabledState(btnFindSquares, true);
			}

			SetControlText(tbBound, gnfs.PrimeBound.ToString());

			LogOutput($"N = {gnfs.N}");
			LogOutput();

			LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			LogOutput(gnfs.CurrentPolynomial.ToString());
			LogOutput();


			LogOutput("Prime Factor Base Bounds:");
			LogOutput($"PrimeBound         : {gnfs.PrimeBound}");
			LogOutput($"RationalFactorBase : {gnfs.RationalFactorBase}");
			LogOutput($"AlgebraicFactorBase: {gnfs.AlgebraicFactorBase}");
			LogOutput($"QuadraticPrimeBase : {gnfs.QuadraticPrimeBase.Last()}");
			LogOutput();


			LogOutput($"Rational Factor Base (RFB):");
			LogOutput(gnfs.RFB.ToString(20));
			LogOutput();

			LogOutput($"Algebraic Factor Base (AFB):");
			LogOutput(gnfs.AFB.ToString(20));
			LogOutput();

			LogOutput($"Quadratic Factor Base (QFB):");
			LogOutput(gnfs.QFB.ToString(20));
			LogOutput();

			SetControlEnabledState(btnFindRelations, true);
			SetControlText(btnFindRelations, CancelButtonText);
			SetControlEnabledState(btnCreateGnfs, false);


			// valueRange & quantity 
			IEnumerable<Relation> smoothRelations = gnfs.GenerateRelations(cancelToken);//, quantity);

			if (cancelToken.IsCancellationRequested)
			{
				return;
			}

			SetControlEnabledState(btnFindSquares, true);

			//IEnumerable<Relation> zeroRelations = smoothRelations.Where(r => r.D == 0);
			//zeroRelations = zeroRelations.Where(r => r.G == 0);

			LogOutput($"Smooth relations:");
			LogOutput("\t_______________________________________________");
			LogOutput($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tQuantity: {smoothRelations.Count()} Target quantity: {(gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count() + 1).ToString()}"/* Search range: -{relationsRange} to {relationsRange}"*/);
			LogOutput("\t```````````````````````````````````````````````");
			//LogOutput( string.Join(Environment.NewLine, smoothRelations.Select(rel => $"{rel.A},{rel.B}")));
			LogOutput(smoothRelations.OrderByDescending(rel => rel.A * rel.B).Take(5).FormatString());
			LogOutput("(restricted result set to top 5)");
			LogOutput();

			//var matrixVectors = smoothRelations.Select(rel => rel.GetMatrixRowVector());
			//var rationalVectors = matrixVectors.Select(tup => tup.Item1);
			//var algebraicVectors = matrixVectors.Select(tup => tup.Item1);

			//BitMatrix rationalVectorsMatrix = new BitMatrix(rationalVectors);
			//BitMatrix algebraicVectorsMatrix = new BitMatrix(algebraicVectors);

			//IEnumerable<BigInteger[]> rationalCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);
			//IEnumerable<BigInteger[]> algebraicCombinations = MatrixSolver.GetSquareCombinations(rationalVectorsMatrix);

			//LogOutput($"*** BINARY MATRIX ***");
			//LogOutput();
			//LogOutput("Rational matrix:");
			//LogOutput(MatrixSolver.FormatCombinations(rationalCombinations));
			//LogOutput();
			//LogOutput();
			//LogOutput("Algebraic matrix:");
			//LogOutput(MatrixSolver.FormatCombinations(algebraicCombinations));
			//LogOutput();
			//LogOutput();

			//List<BigInteger> squares = MatrixSolver.GetCombinationsProduct(rationalCombinations);
			//squares.AddRange(MatrixSolver.GetCombinationsProduct(algebraicCombinations));


			BigInteger productC = smoothRelations.Select(rel => rel.C).Where(i => !i.IsZero).ProductMod(n);
			BigInteger gcd = GCD.FindGCD(n, productC % n);

			LogOutput();
			LogOutput($"relations.Select(rel => f(rel.C)).Product(): {productC}");
			LogOutput();
			LogOutput($"Product(C)%N: {productC % n}");
			LogOutput();
			LogOutput($"GCD(N,ProductC): {gcd}");
			LogOutput();


			if (gcd > 1)
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

		private void FindSquares(CancellationToken cancelToken)
		{
			List<BigInteger> norms = new List<BigInteger>();
			norms.AddRange(gnfs.Relations.Select(rel => BigInteger.Abs(rel.AlgebraicNorm)));
			norms.AddRange(gnfs.Relations.Select(rel => BigInteger.Abs(rel.RationalNorm)));

			norms.AddRange(gnfs.Relations.Select(rel => BigInteger.Abs((BigInteger)rel.A)));
			norms.AddRange(gnfs.Relations.Select(rel => BigInteger.Abs((BigInteger)rel.B)));
			norms.AddRange(gnfs.Relations.Select(rel => BigInteger.Abs(rel.C)));

			IEnumerable<BigInteger> squares = norms.Select(bi => BigInteger.Abs(bi)).Distinct();
			squares = squares.Where(bi => bi.IsSquare()).Distinct();

			if (squares.Any())
			{
				//BigInteger squaresProduct = squares.Product();
				//squares.Insert(0, squaresProduct);

				LogOutput();
				LogOutput("SQUARES FOUND:");
				LogOutput(squares.FormatString());
				LogOutput();

				SquaresMethod squaresMethod = new SquaresMethod(n, squares);

				int maxSteps = 5;
				int counter = 0;
				BigInteger[] factors = new BigInteger[0];
				do
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}

					factors = squaresMethod.Attempt(2);

					counter++;
				}
				while (!factors.Any() && counter < maxSteps);

				//IEnumerable<BigInteger> squaresGCDs = squares.Select(bi => GCD.FindGCD(n, bi));
				//IEnumerable<BigInteger> factors = squaresGCDs.Where(bi => bi > 1);

				if (factors.Any())
				{
					LogOutput();
					LogOutput("**************** FACTORS FOUND ****************");
					LogOutput(factors.FormatString());
					LogOutput("**************** FACTORS FOUND ****************");
				}
			}
			else
			{
				MessageBox.Show("No squares found in relations!\nSieve more relations.");
				//List<int> fbSquares = new List<int>();
				//fbSquares.AddRange(gnfs.AFB.Select(pair => pair.R));
				//fbSquares.AddRange(gnfs.RFB.Select(pair => pair.R));
				//fbSquares.AddRange(gnfs.QFB.Select(pair => pair.R));

				//fbSquares = fbSquares.Where(i => Math.Sqrt(i) % 1 == 0).ToList();

				//if (fbSquares.Any())
				//{

				//}
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

		private static void SetControlEnabledState(Control control, bool enabled)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(new MethodInvoker(() =>
					SetControlEnabledState(control, enabled)
				));
			}
			else
			{
				control.Enabled = enabled;
			}
		}


		private static void SetControlText(Control control, string text)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(new MethodInvoker(() =>
					SetControlText(control, text)
				));
			}
			else
			{
				control.Text = text;
			}
		}

		public void LogOutput(string message = "")
		{
			if (tbOutput.InvokeRequired)
			{
				tbOutput.Invoke(new MethodInvoker(() => LogOutput(message)));
			}
			else
			{
				if (!this.IsDisposed)
				{
					tbOutput.AppendText(message + Environment.NewLine);
				}
			}
		}
	}
}

