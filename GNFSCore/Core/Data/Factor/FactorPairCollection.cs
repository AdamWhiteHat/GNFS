using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Core.Data
{
	using GNFSCore.Core.Data;
	using Interfaces;
	public class FactorPairCollection : List<FactorPair>
	{
		public FactorPairCollection()
			: base()
		{
		}

		public FactorPairCollection(IEnumerable<FactorPair> collection)
			: base(collection)
		{
		}

		public override string ToString()
		{
			return string.Join("\t", this.Select(factr => factr.ToString()));
		}

		public string ToString(int take)
		{
			return string.Join("\t", this.Take(take).Select(factr => factr.ToString()));
		}
	}
}
