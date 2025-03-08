using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Core.Data
{
	using Interfaces;

	[JsonDictionary]
	public class CountDictionary : SortedDictionary<BigInteger, BigInteger>, ICloneable<Dictionary<BigInteger, BigInteger>>
	{
		public CountDictionary()
			: base(Comparer<BigInteger>.Create(BigInteger.Compare))
		{
		}

		public void Add(BigInteger key)
		{
			AddSafe(key, 1);
		}
		private void AddSafe(BigInteger key, BigInteger value)
		{
			if (!ContainsKey(key)) { Add(key, value); }
			else { this[key] += value; }
		}

		public void Combine(CountDictionary dictionary)
		{
			foreach (var kvp in dictionary)
			{
				AddSafe(kvp.Key, kvp.Value);
			}
		}

		public Dictionary<BigInteger, BigInteger> ToDictionary()
		{
			return this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public Dictionary<BigInteger, BigInteger> Clone()
		{
			return ToDictionary();
		}

		#region String Formatting

		public override string ToString()
		{
			//Order();

			StringBuilder result = new StringBuilder();
			result.AppendLine("{");
			foreach (KeyValuePair<BigInteger, BigInteger> kvp in this)
			{
				result.Append('\t');
				result.Append(kvp.Key.ToString().PadLeft(5));
				result.Append(":\t");
				result.AppendLine(kvp.Value.ToString().PadLeft(5));
			}
			result.Append("}");

			return result.ToString();
		}

		public string FormatStringAsFactorization()
		{
			//Order();
			StringBuilder result = new StringBuilder();
			result.Append(
				" -> {\t" +
				string.Join(" * ", this.Select(kvp => $"{kvp.Key}^{kvp.Value}")) +
				"\t};"
				);
			return result.ToString();
		}

		#endregion
	}

}
