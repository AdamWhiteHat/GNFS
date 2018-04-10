using System.Numerics;

namespace GNFSCore.Polynomial
{
	public interface IPolynomial
	{
		int Degree { get; }
		//BigInteger Base { get; }
		BigInteger[] Terms { get; }

		BigInteger Evaluate(BigInteger baseM);
		BigInteger Derivative(BigInteger baseM);
	}
}
