using System;
using System.Threading;

namespace GNFS_Winforms
{
	using GNFSCore;

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

				gnfs.CurrentRelationsProgress.GenerateRelations(cancelToken);

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
