using System;
using System.Linq;
using System.Numerics;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GNFSCore
{
	public class Polynomial
	{
		public BigInteger N { get; private set; }
		public BigInteger Base { get; private set; }
		public int Degree { get; private set; } = 0;		

		//public BigInteger Terms { get; private set;} = new List<BigInteger>();
		//public BigInteger Total { get; private set; } = 0;
		
		public Polynomial(BigInteger n, BigInteger baseM, int degree)
		{
			//n = N;
			//Base = baseM;
			//Degree = degree;
			
		}


		public BigInteger Value(BigInteger i)
		{
			return -1;
		}
	}
}