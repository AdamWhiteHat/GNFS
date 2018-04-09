using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Polynomial;
	using GNFSCore.Factors;
	using GNFSCore.SquareRoot;
	using GNFSCore.IntegerMath;
	using GNFSCore.Matrix;
	using GNFSCore.Polynomial.Internal;

	public partial class MainForm : Form
	{
		#region Private Members		

		private GNFS gnfs;
		private int degree;
		private BigInteger n;
		private BigInteger polyBase;
		private BigInteger primeBound;
		private GnfsUiBridge gnfsBridge;

		private string logFilename;
		private bool IsWorking = false;
		private CancellationToken cancellationToken;
		private CancellationTokenSource cancellationTokenSource;

		private ControlBridge BridgeTextboxN;
		private ControlBridge BridgeTextboxDegree;
		private ControlBridge BridgeTextboxBase;
		private ControlBridge BridgeButtonGnfs;
		private ControlBridge BridgeButtonSquares;
		private ControlBridge BridgeButtonRelation;
		private ControlBridge BridgeButtonBound;

		private static string FindSquaresButtonText = "Find Squares";
		private static string FindRelationsButtonText = "Find Relations";
		private static string CreateGnfsButtonText = "Create/Load";
		private static string CancelButtonText = "Cancel";

		private static readonly BigInteger PerLeslieJensen = BigInteger.Parse("3218147");
		private static readonly BigInteger RSA_100 = BigInteger.Parse("1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139");
		private static readonly BigInteger RSA_110 = BigInteger.Parse("35794234179725868774991807832568455403003778024228226193532908190484670252364677411513516111204504060317568667");
		private static readonly BigInteger RSA_120 = BigInteger.Parse("227010481295437363334259960947493668895875336466084780038173258247009162675779735389791151574049166747880487470296548479");
		private static readonly BigInteger RSA_129 = BigInteger.Parse("114381625757888867669235779976146612010218296721242362562561842935706935245733897830597123563958705058989075147599290026879543541");
		private static readonly BigInteger RSA_130 = BigInteger.Parse("1807082088687404805951656164405905566278102516769401349170127021450056662540244048387341127590812303371781887966563182013214880557");

		#endregion

		#region Winforms Methods

		public MainForm()
		{
			InitializeComponent();
			IsWorking = false;
			logFilename = "";
			gnfs = null;

			gnfsBridge = new GnfsUiBridge(this);

			tbN.Text = PerLeslieJensen.ToString();//RSA_100.ToString();
			tbDegree.Text = "3"; //"5"; //"6"; //"7"						


			n = BigInteger.Parse(tbN.Text);
			degree = int.Parse(tbDegree.Text);

			IEnumerable<BigInteger> primes = PrimeFactory.GetPrimes(10000);
			BigInteger baseM = CommonPolynomial.SuggestPolynomialBase(n, degree, primes);

			tbBase.Text = baseM.ToString();
			tbBase.Text = "117";

			BridgeTextboxN = new ControlBridge(tbN);
			BridgeTextboxDegree = new ControlBridge(tbDegree);
			BridgeTextboxBase = new ControlBridge(tbBase);

			BridgeButtonGnfs = new ControlBridge(btnCreateGnfs);
			BridgeButtonRelation = new ControlBridge(btnFindRelations);
			BridgeButtonSquares = new ControlBridge(btnFindSquares);
			BridgeButtonBound = new ControlBridge(tbBound);

			gnfsBridge = new GnfsUiBridge(this);
		}

		private void SetGnfs(MainForm form, GNFS nfs)
		{
			if (form.InvokeRequired)
			{
				form.Invoke(new MethodInvoker(() =>
					SetGnfs(form, nfs)
				));
			}
			else
			{
				form.gnfs = nfs;
			}
		}

		private void SetAsProcessing()
		{
			panelButtons.Visible = false;
			panelCancel.Visible = true;

			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
			cancellationToken.Register(new Action(() => RestoreAllButtons()));
			IsWorking = true;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			HaultAllProcessing();
		}

		private void HaultAllProcessing()
		{
			if (cancellationTokenSource != null && IsWorking)
			{
				cancellationToken = cancellationTokenSource.Token;
				cancellationTokenSource.Cancel();
			}
		}

		private void RestoreAllButtons()
		{
			IsWorking = false;

			ControlBridge.SetControlVisibleState(panelCancel, false);
			ControlBridge.SetControlVisibleState(panelButtons, true);
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
					string toLog = message + Environment.NewLine;

					tbOutput.AppendText(toLog);

					if (!string.IsNullOrWhiteSpace(logFilename) && File.Exists(logFilename))
					{
						File.AppendAllText(logFilename, toLog);
					}
				}
			}
		}

		#endregion

		#region Button Methods

		private void btnCreateGnfs_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();
				ControlBridge.SetControlEnabledState(btnCreateGnfs, false);

				firstFindRelations = true;

				n = BigInteger.Parse(tbN.Text);
				degree = int.Parse(tbDegree.Text);
				polyBase = BigInteger.Parse(tbBase.Text);
				primeBound = BigInteger.Parse(tbBound.Text);

				int relationQuantity = int.Parse(tbRelationQuantity.Text);
				int relationValueRange = int.Parse(tbRelationValueRange.Text);


				logFilename = DirectoryLocations.GenerateFileNameFromBigInteger(n) + ".LOG.txt";
				if (File.Exists(logFilename))
				{
					if (!Directory.Exists(DirectoryLocations.GenerateSaveDirectory(n)))
					{
						File.Delete(logFilename);
					}
				}

				if (!File.Exists(logFilename))
				{
					string logHeader = $"Log created: {DateTime.Now}";
					string line = new string(Enumerable.Repeat('-', logHeader.Length).ToArray());

					File.WriteAllLines(logFilename, new string[] { logHeader, line, Environment.NewLine });
				}

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS localGnfs = gnfsBridge.CreateGnfs(n, polyBase, degree, primeBound, relationQuantity, relationValueRange, token);
					SetGnfs(this, localGnfs);
					HaultAllProcessing();
					ControlBridge.SetControlEnabledState(panelFunctions, true);

				}).Start();
			}
		}

		private bool firstFindRelations = false;
		private void btnFindRelations_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				bool breakAfterOneRound = false;

				if (firstFindRelations)
				{
					firstFindRelations = false;
					breakAfterOneRound = true;
				}

				GNFS localGnfs = gnfs;
				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = gnfsBridge.FindRelations(breakAfterOneRound, localGnfs, token);
					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
				}).Start();
			}
		}

		private void btnMatrix_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				GNFS localGnfs = gnfs;
				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = gnfsBridge.MatrixSolveGaussian(token, localGnfs);

					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
				}).Start();
			}
		}

		private void btnFindSquares_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				GNFS localGnfs = gnfs;
				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = gnfsBridge.FindSquares(localGnfs, token);

					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
				}).Start();
			}
		}

		private void btnPurgeRough_Click(object sender, EventArgs e)
		{
			int before = gnfs.CurrentRelationsProgress.RoughRelations.Count;

			gnfs.CurrentRelationsProgress.PurgePrimeRoughRelations();

			int after = gnfs.CurrentRelationsProgress.RoughRelations.Count;

			int quantityRemoved = before - after;

			MessageBox.Show($"Purged {quantityRemoved} rough relations whom were prime.");
		}

		private void btnSerialize_Click(object sender, EventArgs e)
		{
			string savePath = $"C:\\GNFS\\{gnfs.N}";
			gnfs.SaveGnfsProgress();
		}

		private void btnPrintRelations_Click(object sender, EventArgs e)
		{
			if (gnfs.CurrentRelationsProgress.SmoothRelations.Any())
			{
				LogOutput(gnfs.CurrentRelationsProgress.ToString());
			}
		}

		#endregion
	}
}