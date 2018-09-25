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

	public interface ITerm : ICloneable<ITerm>
	{
		int Exponent { get; }
		BigInteger CoEfficient { get; set; }
	}

	public interface IPoly : ICloneable<IPoly>
	{
		int Degree { get; }
		ITerm[] Terms { get; }

		BigInteger this[int degree] { get; set; }

		void RemoveZeros();
		BigInteger Evaluate(BigInteger indeterminateValue);
	}

	public static class ITermExtensionMethods
	{
		public static BigInteger[] GetCoefficients(this ITerm[] source)
		{
			return source.Select(trm => trm.CoEfficient).ToArray();
		}
	}


	[Serializable]
	public class PolynomialTerm : ITerm
	{
		public int Exponent { get; private set; }
		public BigInteger CoEfficient { get; set; }

		private static string IndeterminateSymbol = "X";

		public PolynomialTerm(BigInteger coefficient, int exponent)
		{
			Exponent = exponent;
			CoEfficient = coefficient;
		}

		public static ITerm[] GetTerms(BigInteger[] terms)
		{
			List<ITerm> results = new List<ITerm>();

			int degree = 0;
			foreach (BigInteger term in terms)
			{
				results.Add(new PolynomialTerm(term, degree));

				degree += 1;
			}

			return results.ToArray();
		}

		public BigInteger Evaluate(BigInteger indeterminate)
		{
			return BigInteger.Multiply(CoEfficient, BigInteger.Pow(indeterminate, Exponent));
		}

		public ITerm Clone()
		{
			return new PolynomialTerm(this.CoEfficient, this.Exponent);
		}

		public override string ToString()
		{
			return $"{CoEfficient}*{IndeterminateSymbol}^{Exponent}";
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("CoEfficient", CoEfficient.ToString());
			writer.WriteElementString("Exponent", Exponent.ToString());
		}
		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();
			CoEfficient = BigInteger.Parse(reader.ReadElementString("CoEfficient"));
			Exponent = int.Parse(reader.ReadElementString("Exponent"));
		}
		public XmlSchema GetSchema() { return null; }
	}


	[Serializable]
	public class SparsePolynomial : IXmlSerializable, IPoly
	{
		[XmlArrayItem("Terms")]
		public ITerm[] Terms { get { return _terms.ToArray(); } }
		private List<ITerm> _terms;
		public int Degree { get; private set; }

		public BigInteger this[int degree]
		{
			get
			{
				ITerm term = Terms.FirstOrDefault(t => t.Exponent == degree);

				if (term == default(ITerm))
				{
					return BigInteger.Zero;
				}
				else
				{
					return term.CoEfficient;
				}
			}
			set
			{
				ITerm term = Terms.FirstOrDefault(t => t.Exponent == degree);

				if (term == default(ITerm))
				{
					if (value != BigInteger.Zero)
					{
						ITerm newTerm = new PolynomialTerm(value, degree);
						List<ITerm> terms = _terms;
						terms.Add(newTerm);
						SetTerms(terms);
					}
				}
				else
				{
					term.CoEfficient = value;
				}
			}
		}

		public SparsePolynomial() { }

		public SparsePolynomial(ITerm[] terms)
		{
			SetTerms(terms);
			SetDegree();
		}

		public SparsePolynomial(BigInteger n, BigInteger polynomialBase, int degree)
		{
			Degree = degree;
			SetTerms(GetPolynomialTerms(n, polynomialBase, Degree));
		}

		private void SetTerms(IEnumerable<ITerm> terms)
		{
			_terms = terms.OrderBy(t => t.Exponent).ToList();
		}

		private void SetDegree()
		{
			if (_terms.Any())
			{
				Degree = _terms.Max(term => term.Exponent);
			}
			else
			{
				Degree = 0;
			}
		}

		private static List<ITerm> GetPolynomialTerms(BigInteger value, BigInteger polynomialBase, int degree)
		{
			int d = degree;
			BigInteger toAdd = value;
			List<ITerm> result = new List<ITerm>();
			while (d >= 0 && toAdd > 0)
			{
				BigInteger placeValue = BigInteger.Pow(polynomialBase, d);

				if (placeValue == 1)
				{
					result.Add(new PolynomialTerm(toAdd, d));
					toAdd = 0;
				}
				else if (placeValue == toAdd)
				{
					result.Add(new PolynomialTerm(1, d));
					toAdd -= placeValue;
				}
				else if (placeValue < BigInteger.Abs(toAdd))
				{
					BigInteger remainder = 0;
					BigInteger quotient = BigInteger.DivRem(toAdd, placeValue, out remainder);

					if (quotient > placeValue)
					{
						quotient = placeValue;
					}

					result.Add(new PolynomialTerm(quotient, d));
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);

					toAdd -= toSubtract;
				}

				d--;
			}
			return result.ToList();
		}

		public static bool IsOne(IPoly poly)
		{
			return (poly.Degree == 1 & poly[0] == 1);
		}

		public BigInteger Evaluate(BigInteger indeterminateValue)
		{
			return Evaluate(Terms, indeterminateValue);
		}

		public static BigInteger Evaluate(ITerm[] terms, BigInteger indeterminateValue)
		{
			BigInteger result = new BigInteger(0);
			foreach (ITerm term in terms)
			{
				BigInteger placeValue = BigInteger.Pow(indeterminateValue, term.Exponent);
				BigInteger addValue = BigInteger.Multiply(term.CoEfficient, placeValue);
				result = BigInteger.Add(result, addValue);
			}
			return result;
		}

		public static IPoly GetDerivativePolynomial(IPoly poly)
		{
			int d = 0;
			List<ITerm> terms = new List<ITerm>();
			foreach (ITerm term in poly.Terms)
			{
				d = term.Exponent - 1;
				if (d < 0)
				{
					continue;
				}
				terms.Add(new PolynomialTerm(term.CoEfficient * term.Exponent, d));
			}

			return new SparsePolynomial(terms.ToArray());
		}

		public static IPoly GCD(IPoly left, IPoly right)
		{
			IPoly a = left.Clone();
			IPoly b = right.Clone();

			if (b.Degree > a.Degree)
			{
				IPoly swap = b;
				b = a;
				a = swap;
			}

			while (!(b.Terms.Length == 0 || b.Terms[0].CoEfficient == 0))
			{
				IPoly temp = a;
				a = b;
				b = Mod(temp, b);
			}

			if (a.Degree == 0)
			{
				return new SparsePolynomial(PolynomialTerm.GetTerms(new BigInteger[] { 1 }));
			}
			else
			{
				return a;
			}
		}

		public static IPoly Mod(IPoly left, IPoly right)
		{
			IPoly remainder = new SparsePolynomial();
			IPoly quotient = Divide(left, right, out remainder);
			return remainder;
		}

		public static IPoly Modulus(IPoly poly, BigInteger mod)
		{
			IPoly clone = poly.Clone();
			List<ITerm> terms = new List<ITerm>();

			foreach (ITerm term in clone.Terms)
			{
				BigInteger remainder = 0;
				BigInteger.DivRem(term.CoEfficient, mod, out remainder);
				terms.Add(new PolynomialTerm(remainder, term.Exponent));
			}

			// Recalculate the degree
			ITerm[] termArray = terms.SkipWhile(t => t.CoEfficient.Sign == 0).ToArray();
			IPoly result = new SparsePolynomial(termArray);
			return result;
		}

		public void RemoveZeros()
		{
			_terms.RemoveAll(t => t.CoEfficient == 0);
			SetDegree();
		}

		public static IPoly Divide(IPoly left, IPoly right)
		{
			IPoly remainder = new SparsePolynomial();
			return SparsePolynomial.Divide(left, right, out remainder);
		}

		public static IPoly Divide(IPoly left, IPoly right, out IPoly remainder)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));
			if (right.Degree > left.Degree) { throw new InvalidOperationException(); }

			int rightDegree = right.Degree;
			int quotientDegree = left.Degree - rightDegree + 1;
			BigInteger leadingCoefficent = new BigInteger(right[rightDegree].ToByteArray());

			IPoly rem = left.Clone();
			IPoly quotient = new SparsePolynomial(new ITerm[] { });

			// The leading coefficient is the only number we ever divide by
			// (so if right is monic, polynomial division does not involve division at all!)
			for (int i = quotientDegree - 1; i >= 0; i--)
			{
				quotient[i] = BigInteger.Divide(rem[rightDegree + i], leadingCoefficent);
				rem[rightDegree + i] = BigInteger.Zero;

				for (int j = rightDegree + i - 1; j >= i; j--)
				{
					rem[j] = BigInteger.Subtract(rem[j], BigInteger.Multiply(quotient[i], right[j - i]));
				}
			}

			// Remove zeros
			rem.RemoveZeros();
			quotient.RemoveZeros();

			remainder = rem;
			return quotient;
		}

		public static IPoly MultiplyMod(IPoly poly, BigInteger multiplier, BigInteger mod)
		{
			IPoly result = poly.Clone();

			foreach (ITerm term in result.Terms)
			{
				BigInteger value = term.CoEfficient;
				if (value != 0)
				{
					value = (value * multiplier);
					term.CoEfficient = (value % mod);
				}
			}

			return result;
		}


		public static IPoly Multiply(IPoly left, IPoly right)
		{
			if (left == null) { throw new ArgumentNullException(nameof(left)); }
			if (right == null) { throw new ArgumentNullException(nameof(right)); }

			BigInteger[] terms = new BigInteger[left.Degree + right.Degree + 1];

			for (int i = 0; i <= left.Degree; i++)
				for (int j = 0; j <= right.Degree; j++)
					terms[(i + j)] += BigInteger.Multiply(left[i], right[j]);

			return new SparsePolynomial(PolynomialTerm.GetTerms(terms));
		}

		public static IPoly Multiply(params IPoly[] polys)
		{
			IPoly result = null;

			foreach (IPoly p in polys)
			{
				if (result == null)
				{
					result = p;
				}
				else
				{
					result = SparsePolynomial.Multiply(result, p);
				}
			}

			return result;
		}

		public static IPoly Square(IPoly poly)
		{
			return SparsePolynomial.Multiply(poly, poly);
		}

		public static IPoly Pow(IPoly poly, int exponent)
		{
			if (exponent < 0)
			{
				throw new NotImplementedException("Raising a polynomial to a negative exponent not supported. Build this functionality if it is needed.");
			}
			else if (exponent == 0)
			{
				return new SparsePolynomial(new PolynomialTerm[] { new PolynomialTerm(1, 0) });
			}
			else if (exponent == 1)
			{
				return poly.Clone();
			}
			else if (exponent == 2)
			{
				return Square(poly);
			}

			IPoly total = SparsePolynomial.Square(poly);

			int counter = exponent - 2;
			while (counter != 0)
			{
				total = SparsePolynomial.Multiply(total, poly);
				counter -= 1;
			}

			return total;
		}

		public static IPoly Subtract(IPoly left, IPoly right)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			BigInteger[] terms = new BigInteger[Math.Min(left.Degree, right.Degree) + 1];
			for (int i = 0; i < terms.Length; i++)
			{
				BigInteger l = left[i];
				BigInteger r = right[i];

				terms[i] = (l - r);
			}

			IPoly result = new SparsePolynomial(PolynomialTerm.GetTerms(terms.ToArray()));

			return result;
		}

		public static IPoly Add(IPoly left, IPoly right)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			BigInteger[] terms = new BigInteger[Math.Min(left.Degree, right.Degree) + 1];
			for (int i = 0; i < terms.Length; i++)
			{
				terms[i] = (left[i] + right[i]);
			}

			IPoly result = new SparsePolynomial(PolynomialTerm.GetTerms(terms.ToArray()));
			return result;
		}

		public IPoly Clone()
		{
			return new SparsePolynomial(Terms.Select(pt => pt.Clone()).ToArray());
		}

		public override string ToString()
		{
			return SparsePolynomial.FormatString(this);
		}

		private static Dictionary<char, string> superscriptDictionary = new Dictionary<char, string>()
		{ {'0',"⁰"}, {'1',"¹"}, {'2',"²"}, {'3',"³"}, {'4',"⁴"}, {'5',"⁵"}, {'6',"⁶"}, {'7',"⁷"}, {'8',"⁸"}, {'9',"⁹"} };

		private static string GetSuperscript(BigInteger value)
		{
			string result = "";

			string valueStr = value.ToString();
			foreach (char c in valueStr)
			{
				result += superscriptDictionary[c];
			}

			return result;
		}

		public static string FormatString(IPoly polynomial)
		{
			List<string> stringTerms = new List<string>();
			int degree = polynomial.Terms.Length;
			while (--degree >= 0)
			{
				string termString = "";
				ITerm term = polynomial.Terms[degree];

				if (term.CoEfficient == 0)
				{
					if (term.Exponent == 0)
					{
						if (stringTerms.Count == 0) { stringTerms.Add("0"); }
					}
					continue;
				}
				else if (term.CoEfficient > 1 || term.CoEfficient < -1)
				{
					termString = $"{term.CoEfficient}";
				}

				switch (term.Exponent)
				{
					case 0:
						stringTerms.Add($"{term.CoEfficient}");
						break;

					case 1:
						if (term.CoEfficient == 1) stringTerms.Add("X");
						else if (term.CoEfficient == -1) stringTerms.Add("-X");
						else stringTerms.Add($"{term.CoEfficient}*X");
						break;

					default:
						if (term.CoEfficient == 1) stringTerms.Add($"X{GetSuperscript(term.Exponent)}");
						else if (term.CoEfficient == -1) stringTerms.Add($"-X{GetSuperscript(term.Exponent)}");
						else stringTerms.Add($"{term.CoEfficient}*X{GetSuperscript(term.Exponent)}");
						break;
				}
			}
			return string.Join(" + ", stringTerms).Replace("+ -", "- ");
		}

		private static string spacer = $"{Environment.NewLine}    ";
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("Degree", Degree.ToString());
			writer.WriteStartElement("Terms");
			writer.WriteString($"{spacer}{string.Join(spacer, Terms.Select(trm => $"{trm}"))}{spacer}");
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

			string[] termLines = val.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			if (!termLines.Any())
			{
				throw new XmlException("Failed to parse XML element during deserialization: 'Terms'");
			}

			// a*X^b
			IEnumerable<ITerm> terms = termLines.Select(str =>
			{
				string[] termParts = str.Split(new string[] { "*X^" }, StringSplitOptions.None);
				return new PolynomialTerm(BigInteger.Parse(termParts[0]), int.Parse(termParts[1]));
			});

			_terms = terms.ToList();
		}

		public XmlSchema GetSchema() { return null; }
	}
}