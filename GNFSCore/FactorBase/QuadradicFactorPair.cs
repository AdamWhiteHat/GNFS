using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GNFSCore.IntegerMath;
using GNFSCore.Polynomial;

namespace GNFSCore.FactorBase
{
	public class QuadradicFactorPair : FactorPair
	{
		public QuadradicFactorPair(int p, int r)
			: base(p, r)
		{
		}
	}
}
