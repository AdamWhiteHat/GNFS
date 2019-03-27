using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Matrix;

	public partial class GnfsUiBridge
	{
		public static GNFS MatrixSolveGaussian(GNFS gnfs)
		{
			MatrixSolve.GaussianSolve(gnfs);
			return gnfs;
		}
	}
}
