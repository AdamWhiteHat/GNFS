using System;
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
	using GNFSCore.FactorBase;
	using GNFSCore.IntegerMath;
	using GNFSCore.PrimeSignature;
	using GNFSCore.Polynomial.Internal;

	public partial class MainForm : Form
	{
		public ControlBridge BridgeTextboxN;
		public ControlBridge BridgeTextboxDegree;
		public ControlBridge BridgeTextboxBase;
		public ControlBridge BridgeButtonGnfs;
		public ControlBridge BridgeButtonSquares;
		public ControlBridge BridgeButtonRelation;
		public ControlBridge BridgeButtonBound;
		private GnfsUiBridge gnfsBridge;

		public static string FindSquaresButtonText = "Find Squares";
		public static string FindRelationsButtonText = "Find Relations";
		public static string CreateGnfsButtonText = "Create/Load";
		public static string CancelButtonText = "Cancel";

		private static readonly BigInteger PerLeslieJensen = BigInteger.Parse("3218147");
		private static readonly BigInteger RSA_100 = BigInteger.Parse("1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139");
		private static readonly BigInteger RSA_110 = BigInteger.Parse("35794234179725868774991807832568455403003778024228226193532908190484670252364677411513516111204504060317568667");
		private static readonly BigInteger RSA_120 = BigInteger.Parse("227010481295437363334259960947493668895875336466084780038173258247009162675779735389791151574049166747880487470296548479");
		private static readonly BigInteger RSA_129 = BigInteger.Parse("114381625757888867669235779976146612010218296721242362562561842935706935245733897830597123563958705058989075147599290026879543541");
		private static readonly BigInteger RSA_130 = BigInteger.Parse("1807082088687404805951656164405905566278102516769401349170127021450056662540244048387341127590812303371781887966563182013214880557");

		public MainForm()
		{
			InitializeComponent();
			IsWorking = false;

			gnfsBridge = new GnfsUiBridge(this);

			tbN.Text = RSA_100.ToString();
			tbDegree.Text = "5"; //"3"; //"6"; //"7"						


			n = BigInteger.Parse(tbN.Text);
			degree = int.Parse(tbDegree.Text);

			IEnumerable<BigInteger> primes = PrimeFactory.GetPrimes(10000);
			BigInteger baseM = CommonPolynomial.SuggestPolynomialBase(n, degree, primes);

			tbBase.Text = baseM.ToString();

			BridgeTextboxN = new ControlBridge(tbN);
			BridgeTextboxDegree = new ControlBridge(tbDegree);
			BridgeTextboxBase = new ControlBridge(tbBase);

			BridgeButtonGnfs = new ControlBridge(btnCreateGnfs);
			BridgeButtonRelation = new ControlBridge(btnFindRelations);
			BridgeButtonSquares = new ControlBridge(btnFindSquares);
			BridgeButtonBound = new ControlBridge(tbBound);

			gnfsBridge = new GnfsUiBridge(this);
		}




		private GNFS gnfs;
		private int degree;
		private BigInteger n;
		private BigInteger polyBase;

		private bool IsWorking = false;
		private CancellationToken cancellationToken;
		private CancellationTokenSource cancellationTokenSource;






		private void SetAsProcessing()
		{
			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
			cancellationToken.Register(new Action(() => RestoreAllButtons()));
			IsWorking = true;
		}

		private void HaultAllProcessing()
		{
			if (cancellationTokenSource != null && IsWorking)//(!cancellationTokenSource.IsCancellationRequested)
			{
				cancellationToken = cancellationTokenSource.Token;
				cancellationToken.Register(new Action(() => RestoreAllButtons()));
				cancellationTokenSource.Cancel();
			}
		}

		private void RestoreAllButtons()
		{
			ControlBridge.SetControlText(btnCreateGnfs, CreateGnfsButtonText);
			ControlBridge.SetControlText(btnFindRelations, FindRelationsButtonText);
			ControlBridge.SetControlText(btnFindSquares, FindSquaresButtonText);
			ControlBridge.SetControlEnabledState(btnFindSquares, true);
			IsWorking = false;
		}






		private void btnCreateGnfs_Click(object sender, EventArgs e)
		{
			if (IsWorking)
			{
				HaultAllProcessing();
			}
			else
			{
				SetAsProcessing();
				ControlBridge.SetControlText(btnCreateGnfs, CancelButtonText);
				ControlBridge.SetControlEnabledState(btnConstructPoly, true); // Enable Construct Polynomials Button Control

				n = BigInteger.Parse(tbN.Text);
				degree = int.Parse(tbDegree.Text);
				polyBase = BigInteger.Parse(tbBase.Text);

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					gnfs = gnfsBridge.CreateGnfs(n, polyBase, degree, token);
					HaultAllProcessing();

				}).Start();
			}
		}

		private void btnFindRelations_Click(object sender, EventArgs e)
		{
			if (IsWorking)
			{
				HaultAllProcessing();
			}
			else
			{
				SetAsProcessing();
				ControlBridge.SetControlText(btnFindRelations, CancelButtonText);

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					new List<RoughPair>();
					while (!token.IsCancellationRequested)
					{
						List<RoughPair> knownRough = gnfs.RoughRelations;

						IEnumerable<Relation> relations = gnfs.GenerateRelations(token);
						LogOutput($"Generated relations:");
						LogOutput(relations/*.Skip(relations.Count()-5)*/.FormatString());
						//LogOutput("(restricted result set to top 5)");
						LogOutput();
						LogOutput();
						LogOutput($"Rough numbers (Relations with remainders, i.e. not fully factored):");
						LogOutput(gnfs.RoughRelations.Except(knownRough)/*.Skip(gnfs.RoughNumbers.Count()-5)*/.FormatString());
						//LogOutput("(restricted result set to top 5)");
						LogOutput();
					}
					HaultAllProcessing();

				}).Start();
			}
		}

		private void btnFindSquares_Click(object sender, EventArgs e)
		{
			if (IsWorking)
			{
				HaultAllProcessing();
			}
			else
			{
				SetAsProcessing();
				ControlBridge.SetControlText(btnFindSquares, CancelButtonText);

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					gnfsBridge.FindSquares(gnfs, token);
					HaultAllProcessing();
				}).Start();
			}
		}




		private void btnConstructPoly_Click(object sender, EventArgs e)
		{
			SkewSymmetricPolynomial skewPoly = new SkewSymmetricPolynomial(gnfs, degree);

			LogOutput("Skew Polynomial:");
			LogOutput($"{ skewPoly}");
			LogOutput();
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
					tbOutput.AppendText(message + Environment.NewLine);
				}
			}
		}
	}
}