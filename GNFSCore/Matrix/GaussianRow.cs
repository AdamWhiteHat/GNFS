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

			BigInteger max = BigInteger.Max(rationalMaxValue, algebraicMaxValue);

			List<BigInteger>  primes = PrimeFactory.GetPrimesTo(max+1).ToList();

			RationalPart = GetVector(relation.RationalFactorization, rationalMaxValue, primes).ToList();
			AlgebraicPart = GetVector(relation.AlgebraicFactorization, algebraicMaxValue, primes).ToList();
			QuadraticPart = qfb.Select(qf => QuadraticResidue.GetQuadraticCharacter(relation, qf)).ToList();
		}

		protected static bool[] GetVector(CountDictionary primeFactorizationDict, BigInteger maxValue, List<BigInteger> primes)
		{
			BigInteger prime = primes.SkipWhile(n => n < maxValue).First();

			int size = primes.IndexOf(prime) + 1;

			bool[] result = new bool[size];

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

					int index = primes.IndexOf(kvp.Key);
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

