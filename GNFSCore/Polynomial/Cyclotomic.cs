using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.Polynomial
{
	public class Cyclotomic
	{
		public BigInteger N { get; private set; }

		public Cyclotomic(BigInteger n)
		{
			N = n;
		}
	}
}
