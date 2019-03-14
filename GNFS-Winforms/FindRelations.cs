using System;
using System.Threading;

namespace GNFS_Winforms
{
	using GNFSCore;

	public partial class GnfsUiBridge
	{
		public GNFS FindRelations(bool oneRound, GNFS gnfs, CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
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
