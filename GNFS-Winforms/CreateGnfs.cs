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
	using GNFSCore.Factors;
	using GNFSCore.IntegerMath;
	using GNFSCore.Matrix;

	public partial class GnfsUiBridge
	{
		public GNFS CreateGnfs(CancellationToken cancelToken, BigInteger n, BigInteger polyBase, int degree, BigInteger primeBound, int relationQuantity, int relationValueRange)
		{
			GNFS gnfs = new GNFS(cancelToken, n, polyBase, degree, primeBound, relationQuantity, relationValueRange);

			mainForm.LogOutput(gnfs.ToString());

			return gnfs;
		}

		public GNFS CreateGnfs(CancellationToken cancelToken, BigInteger n)
		{
			GNFS gnfs = new GNFS(cancelToken, n);

			mainForm.LogOutput(gnfs.ToString());

			return gnfs;
		}

	}
}
