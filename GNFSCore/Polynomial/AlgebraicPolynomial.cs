using System;
using System.Xml;
using System.Linq;
using System.Numerics;
using System.Xml.Schema;
using System.Collections;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using GNFSCore.IntegerMath;

namespace GNFSCore.Polynomial
{
	using Internal;

	[Serializable]
	public class AlgebraicPolynomial : IPolynomial, IXmlSerializable
	{
		public int Degree { get; private set; }

		[XmlArrayItem("Terms")]
		public BigInteger[] Terms { get; private set; }

		public AlgebraicPolynomial()
		{
			Degree = 0;
			Terms = new BigInteger[0];
		}

		public AlgebraicPolynomial(BigInteger[] terms)
		{
			BigInteger[] safeTerms = terms.ToArray();
			if (safeTerms.Any() && safeTerms.Count() > 1 && safeTerms.Last() == 0)
			{
				safeTerms = CommonPolynomial.RemoveZeros(safeTerms);
			}

			Terms = safeTerms;
			Degree = Terms.Length - 1;
		}

		public AlgebraicPolynomial(BigInteger n, BigInteger polynomialBase, int degree)
		{
			Degree = degree;
			Terms = GetPolynomialTerms(n, polynomialBase, degree);
		}

		private static BigInteger[] GetPolynomialTerms(BigInteger value, BigInteger polynomialBase, int degree)
		{
			int d = degree;
			BigInteger toAdd = value;

			List<BigInteger> result = new List<BigInteger>();
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(polynomialBase, d);

				if (placeValue == 1)
				{
					result.Add(toAdd);
				}
				else if (placeValue < BigInteger.Abs(toAdd))
				{
					BigInteger remainder = 0;
					BigInteger quotient = BigInteger.DivRem(toAdd, placeValue, out remainder);

					if (quotient > placeValue)
					{
						quotient = placeValue;
					}

					result.Add(quotient);
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);
					toAdd -= toSubtract;
				}

				d--;
			}

			return result.ToArray();
		}

		public double Evaluate(double baseM)
		{
			return CommonPolynomial.Evaluate(this, baseM);
		}

		public BigInteger Evaluate(BigInteger baseM)
		{
			return CommonPolynomial.Evaluate(this, baseM);
		}

		public BigInteger Derivative(BigInteger baseM)
		{
			return CommonPolynomial.Derivative(this, baseM);
		}

		public List<BigInteger> GetRootsMod(BigInteger baseM, IEnumerable<BigInteger> modList)
		{
			return CommonPolynomial.GetRootsMod(this, baseM, modList);
		}

		public IEnumerable<BigInteger> GetRootsModEnumerable(BigInteger baseM, IEnumerable<BigInteger> modList)
		{
			BigInteger polyResult = Evaluate(baseM);
			IEnumerable<BigInteger> primeList = modList;

			foreach (BigInteger mod in primeList)
			{
				if ((polyResult % mod) == 0)
				{
					yield return mod;
				}
			}

			yield break;
		}

		public IPolynomial Clone()
		{
			return new AlgebraicPolynomial(Terms.ToArray());
		}

		public override string ToString()
		{
			return CommonPolynomial.FormatString(this);
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("Degree", Degree.ToString());
			writer.WriteStartElement("Terms");
			writer.WriteString(Environment.NewLine + "    " + string.Join(Environment.NewLine + "    ", Terms.Select(t => t.ToString())) + Environment.NewLine + "  ");
			writer.WriteEndElement();
			writer.WriteElementString("Polynomial", this.ToString());
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();

			Degree = int.Parse(reader.ReadElementString("Degree"));

			reader.ReadStartElement("Terms");
			string val = reader.Value;
			if (!string.IsNullOrWhiteSpace(val))
			{
				val = val.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\f", "");
			}
			reader.Read();
			reader.ReadEndElement();

			string[] terms = val.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			if (!terms.Any())
			{
				throw new XmlException("Failed to parse XML element during deserialization: 'Terms'");
			}

			Terms = terms.Select(s => BigInteger.Parse(s)).ToArray();
		}

		public XmlSchema GetSchema() { return null; }
	}
}