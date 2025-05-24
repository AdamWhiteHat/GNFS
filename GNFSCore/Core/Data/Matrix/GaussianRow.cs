using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Data.Matrix
{
	using RelationSieve;
	using Algorithm.IntegerMath;

	public class GaussianRow
	{
		public bool Sign { get; set; }

		public List<bool> RationalPart { get; set; }
		public List<bool> AlgebraicPart { get; set; }
		public List<bool> QuadraticPart { get; set; }

		public int LastIndexOfRational { get { return RationalPart.LastIndexOf(true); } }
		public int LastIndexOfAlgebraic { get { return AlgebraicPart.LastIndexOf(true); } }
		public int LastIndexOfQuadratic { get { return QuadraticPart.LastIndexOf(true); } }

		public Relation SourceRelation { get; private set; }

		public GaussianRow(GNFS gnfs, Relation relation)
		{
			SourceRelation = relation;

			if (relation.RationalNorm.Sign == -1)
			{
				Sign = true;
			}
			else
			{
				Sign = false;
			}

			FactorPairCollection qfb = gnfs.QuadraticFactorPairCollection;

			BigInteger rationalMaxValue = gnfs.PrimeFactorBase.RationalFactorBaseMax;
			BigInteger algebraicMaxValue = gnfs.PrimeFactorBase.AlgebraicFactorBaseMax;

			RationalPart = GetVector(relation.RationalFactorization, rationalMaxValue).ToList();
			AlgebraicPart = GetVector(relation.AlgebraicFactorization, algebraicMaxValue).ToList();
			QuadraticPart = qfb.Select(qf => QuadraticResidue.GetQuadraticCharacter(relation, qf)).ToList();
		}

		protected static bool[] GetVector(CountDictionary primeFactorizationDict, BigInteger maxValue)
		{
			int primeIndex = PrimeFactory.GetIndexFromValue(maxValue);

			bool[] result = new bool[primeIndex];

			if (primeFactorizationDict.Any())
			{
				foreach (KeyValuePair<BigInteger, BigInteger> kvp in primeFactorizationDict)
				{
					if (kvp.Key > maxValue)
					{
						continue;
					}
					if (kvp.Key == -1)
					{
						continue;
					}
					if (kvp.Value % 2 == 0)
					{
						continue;
					}

					int index = PrimeFactory.GetIndexFromValue(kvp.Key);
					result[index] = true;
				}
			}

			return result;
		}

		public bool[] GetBoolArray()
		{
			List<bool> result = new List<bool>() { Sign };
			result.AddRange(RationalPart);
			result.AddRange(AlgebraicPart);
			result.AddRange(QuadraticPart);
			//result.Add(false);
			return result.ToArray();
		}

		public void ResizeRationalPart(int size)
		{
			RationalPart = RationalPart.Take(size + 1).ToList();
		}

		public void ResizeAlgebraicPart(int size)
		{
			AlgebraicPart = AlgebraicPart.Take(size + 1).ToList();
		}

		public void ResizeQuadraticPart(int size)
		{
			QuadraticPart = QuadraticPart.Take(size + 1).ToList();
		}
	}
}

