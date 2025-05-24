using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using GNFSCore.Data;
using GNFSCore.Algorithm.Matrix;

namespace GNFS_Winforms
{
	public partial class GnfsUiBridge
	{
		public static GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			MatrixSolver.GaussianSolve(cancelToken, gnfs);
			return gnfs;
		}
	}
}
