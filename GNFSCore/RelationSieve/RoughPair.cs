using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using GNFSCore.IntegerMath;

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
			AlgebraicQuotient = BigInteger.Abs(relation.AlgebraicQuotient);
			RationalQuotient = BigInteger.Abs(relation.RationalQuotient);
		}

		public RoughPair(string line)
		{
			string[] parts = line.Split(new char[] { '|' });
			string[] ab = parts[0].Split(new char[] { ',' });
			string[] quotients = parts[1].Split(new char[] { ',' });

			A = int.Parse(ab[0]);
			B = int.Parse(ab[1]);

			AlgebraicQuotient = BigInteger.Parse(quotients[0]);
			RationalQuotient = BigInteger.Parse(quotients[1]);
		}

		public Relation Combine(RoughPair rough)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return $"{A},{B}|{AlgebraicQuotient},{RationalQuotient}";
		}

		public static List<RoughPair> LoadFromFile(string filepath)
		{
			List<RoughPair> result = new List<RoughPair>();

			string[] lines = File.ReadAllLines(filepath);

			return lines.Select(ln => new RoughPair(ln)).ToList();
		}

		public static void SaveToFile(string filepath, List<RoughPair> roughpairCollection)
		{
			File.WriteAllLines(filepath, roughpairCollection.Select(rp => rp.ToString()));
		}
	}
}
