using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GNFSCore.FactorBase
{
	public class Rational
	{
		// m = polynomial base
		// array of (p, p mod m) up to bound
		// quantity = phi(bound)

		public static IEnumerable<Tuple<int, int>> GetRationalFactorBase(BigInteger polynomialBase, int bound)
		{
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();




			return result;
		}
	}
}
