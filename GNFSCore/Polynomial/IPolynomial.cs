using System;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;

namespace GNFSCore.Polynomial
{
	public interface IPolynomial : ICloneable<IPolynomial>
	{
		int Degree { get; }
		//BigInteger Base { get; }
		BigInteger[] Terms { get; }

		BigInteger Evaluate(BigInteger baseM);
		BigInteger Derivative(BigInteger baseM);
	}

	public interface ICloneable<T>
	{
		T Clone();
	}

	public interface ITerm : ICloneable<ITerm>
	{
		int Exponent { get; }
		BigInteger CoEfficient { get; set; }
	}

	public interface IPoly
		: ICloneable<IPoly>,
			IComparable, IComparable<IPoly>,
			IEquatable<IPoly>, IEquatable<SparsePolynomial>,
			IEqualityComparer<IPoly>, IEqualityComparer<SparsePolynomial>,
			IFormattable
	{
		int Degree { get; }
		ITerm[] Terms { get; }

		BigInteger this[int degree]
		{
			get;
			set;
		}

		void RemoveZeros();
		BigInteger Evaluate(BigInteger indeterminateValue);
	}

	[Serializable]
	public class PolyTerm : ITerm
	{
		public static PolyTerm Zero = new PolyTerm(BigInteger.Zero, 0);

		public int Exponent { get; private set; }
		public BigInteger CoEfficient { get; set; }
		private static string IndeterminateSymbol = "X";

		public PolyTerm(BigInteger coefficient, int exponent)
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
				results.Add(new PolyTerm(term, degree));

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
			return new PolyTerm(this.CoEfficient, this.Exponent);
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


	public static class ITermExtensionMethods
	{
		public static BigInteger[] GetCoefficients(this ITerm[] source)
		{
			return source.Select(trm => trm.CoEfficient).ToArray();
		}
	}
}
