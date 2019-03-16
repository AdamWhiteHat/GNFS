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
		public GNFS CreateGnfs(CancellationToken cancelToken, BigInteger n, BigInteger polyBase, int degree, BigInteger primeBound, int relationQuantity, int relationValueRange)
		{
			GNFS gnfs = new GNFS(cancelToken, n, polyBase, degree, primeBound, relationQuantity, relationValueRange);
			return gnfs;
		}

		public GNFS LoadGnfs(CancellationToken cancelToken, BigInteger n)
		{
			string jsonFilename = Path.Combine(DirectoryLocations.GetSaveLocation(n), "GNFS.json");
			GNFS gnfs = Serialization.Load.Gnfs(cancelToken, jsonFilename);
			return gnfs;
		}

	}
}
