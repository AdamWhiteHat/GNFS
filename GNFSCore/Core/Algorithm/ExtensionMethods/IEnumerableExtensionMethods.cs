﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace GNFSCore.Core.Algorithm.ExtensionMethods
{
	public static class IEnumerableExtensionMethods
	{
		public static int Product(this IEnumerable<int> input)
		{
			int result = 1;
			foreach (int i in input)
			{
				result *= i;
			}
			return result;
		}

		public static double Product(this IEnumerable<double> input)
		{
			double result = 1;
			foreach (double i in input)
			{
				result *= i;
			}
			return result;
		}

		public static string FormatString<U, V>(this IEnumerable<Tuple<U, V>> tuples)
		{
			return string.Join("\t", tuples.Select(tup => $"({tup.Item1},{tup.Item2})"));
		}

		public static string FormatString<T>(this IEnumerable<T[]> input, int padLength = 0)
		{
			if (input == null || input.Count() < 1)
			{
				return $"{{{Environment.NewLine}}}{Environment.NewLine}";
			}
			return
				$"{{{Environment.NewLine}" +
				string.Join("," + Environment.NewLine, input.Select(i => $"\t({string.Join("; ", i.ToString().PadLeft(padLength))})")) +
				$"{Environment.NewLine}}}{Environment.NewLine}";
		}

		public static string FormatString<T>(this IEnumerable<T> input, bool newLine = true, int padding = 0)
		{
			if (input == null || input.Count() < 1)
			{
				return $"{{{Environment.NewLine}}}{Environment.NewLine}";
			}

			string nl = newLine ? Environment.NewLine : "";
			string tab = "".PadLeft(padding);
			string delim = "," + nl;
			return $"{{{nl}" +
					string.Join(delim, input.Select(i => $"{tab}{i.ToString().PadLeft(padding)}")) +
					$"{nl}}}";
		}
	}
}
