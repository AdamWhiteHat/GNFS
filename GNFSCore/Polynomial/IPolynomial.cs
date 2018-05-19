using System;
using System.Numerics;

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
}
