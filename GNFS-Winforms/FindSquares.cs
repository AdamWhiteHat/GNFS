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
	using GNFSCore.PrimeSignature;
	using GNFSCore.Polynomial.Internal;

	public partial class GnfsUiBridge
	{
		public void FindSquares(GNFS gnfs, CancellationToken cancelToken)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}

			List<BigInteger> norms = new List<BigInteger>();
			norms.AddRange(gnfs.CurrentRelationsProgress.SmoothRelations.Select(rel => BigInteger.Abs(rel.AlgebraicNorm)));
			norms.AddRange(gnfs.CurrentRelationsProgress.SmoothRelations.Select(rel => BigInteger.Abs(rel.RationalNorm)));

			norms.AddRange(gnfs.CurrentRelationsProgress.SmoothRelations.Select(rel => BigInteger.Abs((BigInteger)rel.A)));
			norms.AddRange(gnfs.CurrentRelationsProgress.SmoothRelations.Select(rel => BigInteger.Abs((BigInteger)rel.B)));
			norms.AddRange(gnfs.CurrentRelationsProgress.SmoothRelations.Select(rel => BigInteger.Abs(rel.C)));

			IEnumerable<BigInteger> squares = norms.Select(bi => BigInteger.Abs(bi)).Distinct();
			squares = squares.Where(bi => bi.IsSquare()).Distinct();

			if (squares.Any())
			{
				//BigInteger squaresProduct = squares.Product();
				//squares.Insert(0, squaresProduct);

				mainForm.LogOutput();
				mainForm.LogOutput("SQUARES FOUND:");
				mainForm.LogOutput(squares.FormatString());
				mainForm.LogOutput();

				SquaresMethod squaresMethod = new SquaresMethod(gnfs.N, squares);

				int maxSteps = 5;
				int counter = 0;
				BigInteger[] factors = new BigInteger[0];
				do
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}

					factors = squaresMethod.Attempt(2);

					counter++;
				}
				while (!factors.Any() && counter < maxSteps);

				//IEnumerable<BigInteger> squaresGCDs = squares.Select(bi => GCD.FindGCD(n, bi));
				//IEnumerable<BigInteger> factors = squaresGCDs.Where(bi => bi > 1);

				if (factors.Any())
				{
					mainForm.LogOutput();
					mainForm.LogOutput("**************** FACTORS FOUND ****************");
					mainForm.LogOutput(factors.FormatString());
					mainForm.LogOutput("**************** FACTORS FOUND ****************");
				}
			}
			else
			{
				MessageBox.Show("No squares found in relations!\nSieve more relations.");
				//List<int> fbSquares = new List<int>();
				//fbSquares.AddRange(gnfs.AFB.Select(pair => pair.R));
				//fbSquares.AddRange(gnfs.RFB.Select(pair => pair.R));
				//fbSquares.AddRange(gnfs.QFB.Select(pair => pair.R));

				//fbSquares = fbSquares.Where(i => Math.Sqrt(i) % 1 == 0).ToList();

				//if (fbSquares.Any())
				//{

				//}
			}
		}
	}
}
