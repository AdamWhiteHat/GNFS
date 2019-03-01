using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace GNFS_Winforms
{
	using GNFSCore;

	public partial class GnfsUiBridge
	{
		public GNFS CreateGnfs(CancellationToken cancelToken, BigInteger n, BigInteger polyBase, int degree, BigInteger primeBound, int relationQuantity, int relationValueRange)
		{
			GNFS gnfs = new GNFS(cancelToken, n, polyBase, degree, primeBound, relationQuantity, relationValueRange);

			mainForm.LogOutput(gnfs.ToString());

			return gnfs;
		}

		public GNFS LoadGnfs(CancellationToken cancelToken, BigInteger n)
		{
			GNFS gnfs = new GNFS(cancelToken, n);

			mainForm.LogOutput(gnfs.ToString());

			return gnfs;
		}

	}
}
