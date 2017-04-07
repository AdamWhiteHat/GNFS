using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore
{
	using IntegerMath;

	public class QuadraticResidue
	{
		// a^(p-1)/2 ≡ 1 (mod p)
		public static bool IsQuadraticResidue(BigInteger a, BigInteger p)
		{
			BigInteger quotient = BigInteger.Divide(p - 1, 2);
			BigInteger modPow = BigInteger.ModPow(a, quotient, p);

			return modPow.IsOne;
		}

		public static int GetQuadraticCharacter(Relation rel, Tuple<int,int> quadraticFactor)
		{
			BigInteger ab = rel.A + rel.B;
			BigInteger abp = BigInteger.Abs(BigInteger.Multiply(ab, quadraticFactor.Item1));

			int legendreSymbol = Legendre.Symbol(abp, quadraticFactor.Item2);

			if(legendreSymbol != 1)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		public static string GetQuadraticCharacters(Relation rel, IEnumerable<Tuple<int, int>> quadraticCharacterBase)
		{
			IEnumerable<int> results = quadraticCharacterBase.Select(tup => GetQuadraticCharacter(rel, tup));
			
			return string.Join("", results.Select(i => i.ToString()));
		}
	}
}
