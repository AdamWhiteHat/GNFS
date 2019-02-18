using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace GNFSCore
{
	public class CountDictionary : IFormattable, IXmlSerializable
	{
		private Dictionary<BigInteger, BigInteger> internalDictionary;

		public IEnumerable<BigInteger> Keys { get { return internalDictionary.Keys; } }
		public BigInteger this[BigInteger key] { get { return internalDictionary.ContainsKey(key) ? internalDictionary[key] : BigInteger.MinusOne; } }

		public CountDictionary()
		{
			internalDictionary = new Dictionary<BigInteger, BigInteger>();
		}

		public void Add(BigInteger key)
		{
			this.Add(key, 1);
		}
		private void Add(BigInteger key, BigInteger value)
		{
			if (!ContainsKey(key)) { internalDictionary.Add(key, value); }
			else { internalDictionary[key] += value; }
		}

		public bool ContainsKey(BigInteger key)
		{
			return internalDictionary.ContainsKey(key);
		}

		public void Order()
		{
			var orderedDictionary = internalDictionary.OrderBy(kvp => kvp.Key);
			internalDictionary = orderedDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		public void Combine(CountDictionary dictionary)
		{
			foreach (var kvp in dictionary.internalDictionary)
			{
				Add(kvp.Key, kvp.Value);
			}
		}

		public Dictionary<BigInteger, BigInteger> ToDictionary()
		{
			return internalDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		#region IFormattable

		public override string ToString()
		{
			Order();

			StringBuilder result = new StringBuilder();
			result.AppendLine("{");
			foreach (KeyValuePair<BigInteger, BigInteger> kvp in internalDictionary)
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
			Order();
			StringBuilder result = new StringBuilder();
			result.Append(
				" -> {\t" +
				string.Join(" * ", internalDictionary.Select(kvp => $"{ kvp.Key}^{ kvp.Value}")) +
				"\t};"
				);
			return result.ToString();
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Serialization

		#region Linq.Xml

		public XElement SerializeToXElement(string elementName)
		{
			XElement result = new XElement(elementName);

			foreach (KeyValuePair<BigInteger, BigInteger> kvp in internalDictionary)
			{
				result.Add(
					new XElement("KeyValuePair",
						new XElement("Key", kvp.Key),
						new XElement("Value", kvp.Value)
					)
				);
			}

			return result;
		}

		public static CountDictionary DeserializeFromXElement(XElement element)
		{
			CountDictionary result = new CountDictionary();

			IEnumerable<XElement> keyValuePairs = element.Elements("KeyValuePair");

			BigInteger key = -1;
			BigInteger value = -1;
			foreach (XElement kvp in keyValuePairs)
			{
				key = BigInteger.Parse(kvp.Element("Key").Value);
				value = BigInteger.Parse(kvp.Element("Value").Value);
				result.internalDictionary.Add(key, value);
			}

			return result;
		}

		#endregion

		#region IXmlSerializable

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Factors");

			foreach (KeyValuePair<BigInteger, BigInteger> kvp in internalDictionary)
			{
				writer.WriteStartElement("Factor");
				writer.WriteElementString("Prime", kvp.Key.ToString());
				writer.WriteElementString("Exponent", kvp.Value.ToString());
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();

			reader.ReadStartElement("Factors");
			do
			{
				reader.ReadStartElement();
				BigInteger key = BigInteger.Parse(reader.ReadElementString("Prime"));
				BigInteger value = BigInteger.Parse(reader.ReadElementString("Exponent"));
				this.Add(key, value);
			}
			while (reader.ReadToNextSibling("Factor"));
			reader.ReadEndElement();

			reader.ReadEndElement();
		}

		public XmlSchema GetSchema() { return null; }

		#endregion

		#endregion

	}

}
