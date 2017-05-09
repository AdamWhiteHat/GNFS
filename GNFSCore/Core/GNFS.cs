using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.FactorBase;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public partial class GNFS
	{
		public BigInteger N { get; private set; }
		public Relation[] Relations { get; private set; }
		public AlgebraicPolynomial Algebraic { get; private set; }

		public int PrimeBound { get; private set; }
		public int MaxPrimeBound { get { return Math.Max(Math.Max(RationalFactorBase, AlgebraicFactorBase), PrimeFactory.GetValueFromIndex(QuadraticFactorBaseIndexMax)); } }

		public int RationalFactorBase { get; private set; }
		public int AlgebraicFactorBase { get; private set; }
		public int QuadraticFactorBaseIndexMin { get; private set; }
		public int QuadraticFactorBaseIndexMax { get; private set; }

		public IEnumerable<int> RationalPrimeBase;
		public IEnumerable<int> AlgebraicPrimeBase;
		public IEnumerable<int> QuadraticPrimeBase;

		public RationalFactorCollection RFB { get; internal set; } = null;
		public AlgebraicFactorCollection AFB { get; internal set; } = null;
		public QuadraticFactorCollection QFB { get; internal set; } = null;


		private int[] _primes;
		private int _degree;


		public GNFS(BigInteger n, BigInteger polynomialBase, int degree)
		{
			N = n;

			// degree = 3 when n <= 10 ^ 60
			// degree = 5 when 10 ^ 60 < n < 10 ^ 180
			_degree = degree;

			CaclulatePrimeBounds();

			ConstructPolynomial(polynomialBase, degree);
			ConstructFactorBase();
		}

		public bool IsFactor(BigInteger toCheck)
		{
			return (N % toCheck == 0);
		}

		public IEnumerable<int> GetPrimes(int maxValue)
		{
			int index = PrimeFactory.GetIndexFromValue(maxValue);
			return _primes.Take(index);
		}

		private void CaclulatePrimeBounds()
		{
			//BigInteger remainder = new BigInteger();
			int base10 = N.ToString().Count(); //N.NthRoot(10, ref remainder);

			if (base10 <= 18)
			{
				PrimeBound = base10 * 10;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 < 100)
			{
				PrimeBound = 100000;
			}
			else if (base10 > 100 && base10 < 150)
			{
				PrimeBound = 250000;
			}
			else if (base10 > 150 && base10 < 200)
			{
				PrimeBound = 125000000;
			}
			else if (base10 > 200)
			{
				PrimeBound = 250000000;
			}

			PrimeBound *= 3;

			RationalFactorBase = PrimeBound;

			_primes = PrimeFactory.GetPrimes(RationalFactorBase * 3);

			RationalPrimeBase = GetPrimes(RationalFactorBase);

			int algebraicQuantity = RationalPrimeBase.Count() * 3;

			AlgebraicFactorBase = PrimeFactory.GetValueFromIndex(algebraicQuantity); //(int)(PrimeBound * 1.1);
			QuadraticFactorBaseIndexMin = PrimeFactory.GetIndexFromValue(AlgebraicFactorBase) + 1;
			QuadraticFactorBaseIndexMax = QuadraticFactorBaseIndexMin + base10;

			_primes = PrimeFactory.GetPrimes(MaxPrimeBound);

			AlgebraicPrimeBase = GetPrimes(AlgebraicFactorBase);
			QuadraticPrimeBase = _primes.Skip(QuadraticFactorBaseIndexMin);
		}

		private void ConstructPolynomial(BigInteger polynomialBase, int degree)
		{
			Algebraic = new AlgebraicPolynomial(N, polynomialBase, degree);
		}

		private void ConstructFactorBase()
		{
			RFB = RationalFactorCollection.Factory.BuildRationalFactorBase(this);
			AFB = AlgebraicFactorCollection.Factory.GetAlgebraicFactorBase(this);
			QFB = QuadraticFactorCollection.Factory.GetQuadradicFactorBase(this);
		}

		public Relation[] GenerateRelations(int valueRange, int quantity = -1)
		{
			List<Relation> result = new List<Relation>();

			int b = -1;
			BigInteger m = Algebraic.Base;
			if (quantity == -1)
			{
				quantity = RFB.Count + AFB.Count + QFB.Count + 1;
			}
			IEnumerable<int> A = Enumerable.Range(2, valueRange * 2);

			IEnumerable<int> pRational = RFB.Select(tupl => tupl.P).OrderBy(i => i);
			IEnumerable<int> pAlgebraic = AFB.Select(tupl => tupl.P).OrderBy(i => i).Distinct();

			int maxB = Math.Max(valueRange, quantity) + 2;

			while (result.Count() < quantity)
			{
				b += 2;

				IEnumerable<int> coprimes = A.Where(a => CoPrime.IsCoprime(a, b));
				IEnumerable<Relation> unfactored = coprimes.Select(a => new Relation(a, b, this));

				List<Relation> smooth = SieveRelations(unfactored, pRational, pAlgebraic);

				if (smooth.Any())
				{
					result.AddRange(smooth);
				}

				if (b > maxB)
				{
					break;
				}
			}

			Relations = result.OrderBy(rel => rel.B).ThenBy(rel => rel.A).ToArray();
			return Relations;
		}

		public static List<Relation> SieveRelations(IEnumerable<Relation> unfactored, IEnumerable<int> pRational, IEnumerable<int> pAlgebraic)
		{
			List<Relation> results = new List<Relation>();

			foreach (Relation rel in unfactored)
			{
				rel.Sieve();
				bool smooth = rel.IsSmooth;
				if (smooth)
				{
					results.Add(rel);
				}
			}

			return results;
		}
	}
}
