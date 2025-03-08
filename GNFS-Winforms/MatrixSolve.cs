using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.Matrix;

	public partial class GnfsUiBridge
	{
		public static GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			MatrixSolver.GaussianSolve(cancelToken, gnfs);
			return gnfs;
		}
	}
}
