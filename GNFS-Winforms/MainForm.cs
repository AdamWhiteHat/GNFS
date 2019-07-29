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

	public partial class MainForm : Form
	{
		#region Private Members		

		private GNFS gnfs;
		private int degree;
		private BigInteger n;
		private BigInteger polyBase;
		private BigInteger primeBound;

		public bool IsWorking { get; private set; }
		private CancellationToken cancellationToken;
		private CancellationTokenSource cancellationTokenSource;

		//private static readonly BigInteger MatthewBriggs = BigInteger.Parse("45113");
		//private static readonly BigInteger PerLeslieJensen = BigInteger.Parse("3218147");
		//private static readonly BigInteger RSA_100 = BigInteger.Parse("1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139");
		//private static readonly BigInteger RSA_110 = BigInteger.Parse("35794234179725868774991807832568455403003778024228226193532908190484670252364677411513516111204504060317568667");
		//private static readonly BigInteger RSA_120 = BigInteger.Parse("227010481295437363334259960947493668895875336466084780038173258247009162675779735389791151574049166747880487470296548479");
		//private static readonly BigInteger RSA_129 = BigInteger.Parse("114381625757888867669235779976146612010218296721242362562561842935706935245733897830597123563958705058989075147599290026879543541");
		//private static readonly BigInteger RSA_130 = BigInteger.Parse("1807082088687404805951656164405905566278102516769401349170127021450056662540244048387341127590812303371781887966563182013214880557");

		#endregion

		#region Winforms Methods

		public MainForm()
		{
			InitializeComponent();

			Logging.PrimaryForm = this;
			Logging.OutputTextbox = tbOutput;

			IsWorking = false;
			gnfs = null;

			tbN.Text = Settings.N.ToString();
			tbDegree.Text = Settings.Degree;
			tbBase.Text = Settings.Base;
			tbBound.Text = Settings.Bound;
			tbRelationQuantity.Text = Settings.RelationQuantity;
			tbRelationValueRange.Text = Settings.RelationValueRange;

			n = BigInteger.Parse(tbN.Text);
			degree = int.Parse(tbDegree.Text);

		}

		private static void SetGnfs(MainForm form, GNFS gnfs)
		{
			if (/* !GNFSCore.DirectoryLocations.IsLinuxOS() && */ form.InvokeRequired)
			{
				form.Invoke(new Action(() =>
					SetGnfs(form, gnfs)
				));
			}
			else
			{
				form.gnfs = gnfs;

				form.tbN.Text = gnfs.N.ToString();

				form.tbBound.Text = gnfs.PrimeFactorBase.RationalFactorBaseMax.ToString();
				form.tbBase.Text = gnfs.PolynomialBase.ToString();
				form.tbDegree.Text = gnfs.PolynomialDegree.ToString();

				form.tbRelationQuantity.Text = gnfs.CurrentRelationsProgress.Quantity.ToString();
				form.tbRelationValueRange.Text = gnfs.CurrentRelationsProgress.ValueRange.ToString();
			}
		}

		private void SetAsProcessing()
		{
			panelButtons.Visible = false;
			panelCancel.Visible = true;			

			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;
			cancellationToken.Register(new Action(() => RestoreAllButtons()));

			Logging.LogMessage($"Processing thread LAUNCHED.");

			IsWorking = true;			
		}


		private void HaultAllProcessing()
		{
			if (cancellationTokenSource != null && IsWorking)
			{
				cancellationTokenSource.Cancel();
			}
		}

		private void RestoreAllButtons()
		{
			IsWorking = false;

			ControlBridge.SetControlVisibleState(panelCancel, false);
			ControlBridge.SetControlVisibleState(panelButtons, true);

			Logging.LogMessage($"Processing thread COMPLETED.");
		}

		private void tbOutput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				if (e.KeyCode == Keys.A)
				{
					tbOutput.SelectAll();
				}
			}
		}

		#endregion

		#region Button Methods

		private void btnCancel_Click(object sender, EventArgs e)
		{
			HaultAllProcessing();
			Logging.LogMessage($"Processing thread CANCELED.");
		}

		private void btnIncreaseSmoothnessBound_Click(object sender, EventArgs e)
		{
			BigInteger rationalBaseMax = gnfs.PrimeFactorBase.RationalFactorBaseMax;
			BigInteger textboxBaseMax = BigInteger.Parse(tbBound.Text);

			BigInteger newBaseMax = rationalBaseMax;
			if (textboxBaseMax > rationalBaseMax)
			{
				newBaseMax = textboxBaseMax;
			}
			else
			{
				newBaseMax = rationalBaseMax + 100000;
			}

			tbBound.Text = newBaseMax.ToString();

			gnfs.CaclulatePrimeFactorBaseBounds(newBaseMax);
			gnfs.SetPrimeFactorBases();
		}

		private void btnFindRelations_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				bool breakAfterOneRound = false;

				if (Logging.FirstFindRelations)
				{
					Logging.FirstFindRelations = false;
					breakAfterOneRound = true;
				}

				Logging.LogMessage("[Find relations task starting up...]");

				GNFS localGnfs = gnfs;
				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = GnfsUiBridge.FindRelations(token, localGnfs, breakAfterOneRound);
					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
					Logging.LogMessage("[Find relations task complete]");
				}).Start();
			}
		}

		private void btnMatrix_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				Logging.LogMessage("[Matrix solve task starting up...]");

				GNFS localGnfs = gnfs;
				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = GnfsUiBridge.MatrixSolveGaussian(token, localGnfs);

					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
					Logging.LogMessage("[Matrix solve task complete]");
				}).Start();

			}
		}

		private void btnFindSquares_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				Logging.LogMessage("[Find square root task starting up...]");

				GNFS localGnfs = gnfs;
				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = GnfsUiBridge.FindSquares(token, localGnfs);

					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
					Logging.LogMessage("[Find square root task complete]");
				}).Start();

			}
		}

		private void btnPurgeRough_Click(object sender, EventArgs e)
		{
			int before = gnfs.CurrentRelationsProgress.RoughRelations.Count;

			gnfs.CurrentRelationsProgress.PurgePrimeRoughRelations();

			int after = gnfs.CurrentRelationsProgress.RoughRelations.Count;

			int quantityRemoved = before - after;

			Logging.LogMessage($"Purged {quantityRemoved} rough relations whom were prime.");
		}

		private void btnPrintRelations_Click(object sender, EventArgs e)
		{
			if (gnfs.CurrentRelationsProgress.SmoothRelations.Any())
			{
				Logging.LogMessage(gnfs.CurrentRelationsProgress.ToString());
			}
		}

		private void linkGitHubProject_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(linkGitHubProject.Text);
		}

		#endregion

		#region Create / Load / Save

		private void btnSave_Click(object sender, EventArgs e)
		{
			Logging.LogMessage("[Save progress task began...]");
			Serialization.Save.All(gnfs);
			Logging.LogMessage("[Save progress task successfully completed]");
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();
				//ControlBridge.SetControlEnabledState(btnCreate, false);

				n = BigInteger.Parse(tbN.Text);

				//Logging.OutputFilename = DirectoryLocations.GetUniqueNameFromN(n) + ".LOG.txt";
				//Logging.CreateLogFileIfNotExists();

				string jsonFilename = Path.Combine(DirectoryLocations.GetSaveLocation(n), "GNFS.json");
				Logging.LogMessage($"[Loading factorization progress from \"{jsonFilename}\"...]");

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS localGnfs = GnfsUiBridge.LoadGnfs(n);
					SetGnfs(this, localGnfs);
					HaultAllProcessing();
					ControlBridge.SetControlEnabledState(panelFunctions, true);
					Logging.LogMessage();
					Logging.LogMessage("Counts/Quantities:");
					Logging.LogMessage();
					Logging.LogMessage($"Algebraic Factor Base (MaxValue):\t{localGnfs.PrimeFactorBase.AlgebraicFactorBaseMax}");
					Logging.LogMessage($"Rational Factor Base (MaxValue):\t{localGnfs.PrimeFactorBase.RationalFactorBaseMax}");
					Logging.LogMessage($"Quadratic Factor Base (MaxValue):\t{localGnfs.PrimeFactorBase.QuadraticFactorBaseMax}");
					Logging.LogMessage();
					Logging.LogMessage($"Algebraic Factor Pairs (Quantity):\t{localGnfs.AlgebraicFactorPairCollection.Count}");
					Logging.LogMessage($"Rational Factor Pairs (Quantity):\t{localGnfs.RationalFactorPairCollection.Count}");
					Logging.LogMessage($"Quadratic Factor Pairs (Quantity):\t{localGnfs.QuadraticFactorPairCollection.Count}");
					Logging.LogMessage();
					Logging.LogMessage($"     Smooth Relations (Quantity):\t{localGnfs.CurrentRelationsProgress.SmoothRelationsCounter}");
					Logging.LogMessage($"      Rough Relations (Quantity):\t{localGnfs.CurrentRelationsProgress.RoughRelations.Count}");
					Logging.LogMessage($"       Free Relations (Quantity):\t{localGnfs.CurrentRelationsProgress.FreeRelationsCounter}");
					Logging.LogMessage();
					Logging.LogMessage("[Loading factorization progress complete]");




				}).Start();
			}
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();
				//ControlBridge.SetControlEnabledState(btnCreate, false);

				n = BigInteger.Parse(tbN.Text);
				degree = int.Parse(tbDegree.Text);
				polyBase = BigInteger.Parse(tbBase.Text);
				primeBound = BigInteger.Parse(tbBound.Text);

				int relationQuantity = int.Parse(tbRelationQuantity.Text);
				int relationValueRange = int.Parse(tbRelationValueRange.Text);

				//Logging.OutputFilename = DirectoryLocations.GetUniqueNameFromN(n) + ".LOG.txt";
				//Logging.CreateLogFileIfNotExists();

				Logging.LogMessage($"[New factorization job creation initialization for N = {DirectoryLocations.GetUniqueNameFromN(n)}...]");

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS localGnfs =
						GnfsUiBridge.CreateGnfs
						(
							token,  // CancellationToken
							n,      // Semi-prime to factor N = P*Q
							polyBase, // Polynomial base (value for x)
							degree, // Polynomial Degree
							primeBound, //  BigInteger
							relationQuantity, // Total # of relations to collect before proceeding.
							relationValueRange // 
						);



					SetGnfs(this, localGnfs);
					HaultAllProcessing();
					ControlBridge.SetControlEnabledState(panelFunctions, true);
					Logging.LogMessage($"[New factorization job initialization complete]");
					Logging.LogMessage($"NOTE: You should save your progress now.");
				}).Start();
			}
		}

		#endregion


	}
}