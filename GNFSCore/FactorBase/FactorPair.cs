﻿using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	[Serializable]
	public struct FactorPair : IEquatable<FactorPair>
	{
		public int P { get; }
		public int R { get; }

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
			return (((h1 << 5) + h1) ^ h2);
		}

		public override bool Equals(object obj)
		{
			return (obj is FactorPair && this.Equals((FactorPair)obj));
		}

		public bool Equals(FactorPair other)
		{
			return (this == other);
		}

		public static bool operator !=(FactorPair left, FactorPair right)
		{
			return !(left == right);
		}

		public static bool operator ==(FactorPair left, FactorPair right)
		{
			return (left.P == right.P && left.R == right.R);
		}

		public override string ToString()
		{
			return $"({P},{R})";
		}
	}
}