
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	using Interfaces;
	public static class ITermExtensionMethods
	{
		public static BigInteger[] GetCoefficients(this ITerm[] source)
		{
			return source.Select(trm => trm.CoEfficient).ToArray();
		}
	}
}
