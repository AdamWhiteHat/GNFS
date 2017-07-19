using System;
using System.Linq;
using System.Text;
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

	public partial class GnfsUiBridge
	{
		public void CreateGnfs(GNFS gnfs, BigInteger n, BigInteger polyBase, int degree, CancellationToken cancelToken)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}

			if (gnfs == null)
			{
				gnfs = new GNFS(cancelToken, n, polyBase, degree);
			}

			mainForm.BridgeButtonSquares.SetControlEnabledState(true);
			mainForm.BridgeButtonBound.SetControlText(gnfs.PrimeBound.ToString());

			mainForm.LogOutput($"N = {gnfs.N}");
			mainForm.LogOutput();

			mainForm.LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			mainForm.LogOutput(gnfs.CurrentPolynomial.ToString());
			mainForm.LogOutput();


			mainForm.LogOutput("Prime Factor Base Bounds:");
			mainForm.LogOutput($"PrimeBound         : {gnfs.PrimeBound}");
			mainForm.LogOutput($"RationalFactorBase : {gnfs.RationalFactorBase}");
			mainForm.LogOutput($"AlgebraicFactorBase: {gnfs.AlgebraicFactorBase}");
			mainForm.LogOutput($"QuadraticPrimeBase : {gnfs.QuadraticPrimeBase.Last()}");
			mainForm.LogOutput();

			mainForm.LogOutput($"Rational Factor Base (RFB):");
			mainForm.LogOutput(gnfs.RFB.ToString(20));
			mainForm.LogOutput();

			mainForm.LogOutput($"Algebraic Factor Base (AFB):");
			mainForm.LogOutput(gnfs.AFB.ToString(20));
			mainForm.LogOutput();

			mainForm.LogOutput($"Quadratic Factor Base (QFB):");
			mainForm.LogOutput(gnfs.QFB.ToString(20));
			mainForm.LogOutput();

			mainForm.BridgeButtonRelation.SetControlEnabledState(true);
			mainForm.BridgeButtonRelation.SetControlText(MainForm.CancelButtonText);
			mainForm.BridgeButtonGnfs.SetControlEnabledState(false);

			/* FactorCollection gFactors = FactorCollection.Factory.BuildGFactorBase(gnfs);
			mainForm.LogOutput($"g(x) factors: {gFactors.Count}");
			mainForm.LogOutput(gFactors.FormatString());
			mainForm.LogOutput(); */
			return;
		}

	}
}
