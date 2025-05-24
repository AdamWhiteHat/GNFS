using System;
using System.Numerics;
using Newtonsoft.Json;

namespace GNFSCore.Data
{
	public struct FactorPair : IEquatable<FactorPair>
	{
		[JsonProperty]
		public int P { get; private set; }
		[JsonProperty]
		public int R { get; private set; }

		public FactorPair(BigInteger p, BigInteger r)
		{
			P = (int)p;
			R = (int)r;
		}

		public FactorPair(int p, int r)
		{
			P = p;
			R = r;
		}

		public override int GetHashCode()
		{
			return CombineHashCodes(P, R);
		}

		private static int CombineHashCodes(int h1, int h2)
		{
			return (h1 << 5) + h1 ^ h2;
		}

		public override bool Equals(object obj)
		{
			return obj is FactorPair && Equals((FactorPair)obj);
		}

		public bool Equals(FactorPair other)
		{
			return this == other;
		}

		public static bool operator !=(FactorPair left, FactorPair right)
		{
			return !(left == right);
		}

		public static bool operator ==(FactorPair left, FactorPair right)
		{
			return left.P == right.P && left.R == right.R;
		}

		public override string ToString()
		{
			return $"({P},{R})";
		}
	}
}
