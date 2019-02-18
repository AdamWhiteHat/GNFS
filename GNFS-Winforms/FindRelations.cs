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
	using GNFSCore.Matrix;
	using GNFSCore.Factors;
	using GNFSCore.Polynomials;
	using GNFSCore.IntegerMath;

	public partial class GnfsUiBridge
	{
		public GNFS FindRelations(bool oneRound, GNFS gnfs, CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				//List<RoughPair> knownRough = gnfs.CurrentRelationsProgress.RoughRelations;
				gnfs.CurrentRelationsProgress.GenerateRelations(cancelToken);

				mainForm.LogOutput();
				mainForm.LogOutput($"Sieving progress saved at:");
				mainForm.LogOutput($"   A = {gnfs.CurrentRelationsProgress.A}");
				mainForm.LogOutput($"   B = {gnfs.CurrentRelationsProgress.B}");
				mainForm.LogOutput();

				if (oneRound)
				{
					break;
				}
			}

			return gnfs;
		}
	}
}
