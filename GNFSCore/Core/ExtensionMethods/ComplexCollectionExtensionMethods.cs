using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore
{
	public static class ComplexCollectionExtensionMethods
	{
		public static Complex Sum(this IEnumerable<Complex> source)
		{
			Complex result = Complex.Zero;
			foreach (Complex bi in source)
			{
				result = Complex.Add(result, bi);
			}

			return result;
		}

		public static Complex Product(this IEnumerable<Complex> input)
		{
			Complex result = Complex.One;
			foreach (Complex bi in input)
			{
				result = Complex.Multiply(result, bi);
			}
			return result;
		}

		public static string FormatString(this Complex source)
		{
			string im = "";
			string sign = "";
			if (Math.Sign(source.Imaginary) == 1)
			{
				sign = " + ";
				im = $"{source.Imaginary}i";
			}
			else if (Math.Sign(source.Imaginary) == -1)
			{
				sign = " - ";
				im = $"{Math.Abs(source.Imaginary)}i";
			}
			return $"({source.Real}{sign}{im})";
		}
	}
}
