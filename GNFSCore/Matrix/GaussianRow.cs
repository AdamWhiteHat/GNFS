using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Matrix
{
	using Factors;
	using IntegerMath;

	public class GaussianRow
	{
		public bool Sign { get; set; }

		public List<bool> RationalPart { get; set; }
		public List<bool> AlgebraicPart { get; set; }
		public List<bool> QuadraticPart { get; set; }

		public int LastIndexOfRational { get { return RationalPart.LastIndexOf(true); } }
		public int LastIndexOfAlgebraic { get { return AlgebraicPart.LastIndexOf(true); } }
		public int LastIndexOfQuadratic { get { return QuadraticPart.LastIndexOf(true); } }

		public int RationalLength { get { return RationalPart.Count - 1; } }
		public int AlgebraicLength { get { return AlgebraicPart.Count - 1; } }
		public int QuadraticLength { get { return QuadraticPart.Count - 1; } }

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

			FactorPairCollection qfb = gnfs.QuadradicFactorPairCollection;

			BigInteger rationalMaxValue = gnfs.PrimeFactorBase.RationalFactorBaseMax;
			BigInteger algebraicMaxValue = gnfs.PrimeFactorBase.AlgebraicFactorBaseMax;

			PrimeFactorization ratFactorization = new PrimeFactorization(relation.RationalFactorization, rationalMaxValue);
			PrimeFactorization algFactorization = new PrimeFactorization(relation.AlgebraicFactorization, algebraicMaxValue);

			RationalPart = GetVector(ratFactorization, rationalMaxValue).ToList();
			AlgebraicPart = GetVector(algFactorization, algebraicMaxValue).ToList();
			QuadraticPart = qfb.Select(qf => QuadraticResidue.GetQuadraticCharacter(relation, qf)).ToList();
		}

		protected static bool[] GetVector(PrimeFactorization primeFactorization, BigInteger maxValue)
		{
			int primeIndex = PrimeFactory.GetIndexFromValue(maxValue);

			bool[] result = new bool[primeIndex];
			if (primeFactorization.Any())
			{
				foreach (Factor oddFactor in primeFactorization.Where(f => f.ExponentMod2 == 1))
				{
					if (oddFactor.Prime > maxValue)
					{
						throw new Exception(); // or continue;

					}
					int index = PrimeFactory.GetIndexFromValue(oddFactor.Prime);
					result[index] = true;
				}
			}

			return result.Take(primeIndex).ToArray();
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

