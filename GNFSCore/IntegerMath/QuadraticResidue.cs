using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFSCore.IntegerMath
{
	using Factors;

	public class QuadraticResidue
	{
		// a^(p-1)/2 ≡ 1 (mod p)
		public static bool IsQuadraticResidue(BigInteger a, BigInteger p)
		{
			BigInteger quotient = BigInteger.Divide(p - 1, 2);
			BigInteger modPow = BigInteger.ModPow(a, quotient, p);

			return modPow.IsOne;
		}

		public static bool GetQuadraticCharacter(Relation rel, FactorPair quadraticFactor)
		{
			BigInteger ab = rel.A + rel.B;
			BigInteger abp = BigInteger.Abs(BigInteger.Multiply(ab, quadraticFactor.P));

			int legendreSymbol = Legendre.Symbol(abp, quadraticFactor.R);
			return (legendreSymbol != 1);
		}

		public static bool[] GetQuadraticCharacters(Relation rel, IEnumerable<FactorPair> quadraticCharacterBase)
		{
			return quadraticCharacterBase.Select(pair => GetQuadraticCharacter(rel, pair)).ToArray();
		}
	}
}
