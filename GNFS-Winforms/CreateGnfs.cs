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
	using GNFSCore.Polynomial.Internal;

	public partial class GnfsUiBridge
	{
		public GNFS CreateGnfs(BigInteger n, BigInteger polyBase, int degree, BigInteger primeBound, int relationQuantity, int relationValueRange, CancellationToken cancelToken)
		{
			GNFS gnfs = new GNFS(cancelToken, n, polyBase, degree, primeBound, relationQuantity, relationValueRange);

			mainForm.LogOutput(gnfs.ToString());

			return gnfs;
		}

	}
}
