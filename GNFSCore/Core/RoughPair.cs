using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore
{
	public class RoughPair
	{
		public int A { get; private set; }
		public int B { get; private set; }
		public BigInteger AlgebraicQuotient { get; private set; }
		public BigInteger RationalQuotient { get; private set; }

		public RoughPair(Relation relation)
		{
			A = relation.A;
			B = relation.B;
			AlgebraicQuotient = relation.AlgebraicQuotient;
			RationalQuotient = relation.RationalQuotient;
		}

		public Relation Combine(RoughPair rough)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return $"[({A},{B}) => \"{AlgebraicQuotient},{RationalQuotient}\"]";
		}
	}
}
