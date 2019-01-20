using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.IntegerMath
{
	public static class Combinatorics
	{
		/// <summary>
		/// Returns the Cartesian product of two or more lists
		/// </summary>
		public static List<List<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> empty = new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate
					(
						empty,
						(first, second) =>
							from a in first
							from b in second
							select a.Concat(new[] { b })
					)
					.Select(lst => lst.ToList())
					.ToList();
		}
	}
}
