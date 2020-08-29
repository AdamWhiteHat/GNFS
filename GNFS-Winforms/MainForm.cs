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
	using GNFSCore.IntegerMath;

	public partial class MainForm : Form
	{
		public BigInteger N
		{
			get
			{
				BigInteger result = -1;
				return BigInteger.TryParse(tbN.Text, out result) ? result : -1;
			}
			set
			{
				ControlBridge.SetControlText(tbN, value.ToString());
			}
		}

		public int Degree
		{
			get
			{
				int result = -1;
				return int.TryParse(tbDegree.Text, out result) ? result : -1;
			}
			set
			{
				ControlBridge.SetControlText(tbDegree, value.ToString());
			}
		}

		public BigInteger Base
		{
			get
			{
				BigInteger result = -1;
				return BigInteger.TryParse(tbBase.Text, out result) ? result : -1;
			}
			set
			{
				ControlBridge.SetControlText(tbBase, value.ToString());
			}
		}

		public BigInteger Bound
		{
			get
			{
				BigInteger result = -1;
				return BigInteger.TryParse(tbBound.Text, out result) ? result : -1;
			}
			set
			{
				ControlBridge.SetControlText(tbBound, value.ToString());
			}
		}

		public bool DoesSaveFileExist
		{
			get
			{
				BigInteger currentN = N;
				if (currentN == -1) return false;
				string directory = DirectoryLocations.GetSaveLocation(currentN);
				if (!Directory.Exists(directory)) return false;
				return File.Exists(Path.Combine(directory, DirectoryLocations.SaveFilename));
			}
		}

		public bool IsWorking { get; private set; }

		#region Private Members		

		private GNFS _gnfs;

		private CancellationToken _cancellationToken;
		private CancellationTokenSource _cancellationTokenSource;

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

			IsWorking = false;
			_gnfs = null;

			Logging.PrimaryForm = this;
			Logging.OutputTextbox = tbOutput;
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			ControlBridge.SetControlText(tbN, Settings.N);
			ControlBridge.SetControlText(tbDegree, Settings.Degree);
			ControlBridge.SetControlText(tbBase, Settings.Base);
			ControlBridge.SetControlText(tbBound, Settings.Bound);
			ControlBridge.SetControlText(tbRelationQuantity, Settings.RelationQuantity);
			ControlBridge.SetControlText(tbRelationValueRange, Settings.RelationValueRange);

			ControlBridge.SetControlVisibleState(panelCancel, false);

			RefreshLoadSaveButtonState();

			this.tbN.TextChanged += new System.EventHandler(this.tbN_TextChanged);
		}

		private static void SetGnfs(MainForm form, GNFS gnfs)
		{
			if (form.IsDisposed || !form.IsHandleCreated)
			{
				throw new Exception();
			}

			if (form.InvokeRequired)
			{
				form.Invoke(new Action(() => SetGnfs(form, gnfs)));
			}
			else
			{
				form._gnfs = gnfs;

				form.N = gnfs.N;
				form.Degree = gnfs.PolynomialDegree;

				form.Base = gnfs.PolynomialBase;
				form.Bound = gnfs.PrimeFactorBase.RationalFactorBaseMax;

				form.tbRelationQuantity.Text = gnfs.CurrentRelationsProgress.SmoothRelations_TargetQuantity.ToString();
				form.tbRelationValueRange.Text = gnfs.CurrentRelationsProgress.ValueRange.ToString();
			}
		}

		private void SetAsProcessing()
		{
			ControlBridge.SetControlVisibleState(panelCancel, true);
			ControlBridge.SetControlVisibleState(panelButtons, false);

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_cancellationToken.Register(new Action(() => RestoreAllButtons()));

			Logging.LogMessage($"Processing thread LAUNCHED.");

			IsWorking = true;
		}


		private void HaultAllProcessing()
		{
			if (_cancellationTokenSource != null && IsWorking)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		private void RestoreAllButtons()
		{
			if (IsWorking)
			{
				IsWorking = false;

				ControlBridge.SetControlVisibleState(panelCancel, false);
				ControlBridge.SetControlVisibleState(panelButtons, true);

				Logging.LogMessage($"Processing thread COMPLETED.");
			}
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

		private void tbN_TextChanged(object sender, EventArgs e)
		{
			TextBox control = (TextBox)sender;
			if (control == null) return;
			if (!control.Modified) return;

			RefreshLoadSaveButtonState();

			//control.Modified = false;
		}

		private void RefreshLoadSaveButtonState()
		{
			if (DoesSaveFileExist)
			{
				ControlBridge.SetControlEnabledState(btnLoad, true);
				ControlBridge.SetControlEnabledState(btnCreate, false);
				ControlBridge.SetControlEnabledState(btnSave, true);
			}
			else
			{
				ControlBridge.SetControlEnabledState(btnLoad, false);
				ControlBridge.SetControlEnabledState(btnCreate, true);
				ControlBridge.SetControlEnabledState(btnSave, false);
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
			BigInteger rationalBaseMax = _gnfs.PrimeFactorBase.RationalFactorBaseMax;
			BigInteger textboxBaseMax = Bound;

			BigInteger newBaseMax = rationalBaseMax;
			if (textboxBaseMax > rationalBaseMax)
			{
				newBaseMax = textboxBaseMax;
			}
			else
			{
				newBaseMax = rationalBaseMax + 100000;
			}

			Bound = newBaseMax;

			_gnfs.CaclulatePrimeFactorBaseBounds(newBaseMax);
			_gnfs.SetPrimeFactorBases();
			PrintCurrentCounts();
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

				GNFS localGnfs = _gnfs;
				CancellationToken token = _cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS resultGnfs = GnfsUiBridge.FindRelations(token, localGnfs, breakAfterOneRound);
					SetGnfs(this, resultGnfs);
					HaultAllProcessing();
					Logging.LogMessage("[Find relations task complete]");
					PrintCurrentCounts();
				}).Start();
			}
		}

		private void btnMatrix_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				Logging.LogMessage("[Matrix solve task starting up...]");

				GNFS localGnfs = _gnfs;
				CancellationToken token = _cancellationTokenSource.Token;
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

				GNFS localGnfs = _gnfs;
				CancellationToken token = _cancellationTokenSource.Token;
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
			int before = _gnfs.CurrentRelationsProgress.RoughRelations.Count;

			_gnfs.CurrentRelationsProgress.PurgePrimeRoughRelations();

			int after = _gnfs.CurrentRelationsProgress.RoughRelations.Count;

			int quantityRemoved = before - after;

			Logging.LogMessage($"Purged {quantityRemoved} rough relations whom were prime.");

			PrintCurrentCounts();
		}

		private void btnPrintRelations_Click(object sender, EventArgs e)
		{
			if (_gnfs.CurrentRelationsProgress.SmoothRelations.Any())
			{
				Logging.LogMessage(_gnfs.CurrentRelationsProgress.ToString());
			}

			PrintCurrentCounts();
		}

		private void PrintCurrentCounts()
		{
			int smoothRelation_CurrentTarget = _gnfs.CurrentRelationsProgress.SmoothRelations_TargetQuantity;

			int smoothRelations_CurrentCount = _gnfs.CurrentRelationsProgress.SmoothRelations.Count;
			int smoothRelation_SavedCounter = _gnfs.CurrentRelationsProgress.SmoothRelationsCounter;

			BigInteger rationalBase_Max = _gnfs.PrimeFactorBase.RationalFactorBaseMax;
			BigInteger algebraicBase_Max = _gnfs.PrimeFactorBase.AlgebraicFactorBaseMax;
			BigInteger quadraticBase_Max = _gnfs.PrimeFactorBase.QuadraticFactorBaseMax;

			int rationalBase_Size = PrimeFactory.GetIndexFromValue(rationalBase_Max);
			int algebraicBase_Size = PrimeFactory.GetIndexFromValue(algebraicBase_Max);
			int quadraticBase_Size = PrimeFactory.GetIndexFromValue(quadraticBase_Max);

			int rationalFactorPair_Count = _gnfs.RationalFactorPairCollection.Count;
			int algebraicFactorPair_Count = _gnfs.AlgebraicFactorPairCollection.Count;
			int quadraticFactorPair_Count = _gnfs.QuadraticFactorPairCollection.Count;

			int smoothRelation_RequiredBeforeMatrixStep = _gnfs.CurrentRelationsProgress.SmoothRelationsRequiredForMatrixStep;

			Logging.LogMessage();
			Logging.LogMessage($"Required smooth relations found before beginning matrix step: {smoothRelation_RequiredBeforeMatrixStep}");
			Logging.LogMessage($"Smooth relations target value: {smoothRelation_CurrentTarget}");
			Logging.LogMessage();
			Logging.LogMessage($"Smooth relations currently loaded count: {smoothRelations_CurrentCount}");
			Logging.LogMessage($"Smooth relations saved counter: {smoothRelation_SavedCounter}");
			Logging.LogMessage();
			Logging.LogMessage($"quadraticFactorPair_Count: {quadraticFactorPair_Count}");
			Logging.LogMessage($"PrimeIndxOf(quadraticBase_Max): {PrimeFactory.GetIndexFromValue(quadraticBase_Max)}");
			Logging.LogMessage();
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
			Serialization.Save.All(_gnfs);
			Logging.LogMessage("[Save progress task successfully completed]");
			RefreshLoadSaveButtonState();
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				BigInteger n = N;

				string jsonFilename = Path.Combine(DirectoryLocations.GetSaveLocation(n), DirectoryLocations.SaveFilename);
				Logging.LogMessage($"[Loading factorization progress from \"{jsonFilename}\"...]");

				CancellationToken token = _cancellationTokenSource.Token;

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
					PrintCurrentCounts();
					RefreshLoadSaveButtonState();

				}).Start();
			}
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();

				BigInteger n = N;
				int degree = Degree;
				BigInteger polyBase = Base;
				BigInteger bound = Bound;

				int relationQuantity = int.Parse(tbRelationQuantity.Text);
				int relationValueRange = int.Parse(tbRelationValueRange.Text);

				//Logging.OutputFilename = DirectoryLocations.GetUniqueNameFromN(n) + ".LOG.txt";
				//Logging.CreateLogFileIfNotExists();

				Logging.LogMessage($"[New factorization job creation initialization for N = {DirectoryLocations.GetUniqueNameFromN(n)}...]");

				CancellationToken token = _cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS localGnfs =
						GnfsUiBridge.CreateGnfs
						(
							token,  // CancellationToken
							n,      // Semi-prime to factor N = P*Q
							polyBase, // Polynomial base (value for x)
							degree, // Polynomial Degree
							bound, //  BigInteger
							relationQuantity, // Total # of relations to collect before proceeding.
							relationValueRange // 
						);

					SetGnfs(this, localGnfs);
					HaultAllProcessing();
					ControlBridge.SetControlEnabledState(panelFunctions, true);
					Logging.LogMessage($"[New factorization job initialization complete]");
					Logging.LogMessage($"NOTE: You should save your progress now.");
					PrintCurrentCounts();
					RefreshLoadSaveButtonState();
				}).Start();
			}
		}


		#endregion

	}
}