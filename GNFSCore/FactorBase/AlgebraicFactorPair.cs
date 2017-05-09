
using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.IntegerMath;
using GNFSCore.Polynomial;
using ExtendedNumerics;

namespace GNFSCore.FactorBase
{
	public class AlgebraicFactorPair : FactorPair
	{
		public AlgebraicFactorPair(int p, int r)
			: base(p, r)
		{
		}
	}
}
