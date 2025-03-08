using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using GNFSCore.Core.Algorithm;

namespace GNFSCore.Core.Algorithm.IntegerMath
{
	public class FastPrimeSieve : IEnumerable<BigInteger>
	{
		private static readonly uint PageSize; // L1 CPU cache size in bytes
		private static readonly uint BufferBits;
		private static readonly uint BufferBitsNext;

		static FastPrimeSieve()
		{
			uint cacheSize = 393216;
			List<uint> cacheSizes = CPUInfo.GetCacheSizes(CPUInfo.CacheLevel.Level1);
			if (cacheSizes.Any())
			{
				cacheSize = cacheSizes.First() * 1024;
			}

			PageSize = cacheSize; // L1 CPU cache size in bytes
			BufferBits = PageSize * 8; // in bits
			BufferBitsNext = BufferBits * 2;
		}

		public static IEnumerable<BigInteger> GetRange(BigInteger floor, BigInteger ceiling)
		{
			FastPrimeSieve primesPaged = new FastPrimeSieve();
			IEnumerator<BigInteger> enumerator = primesPaged.GetEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current >= floor)
				{
					break;
				}
			}

			do
			{
				if (enumerator.Current > ceiling)
				{
					break;
				}
				yield return enumerator.Current;
			}
			while (enumerator.MoveNext());

			yield break;
		}

		public IEnumerator<BigInteger> GetEnumerator()
		{
			return Iterator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static IEnumerator<BigInteger> Iterator()
		{
			IEnumerator<BigInteger> basePrimes = null;
			List<uint> basePrimesArray = new List<uint>();
			uint[] cullBuffer = new uint[PageSize / 4]; // 4 byte words

			yield return 2;

			for (var low = (BigInteger)0; ; low += BufferBits)
			{
				for (var bottomItem = 0; ; ++bottomItem)
				{
					if (bottomItem < 1)
					{
						if (bottomItem < 0)
						{
							bottomItem = 0;
							yield return 2;
						}

						BigInteger next = 3 + low + low + BufferBitsNext;
						if (low <= 0)
						{
							// cull very first page
							for (int i = 0, sqr = 9, p = 3; sqr < next; i++, p += 2, sqr = p * p)
							{
								if ((cullBuffer[i >> 5] & 1 << (i & 31)) == 0)
								{
									for (int j = sqr - 3 >> 1; j < BufferBits; j += p)
									{
										cullBuffer[j >> 5] |= 1u << j;
									}
								}
							}
						}
						else
						{
							// Cull for the rest of the pages
							Array.Clear(cullBuffer, 0, cullBuffer.Length);

							if (basePrimesArray.Count == 0)
							{
								// Init second base primes stream
								basePrimes = Iterator();
								basePrimes.MoveNext();
								basePrimes.MoveNext();
								basePrimesArray.Add((uint)basePrimes.Current); // Add 3 to base primes array
								basePrimes.MoveNext();
							}

							// Make sure basePrimesArray contains enough base primes...
							for (BigInteger p = basePrimesArray[basePrimesArray.Count - 1], square = p * p; square < next;)
							{
								p = basePrimes.Current;
								basePrimes.MoveNext();
								square = p * p;
								basePrimesArray.Add((uint)p);
							}

							for (int i = 0, limit = basePrimesArray.Count - 1; i < limit; i++)
							{
								var p = (BigInteger)basePrimesArray[i];
								var start = p * p - 3 >> 1;

								// adjust start index based on page lower limit...
								if (start >= low)
								{
									start -= low;
								}
								else
								{
									var r = (low - start) % p;
									start = r != 0 ? p - r : 0;
								}
								for (var j = (uint)start; j < BufferBits; j += (uint)p)
								{
									cullBuffer[j >> 5] |= 1u << (int)j;
								}
							}
						}
					}

					while (bottomItem < BufferBits && (cullBuffer[bottomItem >> 5] & 1 << (bottomItem & 31)) != 0)
					{
						++bottomItem;
					}

					if (bottomItem < BufferBits)
					{
						var result = 3 + ((BigInteger)bottomItem + low << 1);
						yield return result;
					}
					else break; // outer loop for next page segment...
				}
			}
		}
	}
}
