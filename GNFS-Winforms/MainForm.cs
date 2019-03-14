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
		private GnfsUiBridge gnfsBridge;

		private string logFilename;
		private bool IsWorking = false;
		private bool firstFindRelations = false;
		private CancellationToken cancellationToken;
		private CancellationTokenSource cancellationTokenSource;

		private static readonly BigInteger MatthewBriggs = BigInteger.Parse("45113");
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

			tbN.Text = MatthewBriggs.ToString(); //PerLeslieJensen.ToString();//RSA_100.ToString();
			tbDegree.Text = "3"; // "5";//"3"; //"4"; //"5"; //"6"; //"7"						


			n = BigInteger.Parse(tbN.Text);
			degree = int.Parse(tbDegree.Text);

			tbBase.Text = "31"; // "1261737131078349405";// "117"; // "31";

			tbBound.Text = "61"; // "1020379"; //"61"; 
			tbRelationQuantity.Text = "200"; // "4778012"; //"200";
			tbRelationValueRange.Text = "300"; //"1800000"; //"300";

			gnfsBridge = new GnfsUiBridge(this);
		}

		private static void SetGnfs(MainForm form, GNFS gnfs)
		{
			if (form.InvokeRequired)
			{
				form.Invoke(new MethodInvoker(() =>
					SetGnfs(form, gnfs)
				));
			}
			else
			{
				form.gnfs = gnfs;

				form.tbN.Text = gnfs.N.ToString();

				form.tbBound.Text = gnfs.PrimeFactorBase.RationalFactorBaseMax.ToString();
				form.tbBase.Text = gnfs.PolynomialBase.ToString();
				form.tbDegree.Text = gnfs.CurrentPolynomial.Degree.ToString();

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

		/// <summary>
		/// Creates a base folder and log file if no such structures exist.
		/// </summary>
		/// <returns>True if a previous save folder was found and loaded. This is so we know to populate the UI with loaded values.</returns>
		private bool CreateLogFileIfNotExists(string logFilename)
		{
			bool load = true;

			string directory = Path.GetDirectoryName(logFilename);

			if (!Directory.Exists(directory))
			{
				firstFindRelations = true;
				if (File.Exists(logFilename))
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

			return load;
		}

		#endregion

		#region Button Methods

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

		private void btnPrintRelations_Click(object sender, EventArgs e)
		{
			if (gnfs.CurrentRelationsProgress.SmoothRelations.Any())
			{
				LogOutput(gnfs.CurrentRelationsProgress.ToString());
			}
		}

		#endregion

		#region Create / Load / Save

		private void btnSave_Click(object sender, EventArgs e)
		{
			GNFS.JsonSave(gnfs);
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();
				ControlBridge.SetControlEnabledState(btnCreate, false);

				n = BigInteger.Parse(tbN.Text);

				logFilename = DirectoryLocations.GetUniqueNameFromN(n) + ".LOG.txt";
				CreateLogFileIfNotExists(logFilename);

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS localGnfs = gnfsBridge.LoadGnfs(token, n);
					SetGnfs(this, localGnfs);
					HaultAllProcessing();
					ControlBridge.SetControlEnabledState(panelFunctions, true);

				}).Start();
			}
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			if (!IsWorking)
			{
				SetAsProcessing();
				ControlBridge.SetControlEnabledState(btnCreate, false);

				n = BigInteger.Parse(tbN.Text);
				degree = int.Parse(tbDegree.Text);
				polyBase = BigInteger.Parse(tbBase.Text);
				primeBound = BigInteger.Parse(tbBound.Text);

				int relationQuantity = int.Parse(tbRelationQuantity.Text);
				int relationValueRange = int.Parse(tbRelationValueRange.Text);

				logFilename = DirectoryLocations.GetUniqueNameFromN(n) + ".LOG.txt";

				CreateLogFileIfNotExists(logFilename);

				CancellationToken token = cancellationTokenSource.Token;
				new Thread(() =>
				{
					GNFS localGnfs = gnfsBridge.CreateGnfs(token, n, polyBase, degree, primeBound, relationQuantity, relationValueRange);
					SetGnfs(this, localGnfs);
					HaultAllProcessing();
					ControlBridge.SetControlEnabledState(panelFunctions, true);

				}).Start();
			}
		}

		#endregion

	}
}