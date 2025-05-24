using System;
using System.Threading;
using GNFSCore;
using GNFSCore.Data;

namespace GNFS_Winforms
{
	public partial class GnfsUiBridge
	{
		public static GNFS FindRelations(CancellationToken cancelToken, GNFS gnfs, bool oneRound)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				if (gnfs.CurrentRelationsProgress.SmoothRelationsCounter >= gnfs.CurrentRelationsProgress.SmoothRelations_TargetQuantity)
				{
					gnfs.CurrentRelationsProgress.IncreaseTargetQuantity(100);
				}

				gnfs.CurrentRelationsProgress.GenerateRelations(cancelToken, gnfs);

				Logging.LogMessage();
				Logging.LogMessage($"Sieving progress saved at:");
				Logging.LogMessage($"   A = {gnfs.CurrentRelationsProgress.A}");
				Logging.LogMessage($"   B = {gnfs.CurrentRelationsProgress.B}");
				Logging.LogMessage();

				if (oneRound)
				{
					break;
				}

				if (gnfs.CurrentRelationsProgress.SmoothRelationsCounter >= gnfs.CurrentRelationsProgress.SmoothRelations_TargetQuantity)
				{
					break;
				}
			}

			return gnfs;
		}
	}
}
