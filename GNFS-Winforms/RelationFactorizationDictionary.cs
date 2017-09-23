using System;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;

	public class RelationFactorizationDictionary
	{
		private Dictionary<BigInteger, List<Relation>> internalDictionary;

		public RelationFactorizationDictionary()
		{
			internalDictionary = new Dictionary<BigInteger, List<Relation>>();
		}

		public void Add(BigInteger relation, List<BigInteger> factorization)
		{

		}
	}
}
