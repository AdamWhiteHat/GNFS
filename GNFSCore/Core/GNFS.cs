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
		public int PrimeBound { get; private set; }
		public Irreducible AlgebraicPolynomial { get; private set; }
		public IEnumerable<Tuple<int, int>> RFB { get; internal set; } = null;
		public IEnumerable<Tuple<int, int>> AFB { get; internal set; } = null;
		public IEnumerable<Tuple<int, int>> QFB { get; internal set; } = null;

		public GNFS(BigInteger n, BigInteger polynomialBase, int degree)
		{
			N = n;
			//degree = 3; // or 4
			BigInteger remainder = new BigInteger();
			PrimeBound = (int)((int)n.NthRoot(degree, ref remainder) * 1.5); // 60;

			ConstructPolynomial(polynomialBase, degree);
			ConstructFactorBase();
		}

		private void ConstructPolynomial(BigInteger polynomialBase, int degree)
		{
			AlgebraicPolynomial = new Irreducible(N, polynomialBase, degree);
		}

		private void ConstructFactorBase()
		{
			RFB = Rational.Factory.BuildRationalFactorBase(this);
			AFB = Algebraic.Factory.GetAlgebraicFactorBase(this);
			QFB = Quadradic.Factory.GetQuadradicFactorBase(this);
		}

		internal static IEnumerable<Tuple<int, int>> PolynomialModP(Irreducible poly, IEnumerable<int> primes, IEnumerable<int> integers)
		{
			List<Tuple<int, int>> result = new List<Tuple<int, int>>();

			foreach (int r in integers)
			{
				var modList = primes.Where(p => p > r);
				var roots = poly.GetRootsMod(r, modList);
				if (roots.Any())
				{
					result.AddRange(roots.Select(p => new Tuple<int, int>(p, r)));
				}
			}

			return result.OrderBy(tup => tup.Item1);
		}

		public Relation[] GenerateRelations(int range)
		{
			List<Relation> result = new List<Relation>();

			int b = 0;
			BigInteger m = AlgebraicPolynomial.Base;
			int quantity = RFB.Count() + AFB.Count();// + QFB.Count();
			IEnumerable<int> A = Enumerable.Range(-range, range * 2);

			IEnumerable<int> pRational = RFB.Select(tupl => tupl.Item1).OrderBy(i => i);
			IEnumerable<int> pAlgebraic = AFB.Select(tupl => tupl.Item1).OrderBy(i => i).Distinct();

			while (result.Count() < quantity)
			{
				b += 1;

				IEnumerable<int> coprimes = A.Where(a => CoPrime.IsCoprime(a, b));
				IEnumerable<Relation> unfactored = coprimes.Select(a => new Relation(a, b, AlgebraicPolynomial));

				List<Relation> smooth = new List<Relation>();

				foreach (Relation rel in unfactored)
				{
					rel.RemoveRationalFactors(pRational);
					rel.RemoveAlgebraicFactors(pAlgebraic);
					bool smth = rel.IsSmooth;
					if (smth)
					{
						smooth.Add(rel);
					}
				}

				if (smooth.Any())
				{
					result.AddRange(smooth);
				}

				if (b > range)
				{
					break;
				}
			}

			return result.ToArray();
		}
	}
}
