using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.PrimeSignature
{
	public class CombinedVector
	{
		private BitVector[] Vectors;

		public CombinedVector(BitVector[] vectors)
		{
			Vectors = vectors;
		}

		public override string ToString()
		{
			return $"{string.Join(",", Vectors.Select(bv => BitVector.FormatElements(bv.Elements)))} | [{string.Join(";", Vectors.Select(bv => bv.Number.ToString()))}]";
		}
	}
}
