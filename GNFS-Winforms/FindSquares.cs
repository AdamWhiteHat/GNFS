using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.SquareRoot;

	public partial class GnfsUiBridge
	{
		public static GNFS FindSquares(CancellationToken cancelToken, GNFS gnfs)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			BigInteger polyBase = gnfs.PolynomialBase;
			List<List<Relation>> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations;

			bool solutionFound = SquareFinder.Solve(cancelToken, gnfs);



			return gnfs;
		}
	}
}
