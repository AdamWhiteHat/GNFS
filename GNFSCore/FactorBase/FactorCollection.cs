using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using ExtendedNumerics;
using GNFSCore.Polynomial;
using GNFSCore.IntegerMath;

namespace GNFSCore.FactorBase
{
	public class FactorCollection : ICollection<IFactorPair>, IEnumerator<IFactorPair>, IEnumerable<IFactorPair>, IDisposable
	{
		public bool IsReadOnly { get { return false; } }
		public int Count { get { return (internalList == null) ? 0 : internalList.Count(); } }

		public IFactorPair Current { get { AssertIsValid(); return currentValue; } }
		object IEnumerator.Current { get { AssertIsValid(); return currentValue; } }

		private int index;
		private IFactorPair currentValue;
		private List<IFactorPair> internalList;

		public FactorCollection()
			: this(new List<IFactorPair>())
		{
		}

		public FactorCollection(List<IFactorPair> collection)
		{
			Reset();
			internalList = collection;
		}

		protected static List<IFactorPair> PolynomialModP(AlgebraicPolynomial polynomial, IEnumerable<int> primes, int rangeFrom, int rangeTo, int totalFactorPairs)
		{
			List<IFactorPair> result = new List<IFactorPair>();

			int r = rangeFrom;
			while (r < rangeTo && result.Count < totalFactorPairs)
			{
				IEnumerable<int> modList = primes.Where(p => p > r);
				List<int> roots = polynomial.GetRootsMod(r, modList);
				if (roots.Any())
				{
					result.AddRange(roots.Select(p => new FactorPair(p, r)));
				}
				r++;
			}

			return result.OrderBy(tup => tup.P).ToList();
		}

		#region Interfaces Implementation

		public void Reset()
		{
			index = 0;
			currentValue = default(IFactorPair);
		}

		public bool MoveNext()
		{
			if (internalList != null && index < internalList.Count())
			{
				currentValue = internalList[index];
				index++;
				return true;
			}
			index = internalList.Count() + 1;
			currentValue = default(IFactorPair);
			return false;
		}

		public void Add(IFactorPair item)
		{
			if (internalList != null)
			{
				internalList.Add(item);
			}
		}

		public bool Remove(IFactorPair item)
		{
			if (internalList != null && item != null && internalList.Contains(item))
			{
				int location = internalList.IndexOf(item);
				bool result = internalList.Remove(item);
				if (result)
				{
					if (location <= index)
					{
						index -= 1;
						currentValue = internalList[index];
					}
				}
			}
			return false;
		}

		public void Clear()
		{
			if (internalList != null)
			{
				internalList.Clear();
				Reset();
			}
		}

		public bool Contains(IFactorPair item)
		{
			if (internalList == null)
			{
				return false;
			}
			return internalList.Contains(item);
		}

		public void CopyTo(IFactorPair[] array, int arrayIndex)
		{
			if (internalList != null)
			{
				int start = arrayIndex > -1 ? arrayIndex : 0;
				internalList.CopyTo(array, start);
			}
		}

		public IEnumerator<IFactorPair> GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		#endregion

		private void AssertIsValid()
		{
			if (internalList == null)
			{
				throw new ArgumentNullException(nameof(internalList));
			}
			if (index == 0 || index > internalList.Count())
			{
				throw new InvalidOperationException();
			}
		}

		public void Dispose()
		{
			if (internalList != null)
			{
				if (internalList.Count() > 0)
				{
					internalList.Clear();
				}
				internalList = null;
			}
			if (currentValue != null)
			{
				currentValue = null;
			}
		}

		public override string ToString()
		{
			return string.Join("\t", internalList.Select(factr => $"({factr.P},{factr.R})"));
		}
	}
}
