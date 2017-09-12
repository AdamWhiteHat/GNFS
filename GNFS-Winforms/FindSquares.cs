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
	using GNFSCore.FactorBase;
	using GNFSCore.IntegerMath;
	using GNFSCore.Matrix;
	using GNFSCore.Polynomial.Internal;
	using GNFSCore.SquareRoot;

	public partial class GnfsUiBridge
	{
		public GNFS FindSquares(GNFS gnfs, CancellationToken cancelToken)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			SquareFinder squareRootFinder = new SquareFinder(gnfs, gnfs.CurrentRelationsProgress.SmoothRelations);
			squareRootFinder.CalculateRationalSide();
			squareRootFinder.CalculateAlgebraicSide();

			mainForm.LogOutput(squareRootFinder.ToString());

			//BigInteger factor = squareRootFinder.NewtonDirectSqrt();
			//if (factor > 1)
			//{
			//	mainForm.LogOutput($@"+***************\n* FACTOR FOUND *\n\n*              * \n* {factor} *\n****************");
			//	mainForm.LogOutput();
			//}
			//else
			//{
			//	mainForm.LogOutput("Square root method failed to find factor...");
			//}


			return gnfs;
		}
	}
}
