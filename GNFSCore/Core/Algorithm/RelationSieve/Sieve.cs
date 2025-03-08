using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GNFSCore.Core.Data.RelationSieve
{
	using Algorithm.IntegerMath;
	using Interfaces;
	using Factors;
	using Matrix;
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Data;

	public static class Sieve
	{
		public static void Relation(PolyRelationsSieveProgress relationsSieve, Relation relation)
		{
			Relations(relationsSieve._gnfs.PrimeFactorBase.RationalFactorBase, ref relation.RationalQuotient, relation.RationalFactorization);

			if (relation.IsRationalQuotientSmooth) // No sense wasting time on factoring the AlgebraicQuotient if the relation is ultimately going to be rejected anyways.
			{
				Relations(relationsSieve._gnfs.PrimeFactorBase.AlgebraicFactorBase, ref relation.AlgebraicQuotient, relation.AlgebraicFactorization);
			}
		}

		private static void Relations(IEnumerable<BigInteger> primeFactors, ref BigInteger quotientValue, CountDictionary dictionary)
		{
			if (quotientValue.Sign == -1 || primeFactors.Any(f => f.Sign == -1))
			{
				throw new Exception("There shouldn't be any negative values either in the quotient or the factors");
			}

			foreach (BigInteger factor in primeFactors)
			{
				if (quotientValue == 1)
				{
					return;
				}

				if (factor * factor > quotientValue)
				{
					if (primeFactors.Contains(quotientValue))
					{
						dictionary.Add(quotientValue);
						quotientValue = 1;
					}
					return;
				}

				while (quotientValue != 1 && quotientValue % factor == 0)
				{
					dictionary.Add(factor);
					quotientValue = BigInteger.Divide(quotientValue, factor);
				}
			}

			/*
            if (quotientValue != 0 && quotientValue != 1)
            {
                if (FactorizationFactory.IsProbablePrime(quotientValue))
                {
                    if (quotientValue < (primeFactors.Last() * 2))
                    {
                        dictionary.Add(quotientValue);
                        quotientValue = 1;
                    }
                }
            }
            */
		}
	}
}
