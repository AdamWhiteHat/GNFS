using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
{
	public class RelationCollection
	{
		public IEnumerable<int> Items { get; set; }
		GNFS Gnfs;
		int Range;

		public RelationCollection(GNFS gnfs, int range)
		{
			Gnfs = gnfs;
			Range = range;

			Items = Enumerable.Range(-range, range * 2);
		}
	}
}
