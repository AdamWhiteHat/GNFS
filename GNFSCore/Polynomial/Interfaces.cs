using System;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;

namespace GNFSCore.Polynomials
{
	public interface ICloneable<T>
	{
		T Clone();
	}

	public interface ITerm : ICloneable<ITerm>
	{
		int Exponent { get; }
		BigInteger CoEfficient { get; set; }
	}

	public interface IPolynomial
		: ICloneable<IPolynomial>,
			IComparable, IComparable<IPolynomial>,
			IEquatable<IPolynomial>, IEquatable<Polynomial>,
			IEqualityComparer<IPolynomial>, IEqualityComparer<Polynomial>,
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
		double Evaluate(double indeterminateValue);
	}
}
