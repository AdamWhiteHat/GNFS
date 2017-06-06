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
		public int MaxPrimeBound { get { return Math.Max(Math.Max(RationalFactorBase, AlgebraicFactorBase), PrimeFactory.GetValueFromIndex(QuadraticFactorBaseMax)); } }

		public int RationalFactorBase { get; private set; }
		public int AlgebraicFactorBase { get; private set; }
		public int QuadraticFactorBaseMin { get; private set; }
		public int QuadraticFactorBaseMax { get; private set; }

		public IEnumerable<int> RationalPrimeBase;
		public IEnumerable<int> AlgebraicPrimeBase;
		public IEnumerable<int> QuadraticPrimeBase;

		public FactorCollection RFB { get; internal set; } = null;
		public FactorCollection AFB { get; internal set; } = null;
		public FactorCollection QFB { get; internal set; } = null;


		private int[] _primes;
		private int _degree;


		public GNFS(BigInteger n, BigInteger polynomialBase, int degree = -1)
		{
			N = n;

			// degree = 3 when n <= 10 ^ 60
			// degree = 5 when 10 ^ 60 < n < 10 ^ 180
			_degree = degree;

			if (_degree == -1)
			{
				_degree = CalculateDegree(n);
			}

			CaclulatePrimeBounds();

			ConstructPolynomial(polynomialBase, degree);
			ConstructFactorBase();
		}

		public bool IsFactor(BigInteger toCheck)
		{
			return (N % toCheck == 0);
		}

		// Values were obtained from the paper:
		// "Polynomial Selection for the Number Field Sieve Integer Factorisation Algorithm"
		// by Brian Antony Murphy
		// Table 3.1, page 44
		private int CalculateDegree(BigInteger n)
		{
			int result = 2;
			int base10 = N.ToString().Count();

			if (base10 < 65)
			{
				result = 3;
			}
			else if (base10 >= 65 && base10 < 125)
			{
				result = 4;
			}
			else if (base10 >= 125 && base10 < 225)
			{
				result = 5;
			}
			else if (base10 >= 225 && base10 < 315)
			{
				result = 6;
			}
			else if (base10 >= 315)
			{
				result = 7;
			}

			return result;
		}

		private int CalculateQuadraticBaseSize(int degree)
		{
			int result = -1;

			if (degree < 3)
			{
				result = 10;
			}
			else if (degree == 3 || degree == 4)
			{
				result = 20;
			}
			else if (degree == 5 || degree == 6)
			{
				result = 40;
			}
			else if (degree == 7)
			{
				result = 80;
			}
			else if (degree >= 8)
			{
				result = 100;
			}

			return result;
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

			RationalPrimeBase = PrimeFactory.GetPrimesTo(RationalFactorBase);

			int algebraicQuantity = RationalPrimeBase.Count() * 3;

			AlgebraicFactorBase = PrimeFactory.GetValueFromIndex(algebraicQuantity); //(int)(PrimeBound * 1.1);
			QuadraticFactorBaseMin = AlgebraicFactorBase + 2;
			QuadraticFactorBaseMax = QuadraticFactorBaseMin + base10;

			_primes = PrimeFactory.GetPrimes(MaxPrimeBound);

			AlgebraicPrimeBase = PrimeFactory.GetPrimesTo(AlgebraicFactorBase);

			int quadraticBaseSize = CalculateQuadraticBaseSize(_degree);

			QuadraticPrimeBase = PrimeFactory.GetPrimesFrom(QuadraticFactorBaseMin).Take(quadraticBaseSize);
		}

		private void ConstructPolynomial(BigInteger polynomialBase, int degree)
		{
			Algebraic = new AlgebraicPolynomial(N, polynomialBase, degree);
		}

		private void ConstructFactorBase()
		{
			RFB = FactorCollection.Factory.BuildRationalFactorBase(this);
			AFB = FactorCollection.Factory.GetAlgebraicFactorBase(this);
			QFB = FactorCollection.Factory.GetQuadradicFactorBase(this);
		}

		public Relation[] GenerateRelations(int valueRange, int quantity = -1)
		{
			if (quantity == -1)
			{
				quantity = RFB.Count + AFB.Count + QFB.Count + 1;
			}

			
			BigInteger m = Algebraic.Base;

			int adjustedRange = valueRange % 2 == 0 ? valueRange + 1 : valueRange;
			IEnumerable<int> A =  Enumerable.Range(-adjustedRange, adjustedRange*2);
			int maxB = Math.Max(adjustedRange, quantity) + 2;

			int b = -1;
			List<Relation> result = new List<Relation>();
			while (result.Count() < quantity)
			{
				b += 2;

				IEnumerable<int> coprimes = A.Where(a => CoPrime.IsCoprime(a, b));
				IEnumerable<Relation> unfactored = coprimes.Select(a => new Relation(a, b, this));

				List<Relation> smooth = SieveRelations(unfactored);

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

		public static List<Relation> SieveRelations(IEnumerable<Relation> unfactored)
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
