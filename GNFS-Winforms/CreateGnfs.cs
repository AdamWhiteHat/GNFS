using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace GNFS_Winforms
{
	using GNFSCore;

	public partial class GnfsUiBridge
	{
		public static GNFS CreateGnfs(CancellationToken cancelToken, BigInteger n, BigInteger polyBase, int degree, BigInteger primeBound, int relationsTargetQuantity, int relationValueRange)
		{
			GNFS gnfs = new GNFS(cancelToken, Logging.LogMessage, n, polyBase, degree, primeBound, relationsTargetQuantity, relationValueRange);
			return gnfs;
		}

		public static GNFS LoadGnfs(BigInteger n)
		{
			string jsonFilename = Path.Combine(DirectoryLocations.GetSaveLocation(n), "GNFS.json");
			GNFS gnfs = Serialization.Load.All(jsonFilename);
			gnfs.LogFunction = Logging.LogMessage;
			return gnfs;
		}
	}
}
