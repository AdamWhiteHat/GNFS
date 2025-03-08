using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

namespace GNFSCore.Core.Data
{
	public class Solution
	{
		[DataMember]
		public BigInteger P { get; private set; }
		[DataMember]
		public BigInteger Q { get; private set; }

		public Solution(BigInteger p, BigInteger q)
		{
			P = p;
			Q = q;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"N = {P * Q}");
			sb.AppendLine();
			sb.AppendLine($"P = {BigInteger.Max(P, Q)}");
			sb.AppendLine($"Q = {BigInteger.Min(P, Q)}");
			sb.AppendLine();

			return sb.ToString();
		}
	}
}
