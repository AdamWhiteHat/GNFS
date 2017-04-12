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
			foreach(int i in input)
			{
				result *= i;
			}
			return result;
		}

		public static string FormatString<T>(this IEnumerable<T> input)
		{
			if(input == null || input.Count() < 1)
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
