using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GNFSCore;
using GNFSCore.Polynomial;

namespace TestArithmetic
{
	[TestClass]
	public class TestMisc
	{
		[ClassInitialize]
		public static void Initialize(TestContext context) { }
		public void WriteOutput(string message = " ") { WriteOutput("{0}", message); }
		public void WriteOutput(string message, params object[] args) { TestContext.WriteLine(message, args); }
		private TestContext testContextInstance;
		public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }


		[TestMethod]
		public void TestBigIntegerClone()
		{
			string rsa100 = "1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139";
			BigInteger positiveLarge = BigInteger.Parse(rsa100);
			BigInteger one = new BigInteger(1);
			BigInteger zero = new BigInteger(0);
			BigInteger negativeOne = new BigInteger(-1);
			BigInteger negativeLarge = BigInteger.Negate(positiveLarge);

			BigInteger clonedPositiveLarge = positiveLarge.Clone();
			BigInteger clonedOne = one.Clone();
			BigInteger clonedZero = zero.Clone();
			BigInteger clonedNegativeOne = negativeOne.Clone();
			BigInteger clonedNegativeLarge = negativeLarge.Clone();

			Assert.AreEqual(positiveLarge, clonedPositiveLarge, "positiveLarge");
			Assert.AreEqual(one, clonedOne, "one");
			Assert.AreEqual(zero, clonedZero, "zero");
			Assert.AreEqual(negativeOne, clonedNegativeOne, "negativeOne");
			Assert.AreEqual(negativeLarge, clonedNegativeLarge, "negativeLarge");
		}

		[TestMethod]
		public void TestPolynomialToString()
		{
			BigInteger[] termsA = new BigInteger[] { 0, 0, 0, 0, -1 };
			BigInteger[] termsB = new BigInteger[] { -1, 0, 0, 0, 0, -1 };
			BigInteger[] termsC = new BigInteger[] { -1, 0, 0, 0, 0 };
			BigInteger[] termsD = new BigInteger[] { 0, 0, 0, 0, 0 };
			BigInteger[] termsE = new BigInteger[] { 0, 1, 0, -0, 0 };

			IPolynomial polyA = new AlgebraicPolynomial(termsA);
			IPolynomial polyB = new AlgebraicPolynomial(termsB);
			IPolynomial polyC = new AlgebraicPolynomial(termsC);
			IPolynomial polyD = new AlgebraicPolynomial(termsD);
			IPolynomial polyE = new AlgebraicPolynomial(termsE);

			WriteOutput($"PolyA: {polyA}");
			WriteOutput($"PolyB: {polyB}");
			WriteOutput($"PolyC: {polyC}");
			WriteOutput($"PolyC.Terms: {polyC.Terms.FormatString(false)}");
			WriteOutput($"PolyD: {polyD}");
			WriteOutput($"PolyE: {polyE}");

		}
	}
}
