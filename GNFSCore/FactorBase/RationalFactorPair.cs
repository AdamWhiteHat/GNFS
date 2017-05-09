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
	public class RationalFactorPair : FactorPair
	{
		public RationalFactorPair(int p, int r)
			: base(p, r)
		{
		}

	}
}
