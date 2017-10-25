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

			// root, rootSet[], rootProduct, rootProduct % ƒ, ƒ(x)
			List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>> rootProductModTuples = squareRootFinder.CalculateRootProducts();

			int rootPadLength = rootProductModTuples.Select(tup => tup.Item1).Max().ToString().Length;
			int productPadLength = rootProductModTuples.Select(tup => BigInteger.Abs(tup.Item4)).Max().ToString().Length + 1;

			char tab = '\t';
			string rootProductsString = string.Join(Environment.NewLine,
				// root, rootSet.ToArray(), rootProduct, rootProduct % f, f
				rootProductModTuples.Select(tup => $"a + b∙{tup.Item1.ToString().PadRight(rootPadLength)} = {tup.Item3}{tab}≡{tab}{tup.Item4.ToString().PadLeft(productPadLength)} (mod {tup.Item5})")); //[{tup.Item2.FormatString(false)}]






			mainForm.LogOutput("Root products:");
			mainForm.LogOutput("( (a₁ + b₁∙x) * … * (aᵢ + bᵢ∙x) ) mod ƒ(x) =");
			mainForm.LogOutput(rootProductsString);
			mainForm.LogOutput();

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
