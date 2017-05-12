using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public struct FactorPair
	{
		public int P;
		public int R;

		public FactorPair(int p, int r)
		{
			P = p;
			R = r;
		}
	}
}
