using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using GNFSCore.Algorithm.SquareRoot;
using GNFSCore.Data;
using GNFSCore.Data.RelationSieve;

namespace GNFS_Winforms
{

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

			bool solutionFound = SquareRootFinder.Solve(cancelToken, gnfs);



			return gnfs;
		}
	}
}
