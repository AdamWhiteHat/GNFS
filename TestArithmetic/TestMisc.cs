using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GNFSCore;

namespace TestArithmetic
{
	[TestClass]
	public class TestMisc
	{
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
	}
}
