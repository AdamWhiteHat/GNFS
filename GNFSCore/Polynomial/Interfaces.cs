using System;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;

namespace GNFSCore.Polynomial
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
		double Evaluate(double indeterminateValue);
	}
}
