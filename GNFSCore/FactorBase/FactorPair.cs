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
	public interface IFactorPair
	{
		int P { get; }
		int R { get; }
	}

	public class FactorPair : IFactorPair
	{
		public int P { get; private set; }
		public int R { get; private set; }

		public FactorPair(int p, int r)
		{
			P = p;
			R = r;
		}
	}
}
