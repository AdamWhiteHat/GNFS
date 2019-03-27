using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace GNFSCore.Interfaces
{
	public interface IPolynomial
		: ICloneable<IPolynomial>,
			IComparable<IPolynomial>,
			IEquatable<IPolynomial>, IEquatable<Polynomial>,
			IEqualityComparer<IPolynomial>, IEqualityComparer<Polynomial>
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
