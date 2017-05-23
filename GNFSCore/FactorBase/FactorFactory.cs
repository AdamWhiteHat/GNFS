using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using GNFSCore.Polynomial;
using System.Numerics;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class FactorPairFactory
	{
		public int From { get; private set; }
		public int ToMinimum { get; private set; }
		public int TotalMinimum { get; private set; }
		public int To { get { return _generatedPairs.LastOrDefault().R; } }
		public int Total { get { return _generatedPairs.Count; } }

		private int currentValue;
		private List<int> _primes;
		private AlgebraicPolynomial _poly;
		private List<FactorPair> _generatedPairs;

		public FactorPairFactory(AlgebraicPolynomial polynomial, List<int> primes, int rangeFrom, int rangeTo, int rangeTotal)
		{
			_poly = polynomial;
			_primes = primes;
			From = rangeFrom;
			ToMinimum = rangeTo;
			TotalMinimum = rangeTotal;
			_generatedPairs = new List<FactorPair>();
			currentValue = From;
		}

		public void GenerateMinimum()
		{
			while (currentValue < ToMinimum && _generatedPairs.Count <= TotalMinimum)
			{
				GenerateMore();
			}
		}

		public int GenerateMore()
		{
			BigInteger polyEval = _poly.Evaluate((BigInteger)currentValue);
			IEnumerable<int> roots = PrimeFactory.GetPrimeEnumerator(currentValue).Where(p => (polyEval % p) == 0);

			List<FactorPair> pairs = roots.Select(p => new FactorPair(p, currentValue)).ToList();
			_generatedPairs.AddRange(pairs);
			currentValue++;

			return pairs.Count;
		}

		//IEnumerable<int> GetRange(int start, int count)
		//{
		//	BigInteger polyEval = _poly.Evaluate((BigInteger)r);

		//	IEnumerable<int> modList = _primes.Where(p => p > r);
		//}

	}
}
