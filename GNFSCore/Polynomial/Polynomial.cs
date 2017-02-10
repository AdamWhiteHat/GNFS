using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GNFSCore.Polynomial
{
	public class PolynomialContainer
	{
		public Cyclotomic RationalPolynomial { get; private set; }
		public Irreducible AlgebraicPolynomial { get; private set; }

		public PolynomialContainer(BigInteger N, BigInteger polynomialBase, int degree)
		{
			RationalPolynomial = new Cyclotomic(N);
			AlgebraicPolynomial = new Irreducible(N, polynomialBase, degree);
		}
	}
}
