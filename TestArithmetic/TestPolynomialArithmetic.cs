using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GNFSCore;
using GNFSCore.Polynomial;
using GNFSCore.Polynomial.Internal;

namespace TestArithmetic
{
	[TestClass]
	public class TestPolynomialArithmetic
	{
		[ClassInitialize]
		public static void Initialize(TestContext context) { }
		public void WriteOutput(string message = " ") { WriteOutput("{0}", message); }
		public void WriteOutput(string message, params object[] args) { TestContext.WriteLine(message, args); }
		private TestContext testContextInstance;
		public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }

		[TestMethod]
		public void TestGCDPolynomials()
		{
			BigInteger[] expectedCoefficients = new BigInteger[] { 0, 1 };
			BigInteger[] termsA = new BigInteger[] { 0,-10,5,3,11,3,5,2 };
			BigInteger[] termsB = new BigInteger[] { 0,1,1,1 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);

			IPolynomial result = CommonPolynomial.GCD(polyA, polyB);
			BigInteger[] resultCoefficients = result.Terms.ToArray();

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"Expected: {new AlgebraicPolynomial(expectedCoefficients)}");
			WriteOutput($"Result  : {result}");

			CollectionAssert.AreEqual(expectedCoefficients, resultCoefficients);
		}

		[TestMethod]
		public void TestMultiplyPolynomials()
		{
			BigInteger[] expectedCoefficients = new BigInteger[] { 7, 9, 12, 5, 3 }; // 3 * X^4 + 5 * X^3 + 12 * X^2 + 9 * X + 7
			BigInteger[] termsA = new BigInteger[] { 7, 2, 3 };
			BigInteger[] termsB = new BigInteger[] { 1, 1, 1 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);

			IPolynomial result = CommonPolynomial.Multiply(polyA, polyB);
			BigInteger[] resultCoefficients = result.Terms.ToArray();

			CollectionAssert.AreEqual(expectedCoefficients, resultCoefficients);

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"Result: {result}");
		}

		[TestMethod]
		public void TestModPolynomials()
		{
			BigInteger[] expectedCoefficients = new BigInteger[] { 4, -1 }; // 4-X			
			BigInteger[] termsA = new BigInteger[] { 7, 2, 3 };
			BigInteger[] termsB = new BigInteger[] { 1, 1, 1 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);

			IPolynomial result = CommonPolynomial.Mod(polyA, polyB);
			BigInteger[] resultCoefficients = result.Terms.ToArray();

			CollectionAssert.AreEqual(expectedCoefficients, resultCoefficients);

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"Result: {result}");
		}

		[TestMethod]
		public void TestDividePolynomials()
		{
			BigInteger[] expectedRemainderCoefficients = new BigInteger[] { 4, -1 }; // 4 - X
			BigInteger[] expectedCoefficients = new BigInteger[] { 3 };
			BigInteger[] termsA = new BigInteger[] { 7, 2, 3 };
			BigInteger[] termsB = new BigInteger[] { 1, 1, 1 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);

			IPolynomial resultRemainder = null;
			IPolynomial result = CommonPolynomial.Divide(polyA, polyB, out resultRemainder);
			BigInteger[] resultCoefficients = result.Terms.ToArray();
			BigInteger[] resultRemainderCoefficients = resultRemainder.Terms.ToArray();

			CollectionAssert.AreEqual(expectedCoefficients, resultCoefficients);
			CollectionAssert.AreEqual(expectedRemainderCoefficients, resultRemainderCoefficients);

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"Result: {result}");
			WriteOutput($"Remainder: {resultRemainder}");
		}

		[TestMethod]
		public void TestAddPolynomials()
		{
			BigInteger[] expectedCoefficients = new BigInteger[] { 8, 3, 4 };
			BigInteger[] termsA = new BigInteger[] { 7, 2, 3 };
			BigInteger[] termsB = new BigInteger[] { 1, 1, 1 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);

			IPolynomial result = CommonPolynomial.Add(polyA, polyB);
			BigInteger[] resultCoefficients = result.Terms.ToArray();

			CollectionAssert.AreEqual(expectedCoefficients, resultCoefficients);

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"Result: {result}");
		}

		[TestMethod]
		public void TestSubtractPolynomials()
		{
			BigInteger[] expectedCoefficients = new BigInteger[] { 6, 1, 2 };
			BigInteger[] termsA = new BigInteger[] { 7, 2, 3 };
			BigInteger[] termsB = new BigInteger[] { 1, 1, 1 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);

			IPolynomial result = CommonPolynomial.Subtract(polyA, polyB);
			BigInteger[] resultCoefficients = result.Terms.ToArray();

			CollectionAssert.AreEqual(expectedCoefficients, resultCoefficients);

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"Result: {result}");
		}
	}
}
