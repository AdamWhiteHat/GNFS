using ExtendedArithmetic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using GNFSCore.Core.Data.RelationSieve;
using GNFSCore.Core.Data.Matrix;
using GNFSCore.Core.Algorithm.ExtensionMethods;
using GNFSCore.Core.Algorithm.SquareRoot;
using GNFSCore.Core.Data;

namespace TestGNFS.Integration
{

	[TestFixture]
	[SingleThreaded]
	[NonParallelizable]

	public class SmallFactorizationTest_45113
	{
		[Test]
		public void Debug()
		{

			string saveLocaiton = TestHelper.GetTestSaveLocation();

			TestContext.WriteLine($"Save Location: \"{saveLocaiton}\".");

			Assert.IsNotNull(saveLocaiton);

			Thread.Yield();

			TestContext.WriteLine($"Test duration: {TestExecutionContext.CurrentContext.CurrentResult.Duration} seconds.");
			TestContext.WriteLine($"Test Assert Count: {TestContext.CurrentContext.AssertCount}");


		}

		[Test]
		[Order(0)]
		public void Test_45113()
		{
			BigInteger N = 45113;
			BigInteger polyBase = 31;
			int polydegree = 3;

			GNFS gnfs = new GNFS(
				  CancellationToken.None,
				  TestContext.WriteLine,
				  N,
				  polyBase,
				  polydegree,
				  50,
				  60,
				  1000,
				  true)
			{
			};

			List<FactorPair> rationalFactorPairs = new List<FactorPair>()
			{
				new FactorPair(2,1),
				new FactorPair(3,1),
				new FactorPair(5,1),
				new FactorPair(7,3),
				new FactorPair(11,9),
				new FactorPair(13,5),
				new FactorPair(17,14),
				new FactorPair(19,12),
				new FactorPair(23,8),
				new FactorPair(29,2)
			};

			List<FactorPair> algebraicFactorPairs = new List<FactorPair>()
			{
				new FactorPair(2, 0),
				new FactorPair(7, 6),
				new FactorPair(17, 13),
				new FactorPair(23, 11),
				new FactorPair(29, 26),
				new FactorPair(31, 18),
				new FactorPair(41, 19),
				new FactorPair(43, 13),
				new FactorPair(53, 1),
				new FactorPair(61, 46),
				new FactorPair(67, 2),
				new FactorPair(67, 6),
				new FactorPair(67, 44),
				new FactorPair(73, 50),
				new FactorPair(79, 23),
				new FactorPair(79, 47),
				new FactorPair(79, 73),
				new FactorPair(89, 28),
				new FactorPair(89, 62),
				new FactorPair(89, 73),
				new FactorPair(97, 28),
				new FactorPair(101, 87),
				new FactorPair(103, 47)
			};

			List<FactorPair> quadraticFactorPairs = new List<FactorPair>()
			{
				new FactorPair(107, 4),
				new FactorPair(107, 80),
				new FactorPair(109, 99),
				new FactorPair(107, 8),
				new FactorPair(109, 52)
			};

			gnfs.RationalFactorPairCollection.Clear();
			gnfs.RationalFactorPairCollection.AddRange(rationalFactorPairs);

			gnfs.AlgebraicFactorPairCollection.Clear();
			gnfs.AlgebraicFactorPairCollection.AddRange(algebraicFactorPairs);

			gnfs.QuadraticFactorPairCollection.Clear();
			gnfs.QuadraticFactorPairCollection.AddRange(quadraticFactorPairs);

			TestContext.WriteLine();

			TestContext.WriteLine("Rational Factor Pairs:");
			TestContext.WriteLine(string.Join(Environment.NewLine, gnfs.RationalFactorPairCollection.Select(fp => fp.ToString())));
			TestContext.WriteLine();

			TestContext.WriteLine("Algebraic Factor Pairs:");
			TestContext.WriteLine(string.Join(Environment.NewLine, gnfs.AlgebraicFactorPairCollection.Select(fp => fp.ToString())));
			TestContext.WriteLine();

			TestContext.WriteLine("Quadratic Factor Pairs:");
			TestContext.WriteLine(string.Join(Environment.NewLine, gnfs.QuadraticFactorPairCollection.Select(fp => fp.ToString())));

			TestContext.WriteLine();


			List<Relation> relations = new List<Relation>()
			{
				new Relation(gnfs, 1, -73),
				new Relation(gnfs, 1, -2),
				new Relation(gnfs, 1, -1),
				new Relation(gnfs, 1, 2),
				new Relation(gnfs, 1, 3),
				new Relation(gnfs, 1, 4),
				new Relation(gnfs, 1, 8),
				new Relation(gnfs, 1, 13),
				new Relation(gnfs, 1, 14),
				new Relation(gnfs, 1, 15),
				new Relation(gnfs, 1, 32),
				new Relation(gnfs, 1, 56),
				new Relation(gnfs, 1, 61),
				new Relation(gnfs, 1, 104),
				new Relation(gnfs, 1, 116),
				new Relation(gnfs, 2, -5),
				new Relation(gnfs, 2, 3),
				new Relation(gnfs, 2, 25),
				new Relation(gnfs, 2, 33),
				new Relation(gnfs, 3, -8),
				new Relation(gnfs, 3, 2),
				new Relation(gnfs, 3, 17),
				new Relation(gnfs, 4, 19),
				new Relation(gnfs, 5, 48),
				new Relation(gnfs, 5, 54),
				new Relation(gnfs, 5, 313),
				new Relation(gnfs, 6, -43),
				new Relation(gnfs, 7, -8),
				new Relation(gnfs, 7, 11),
				new Relation(gnfs, 7, 38),
				new Relation(gnfs, 9, 44),
				new Relation(gnfs, 11, 4),
				new Relation(gnfs, 11, 119),
				new Relation(gnfs, 11, 856),
				new Relation(gnfs, 15, 536),
				new Relation(gnfs, 17, 5),
				new Relation(gnfs, 31, 5),
				new Relation(gnfs, 32, 9),
				new Relation(gnfs, 43, -202),
				new Relation(gnfs, 55, 24)
			};

			List<Relation> dependency = new List<Relation>()
			{
				new Relation(gnfs, -1, 1),
				new Relation(gnfs,104, 1),
				new Relation(gnfs, -8, 3),
				new Relation(gnfs,-43, 6),
				new Relation(gnfs,856, 11),
				new Relation(gnfs,  3, 1),
				new Relation(gnfs,  3, 2),
				new Relation(gnfs, 48, 5),
				new Relation(gnfs, -8, 7),
				new Relation(gnfs, 13, 1),
				new Relation(gnfs, 25, 2),
				new Relation(gnfs, 54, 5),
				new Relation(gnfs, 11, 7)
			};

			gnfs.CurrentRelationsProgress = new PolyRelationsSieveProgress(gnfs, 38, 1000);

			foreach (Relation rel in relations)
			{
				Sieve.Relation(gnfs.CurrentRelationsProgress, rel);
			}
			gnfs.CurrentRelationsProgress.Relations.SmoothRelations.AddRange(relations);

			foreach (Relation rel in dependency)
			{
				Sieve.Relation(gnfs.CurrentRelationsProgress, rel);
			}
			gnfs.CurrentRelationsProgress.Relations.FreeRelations.Add(dependency);

			TestContext.WriteLine(string.Join(Environment.NewLine, dependency.Select(rel => $"{rel} R: {{{string.Join(", ", rel.RationalFactorization.Keys)}}}" + " \t " + $"A: {{{string.Join(", ", rel.AlgebraicFactorization.Keys)}}}")));

			CountDictionary rationalDependency = new CountDictionary();
			CountDictionary algebraicDependency = new CountDictionary();

			foreach (Relation rel in dependency)
			{
				rationalDependency.Combine(rel.RationalFactorization);
				algebraicDependency.Combine(rel.AlgebraicFactorization);
			}

			MatrixSolver.GaussianSolve(CancellationToken.None, gnfs);

			TestContext.WriteLine();

			TestContext.WriteLine("Rational Dependency;");
			TestContext.WriteLine(rationalDependency.ToString());

			TestContext.WriteLine();

			TestContext.WriteLine("Algebraic Dependency:");
			TestContext.WriteLine(algebraicDependency.ToString());


			Polynomial PolynomialDerivative = Polynomial.GetDerivativePolynomial(gnfs.CurrentPolynomial);
			BigInteger PolynomialDerivativeValue = PolynomialDerivative.Evaluate(gnfs.PolynomialBase);
			BigInteger PolynomialDerivativeValueSquared = BigInteger.Pow(PolynomialDerivativeValue, 2);

			BigInteger sqrtTotal_Algebraic = 1;
			foreach (var kvp in algebraicDependency)
			{
				int pow = (int)kvp.Value / 2;
				sqrtTotal_Algebraic *= BigInteger.Pow(kvp.Key, pow);
			}

			BigInteger sqrtTotal_Rational = 1;
			foreach (var kvp in rationalDependency)
			{
				int pow = (int)kvp.Value / 2;
				sqrtTotal_Rational *= BigInteger.Pow(kvp.Key, pow);
			}

			Polynomial MonicPolynomial = Polynomial.MakeMonic(gnfs.CurrentPolynomial, gnfs.PolynomialBase);
			Polynomial MonicPolynomialDerivative = Polynomial.GetDerivativePolynomial(MonicPolynomial);
			BigInteger MonicPolynomialDerivativeValue = MonicPolynomialDerivative.Evaluate(gnfs.PolynomialBase);


			TestContext.WriteLine();
			TestContext.WriteLine($"Algebraic Half-Primes Sqrt: {sqrtTotal_Algebraic} * f'(θ) = {(sqrtTotal_Algebraic * MonicPolynomialDerivativeValue)} ≡ {GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.Mod(sqrtTotal_Algebraic * MonicPolynomialDerivativeValue, N)} (mod N)");
			TestContext.WriteLine($"Rational  Half-Primes Sqrt: {sqrtTotal_Rational}");


			List<Polynomial> PolynomialRingElements = new List<Polynomial>();
			foreach (Relation rel in dependency)
			{
				// poly(x) = A + (B * x)
				Polynomial newPoly =
					new Polynomial(
						new Term[]
						{
							new Term( rel.B, 1),
							new Term( rel.A, 0)
						}
					);

				PolynomialRingElements.Add(newPoly);
			}

			Polynomial PolynomialRing = Polynomial.Product(PolynomialRingElements);
			Polynomial PolynomialRingInField = Polynomial.Field.Modulus(PolynomialRing, MonicPolynomial);

			TestContext.WriteLine();
			TestContext.WriteLine($"∏ Sᵢ = {PolynomialRing}");
			TestContext.WriteLine("(Polynomial Ring)");
			TestContext.WriteLine();
			TestContext.WriteLine($"∏ Sᵢ = {PolynomialRingInField}");
			TestContext.WriteLine("(Polynomial Ring in the Field ℤ)");

			TestContext.WriteLine();
			TestContext.WriteLine($"MonicPolynomial: {MonicPolynomial}");
			TestContext.WriteLine($"f'(θ) = {MonicPolynomialDerivativeValue}");
			TestContext.WriteLine();

			Polynomial MonicPolynomialDerivativeSquared = Polynomial.Square(MonicPolynomialDerivative);

			Polynomial TotalS = Polynomial.Multiply(PolynomialRing, MonicPolynomialDerivativeSquared);
			Polynomial S = Polynomial.Field.Modulus(TotalS, MonicPolynomial);

			TestContext.WriteLine();
			TestContext.WriteLine($"δᵨ = {TotalS}");
			TestContext.WriteLine();
			TestContext.WriteLine($"δᵨ = {S}");
			TestContext.WriteLine(" in ℤ");


			List<BigInteger> rationalNorms = dependency.Select(rel => rel.RationalNorm).ToList();
			BigInteger RationalProduct = rationalNorms.Product();
			BigInteger RationalProductSquareRoot = GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.SquareRoot(RationalProduct);

			var product = PolynomialDerivativeValue * RationalProductSquareRoot;
			var RationalSqrt = GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.Mod(product, N);

			TestContext.WriteLine();
			TestContext.WriteLine($"δᵣ = {RationalProductSquareRoot}^2 = {RationalProduct}");
			TestContext.WriteLine($" {RationalSqrt} ≡ {PolynomialDerivativeValue} * {RationalProductSquareRoot} (mod {N})");

			TestContext.WriteLine();
			TestContext.WriteLine("--------------------");
			TestContext.WriteLine();

			List<BigInteger> primes = new List<BigInteger>()
			{
				new BigInteger(9851),
				new BigInteger(9907),
				new BigInteger(9929)
			};


			BigInteger P = primes.Product();

			List<BigInteger> values = new List<BigInteger>();
			List<Polynomial> sqrtsOfSModP = new List<Polynomial>();

			bool takeInverse = false;
			foreach (BigInteger p in primes)
			{
				Polynomial sqrtOfS = FiniteFieldArithmetic.SquareRoot(S, gnfs.CurrentPolynomial, p, gnfs.PolynomialDegree, gnfs.PolynomialBase);

				BigInteger eval = sqrtOfS.Evaluate(gnfs.PolynomialBase);
				BigInteger x = GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.Mod(eval, p);

				Polynomial inverse = ModularInverse(sqrtOfS, p);
				BigInteger inverseEval = inverse.Evaluate(gnfs.PolynomialBase);
				BigInteger inverseX = GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.Mod(inverseEval, p);

				TestContext.WriteLine();
				TestContext.WriteLine($" β  =  {sqrtOfS}");
				TestContext.WriteLine($"{x}");
				TestContext.WriteLine($"(β) = ({inverse})");
				TestContext.WriteLine($"({inverseX})");
				TestContext.WriteLine();
				TestContext.WriteLine($"{p}");
				TestContext.WriteLine($"{P / p}");

				if (takeInverse)
				{
					sqrtsOfSModP.Add(inverse);
					values.Add(inverseX);
				}
				else
				{
					sqrtsOfSModP.Add(sqrtOfS);
					values.Add(x);
				}

				takeInverse = !takeInverse;
			}


			BigInteger commonModulus = FiniteFieldArithmetic.ChineseRemainder(primes, values);

			TestContext.WriteLine();
			TestContext.WriteLine($"ChineseRemainder.Result : {commonModulus}");

			BigInteger expected_ChineseRemainderResult = BigInteger.Parse("694683807559");
			Assert.AreEqual(expected_ChineseRemainderResult, commonModulus);


			BigInteger algebraicSquareRoot = GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.Mod(commonModulus, N);


			TestContext.WriteLine();
			TestContext.WriteLine($"χ = {RationalSqrt}");
			TestContext.WriteLine($"γ = {algebraicSquareRoot}"); // δ mod N 

			BigInteger expected_RationalSqrt = 15160;
			BigInteger expected_AlgebraicSqrt = 43922;

			Assert.AreEqual(expected_RationalSqrt, RationalSqrt);
			Assert.AreEqual(expected_AlgebraicSqrt, algebraicSquareRoot);
		}

		private static Polynomial ModularInverse(Polynomial poly, BigInteger mod)
		{
			return new Polynomial(Term.GetTerms(poly.Terms.Select(trm => GNFSCore.Core.Algorithm.ExtensionMethods.BigIntegerExtensionMethods.Mod(mod - trm.CoEfficient, mod)).ToArray()));
		}

	}
}