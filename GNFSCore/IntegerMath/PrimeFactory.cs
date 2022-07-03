using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath
{
	using Internal;
	using Newtonsoft.Json.Linq;
	using System.IO;
	using System.Numerics;

	public static class PrimeFactory
	{
		private static List<BigInteger> primes = new List<BigInteger>() { 2, 3, 5, 7, 11, 13 };

		private static readonly string PrimesCache_SearchPath = @"C:\Temp\";
		private static readonly string PrimesCache_GlobbingPattern = "Primes_*.cache";

		public static IEnumerable<BigInteger> GetPrimeEnumerator()
		{
			foreach (string filename in Directory.EnumerateFiles(PrimesCache_SearchPath, PrimesCache_GlobbingPattern).OrderBy(s => s.Length).ThenBy(s => s))
			{
				//Console.WriteLine($"Opening: {filename}");
				foreach (string line in LazyFileReader.ReadLinesIncrementally(filename))
				{
					yield return BigInteger.Parse(line);
				}
			}
			yield break;
		}

		private static class LazyFileReader
		{
			public static IEnumerable<string> ReadLinesIncrementally(string filename)
			{
				FileInfo datafile = new FileInfo(filename);
				if (!datafile.Exists) { throw new FileNotFoundException("File does not exist.", filename); }

				using (FileStream fileStream = datafile.OpenRead())
				{
					using (TextReader textReader = new StreamReader(fileStream))
					{
						string line = null;
						while ((line = textReader.ReadLine()) != null)
						{
							yield return line;
						}
						textReader.Close();
						textReader.Dispose();
					}
				}

				datafile = null;
				yield break;
			}
		}

		public static int GetIndexFromValue(BigInteger n)
		{
			if (n == -1)
			{
				return -1;
			}

			if (n > 0 && n <= 2) { return 1; }
			if (n > 2 && n <= 3) { return 2; }
			if (n > 3 && n <= 5) { return 3; }
			if (n > 5 && n <= 7) { return 4; }
			if (n > 7 && n <= 11) { return 5; }
			if (n > 11 && n <= 13) { return 6; }
			if (n > 13 && n <= 19) { return 8; }
			if (n > 19 && n <= 29) { return 10; }
			if (n > 29 && n <= 43) { return 14; }
			if (n > 43 && n <= 67) { return 19; }
			if (n > 67 && n <= 97) { return 25; }
			if (n > 97 && n <= 149) { return 35; }
			if (n > 149 && n <= 211) { return 47; }
			if (n > 211 && n <= 317) { return 66; }
			if (n > 317 && n <= 479) { return 92; }
			if (n > 479 && n <= 719) { return 128; }
			if (n > 719 && n <= 1069) { return 180; }
			if (n > 1069 && n <= 1601) { return 252; }
			if (n > 1601 && n <= 2399) { return 357; }
			if (n > 2399 && n <= 3607) { return 504; }
			if (n > 3607 && n <= 5399) { return 712; }
			if (n > 5399 && n <= 8093) { return 1018; }
			if (n > 8093 && n <= 12143) { return 1453; }
			if (n > 12143 && n <= 18211) { return 2086; }
			if (n > 18211 && n <= 27329) { return 2990; }
			if (n > 27329 && n <= 40973) { return 4290; }
			if (n > 40973 && n <= 61463) { return 6181; }
			if (n > 61463 && n <= 92173) { return 8900; }
			if (n > 92173 && n <= 138283) { return 12865; }
			if (n > 138283 && n <= 207401) { return 18581; }
			if (n > 207401 && n <= 311099) { return 26885; }
			if (n > 311099 && n <= 466619) { return 38960; }
			if (n > 466619 && n <= 699931) { return 56538; }
			if (n > 699931 && n <= 1049891) { return 82127; }
			if (n > 1049891 && n <= 1574827) { return 119403; }
			if (n > 1574827 && n <= 2362229) { return 173711; }
			if (n > 2362229 && n <= 3543349) { return 252969; }
			if (n > 3543349 && n <= 5314961) { return 368862; }
			if (n > 5314961 && n <= 7972451) { return 538045; }
			if (n > 7972451 && n <= 11958671) { return 785550; }
			if (n > 11958671 && n <= 17937989) { return 1147578; }
			if (n > 17937989 && n <= 26906981) { return 1677592; }
			if (n > 26906981 && n <= 40360471) { return 2454302; }
			if (n > 40360471 && n <= 60540749) { return 3592284; }
			if (n > 60540749 && n <= 90811057) { return 5261347; }
			if (n > 90811057 && n <= 136216589) { return 7710261; }
			if (n > 136216589 && n <= 204324851) { return 11305057; }
			if (n > 204324851 && n <= 306487277) { return 16584598; }
			if (n > 306487277 && n <= 459730919) { return 24341111; }
			if (n > 459730919 && n <= 689596379) { return 35741892; }
			if (n > 689596379 && n <= 1034394589) { return 52505911; }
			if (n > 1034394589 && n <= 1551591841) { return 77166509; }
			if (n > 1551591841 && n <= 2327387749) { return 113454852; }
			if (n > 2327387749 && n <= 3491081621) { return 166873437; }

			double dN = (double)n;
			double lnN = Math.Log(dN);
			double nLogn = dN / lnN;

			// round( (n / ln(n)) * (1 + (1 / ln(n)) + (2 / (ln(n) ^ 2)) + (7.59 / (ln(n) ^ 3))) )

			double a = 1 / lnN;
			double b = 2 / Math.Pow(lnN, 2);
			double c = 7.59 / Math.Pow(lnN, 3);

			double d = 1 + a + b + c;

			double e = nLogn * d;

			return (int)Math.Round(e);
		}

		public static BigInteger GetApproximateValueFromIndex(UInt64 n)
		{
			if (n < 6)
			{
				return primes[(int)n];
			}

			double fn = (double)n;
			double flogn = Math.Log(n);
			double flog2n = Math.Log(flogn);

			double upper;

			if (n >= 688383)    /* Dusart 2010 page 2 */
			{
				upper = fn * (flogn + flog2n - 1.0 + ((flog2n - 2.00) / flogn));
			}
			else if (n >= 178974)    /* Dusart 2010 page 7 */
			{
				upper = fn * (flogn + flog2n - 1.0 + ((flog2n - 1.95) / flogn));
			}
			else if (n >= 39017)    /* Dusart 1999 page 14 */
			{
				upper = fn * (flogn + flog2n - 0.9484);
			}
			else                    /* Modified from Robin 1983 for 6-39016 _only_ */
			{
				upper = fn * (flogn + 0.6000 * flog2n);
			}

			if (upper >= (double)UInt64.MaxValue)
			{
				throw new OverflowException($"{upper} > {UInt64.MaxValue}");
			}

			return new BigInteger((UInt64)Math.Ceiling(upper));
		}

		public static IEnumerable<BigInteger> GetPrimesFrom(BigInteger minValue)
		{
			return GetPrimeEnumerator().SkipWhile(n => n < minValue).ToList();
		}

		public static IEnumerable<BigInteger> GetPrimesTo(BigInteger maxValue)
		{
			bool done = false;
			return GetPrimeEnumerator().TakeWhile(n =>
			{
				bool retVal = done;

				done = n > maxValue;

				return !retVal;
			}).ToList();
		}

		public static IEnumerable<BigInteger> GetPrimesRange(BigInteger minValue, BigInteger maxValue)
		{
			return GetPrimeEnumerator().SkipWhile(n => n < minValue).TakeWhile(n => n <= maxValue).ToList();
		}

		public static BigInteger GetNextPrime(BigInteger fromValue)
		{
			var enumerable = GetPrimeEnumerator().SkipWhile(n => n <= fromValue);
			return enumerable.FirstOrDefault();
		}
	}
}
