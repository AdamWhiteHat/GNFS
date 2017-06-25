using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore
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

		public static string FormatString<T>(this IEnumerable<T[]> input)
		{
			if (input == null || input.Count() < 1)
			{
				return $"{{{Environment.NewLine}}}{Environment.NewLine}";
			}
			return
				$"{{{Environment.NewLine}" +
				string.Join("," + Environment.NewLine, input.Select(i => $"\t({string.Join("; ", i.ToString())})")) +
				$"{Environment.NewLine}}}{Environment.NewLine}";
		}

		public static string FormatString<T>(this IEnumerable<T> input)
		{
			if (input == null || input.Count() < 1)
			{
				return $"{{{Environment.NewLine}}}{Environment.NewLine}";
			}
			return
				$"{{{Environment.NewLine}" +
				string.Join(Environment.NewLine, input.Select(i => $"\t{i.ToString()}")) +
				$"{Environment.NewLine}}}{Environment.NewLine}";
		}
	}
}
