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
using ExtendedNumerics;

namespace GNFSCore.Polynomial
{
	using Internal;

	[Serializable]
	public class AlgebraicPolynomial : IPolynomial, IXmlSerializable
	{
		public int Degree { get; private set; }
		public BigInteger N { get; private set; }
		public BigInteger Base { get; private set; }

		[XmlArrayItem("Terms")]
		public BigInteger[] Terms { get; private set; }

		public AlgebraicPolynomial()
		{ }

		public AlgebraicPolynomial(BigInteger n, BigInteger polynomialBase, int degree)
		{
			N = n;
			Base = polynomialBase;
			Degree = degree;

			Initialize();
		}

		private void Initialize()
		{
			Terms = Enumerable.Repeat(BigInteger.Zero, Degree + 1).ToArray();
			SetPolynomialValue(N);
		}

		private void SetPolynomialValue(BigInteger value)
		{
			N = value;
			int d = Degree;
			BigInteger toAdd = N;

			// Build out Terms[]
			while (d >= 0)
			{
				BigInteger placeValue = BigInteger.Pow(Base, d);

				if (placeValue == 1)
				{
					Terms[d] = toAdd;
				}
				else if (placeValue < BigInteger.Abs(toAdd))
				{
					BigInteger remainder = 0;
					BigInteger quotient = BigInteger.DivRem(toAdd, placeValue, out remainder);

					//Fraction fractionRemainder = new Fraction(remainder, placeValue);

					//bool roundUp = (Fraction.Abs(fractionRemainder) > Fraction.OneHalf);

					//if (roundUp)
					//{
					//	int adjustment = fractionRemainder.Sign;
					//	quotient += adjustment;
					//}

					if (quotient > placeValue)
					{
						quotient = placeValue;
					}

					Terms[d] = quotient;
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);
					toAdd -= toSubtract;
				}

				d--;
			}
		}

		public double Evaluate(double baseM)
		{
			return CommonPolynomial.Evaluate(this, baseM);
		}

		public BigInteger Evaluate(BigInteger baseM)
		{
			return CommonPolynomial.Evaluate(this, baseM);
		}

		public BigRational Evaluate(BigRational baseM)
		{
			return CommonPolynomial.Evaluate(this, baseM);
		}

		public BigInteger Derivative(BigInteger baseM)
		{
			return CommonPolynomial.Derivative(this, baseM);
		}

		public BigInteger g(BigInteger x, int p)
		{
			return BigInteger.Subtract(BigInteger.Pow(x, p), x);
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

		public override string ToString()
		{
			return CommonPolynomial.FormatString(this);
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("N", N.ToString());
			writer.WriteElementString("Base", Base.ToString());
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

			N = BigInteger.Parse(reader.ReadElementString("N"));
			Base = BigInteger.Parse(reader.ReadElementString("Base"));
			Degree = int.Parse(reader.ReadElementString("Degree"));

			reader.ReadStartElement("Terms");
			string val = reader.Value;
			if (!string.IsNullOrWhiteSpace(val))
			{
				val = val.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\f", "");
			}
			reader.Read();
			reader.ReadEndElement();
			reader.Skip(); // Polynomial
			reader.ReadEndElement();

			string[] terms = val.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			if (terms.Any())
			{
				Terms = terms.Select(s => BigInteger.Parse(s)).ToArray();
			}
			else
			{
				Initialize();
			}
		}

		public XmlSchema GetSchema() { return null; }
	}
}