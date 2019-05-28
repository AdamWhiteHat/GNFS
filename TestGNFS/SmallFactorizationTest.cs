using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using NUnit.Framework;

using GNFSCore;
using GNFSCore.Matrix;
using GNFSCore.SquareRoot;
using GNFSCore.IntegerMath;

namespace TestGNFS.Integration
{
	[TestFixture]
	public class SmallFactorizationTest
	{
		public TestContext TestContext
		{
			get { return testContextInstance; }
			set { testContextInstance = value; }
		}
		TestContext testContextInstance;

		private static int degree = 3;
		private static BigInteger polyBase = 31;
		private static BigInteger primeBound = 29;
		private static int relationQuantity = 70;
		private static int relationValueRange = 1000;
		private static BigInteger N = new BigInteger(45113);

		private static GNFS gnfs;
		private static CancellationToken cancelToken;
		private static CancellationTokenSource cancellationTokenSource;

		private static bool step00_passed = false;
		private static bool step01_passed = false;
		private static bool step02_passed = false;
		private static bool step03_passed = false;
		private static bool step04_passed = false;

		private static readonly string TestSaveLocation = "C:\\GNFS\\Test";

		private void RecursiveDelete(string path)
		{
			IEnumerable<string> files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				File.Delete(file);
			}

			IEnumerable<string> directories = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
			foreach (string directory in directories)
			{
				RecursiveDelete(directory);
			}

			Directory.Delete(path);
		}

		[Order(0)]
		[Test]
		public void Test00_Initialize()
		{
			TestContext.WriteLine($"ENTER: {nameof(Test00_Initialize)}");

			step00_passed = false;
			step01_passed = false;
			step02_passed = false;
			step03_passed = false;
			step04_passed = false;

			if (Directory.Exists(TestSaveLocation))
			{
				RecursiveDelete(TestSaveLocation);
			}

			DirectoryLocations.SetBaseDirectory(TestSaveLocation);

			gnfs = null;
			cancellationTokenSource = new CancellationTokenSource();
			cancelToken = cancellationTokenSource.Token;

			step00_passed = true;

			TestContext.WriteLine($"{nameof(Test00_Initialize)} passed?: {step00_passed}");
			TestContext.WriteLine($"LEAVE: {nameof(Test00_Initialize)}");
		}

		[Order(1)]
		[Test]
		public void Test01_GNFSCreate()
		{
			while (!step00_passed)
			{
				Thread.SpinWait(100);
			}

			TestContext.WriteLine($"ENTER: {nameof(Test01_GNFSCreate)}");

			Assert.IsTrue(step00_passed, "IsTrue(step00_passed)");

			gnfs = new GNFS(cancelToken, Console.WriteLine, N, polyBase, degree, primeBound, relationQuantity, relationValueRange);

			Assert.IsNotNull(gnfs, "IsNotNull(gnfs)");

			Assert.IsTrue(gnfs.PrimeFactorBase.RationalFactorBase.Any(), "IsTrue(gnfs.PrimeFactorBase.RATIONALFactorBase.Any())");
			Assert.IsTrue(gnfs.PrimeFactorBase.AlgebraicFactorBase.Any(), "IsTrue(gnfs.PrimeFactorBase.ALGEBRAICFactorBase.Any())");
			Assert.IsTrue(gnfs.PrimeFactorBase.QuadraticFactorBase.Any(), "IsTrue(gnfs.PrimeFactorBase.QUADRATICFactorBase.Any())");

			Assert.IsTrue(gnfs.RationalFactorPairCollection.Any(), "IsTrue(gnfs.RATIONALFactorPairCollection.Any())");
			Assert.IsTrue(gnfs.AlgebraicFactorPairCollection.Any(), "IsTrue(gnfs.ALGEBRAICFactorPairCollection.Any())");
			Assert.IsTrue(gnfs.QuadraticFactorPairCollection.Any(), "IsTrue(gnfs.QUADRATICFactorPairCollection.Any())");

			step01_passed = true;

			TestContext.WriteLine($"{nameof(Test01_GNFSCreate)} passed?: {step01_passed}");
			TestContext.WriteLine($"LEAVE: {nameof(Test01_GNFSCreate)}");
		}

		[Order(2)]
		[Test]
		public void Test02_GenerateRelations()
		{
			TestContext.WriteLine($"ENTER: {nameof(Test02_GenerateRelations)}");
			Assert.IsTrue(step01_passed, "IsTrue(step01_passed)");
			Assert.IsNotNull(gnfs, "IsNotNull(gnfs)");

			bool success = false;

			while (!cancelToken.IsCancellationRequested)
			{
				if (gnfs.CurrentRelationsProgress.SmoothRelationsCounter >= gnfs.CurrentRelationsProgress.Quantity)
				{
					gnfs.CurrentRelationsProgress.IncreaseQuantity(100);
				}

				gnfs.CurrentRelationsProgress.GenerateRelations(cancelToken);

				Console.Write(".");

				if (gnfs.CurrentRelationsProgress.SmoothRelationsCounter >= gnfs.CurrentRelationsProgress.Quantity)
				{
					success = true;
					break;
				}
			}

			Assert.IsTrue(success, "IsTrue(success)");

			step02_passed = success;

			TestContext.WriteLine($"{nameof(Test02_GenerateRelations)} passed?: {step02_passed}");
			TestContext.WriteLine($"LEAVE: {nameof(Test02_GenerateRelations)}");
		}

		[Order(3)]
		[Test]
		public void Test03_Matrix()
		{
			TestContext.WriteLine($"ENTER: {nameof(Test03_Matrix)}");

			Assert.IsTrue(step02_passed, "IsTrue(step02_passed)");
			Assert.IsNotNull(gnfs, "IsNotNull(gnfs)");

			MatrixSolve.GaussianSolve(cancelToken, gnfs);

			Assert.IsTrue(gnfs.CurrentRelationsProgress.FreeRelations.Any(), "IsTrue(gnfs.CurrentRelationsProgress.FreeRelations.Any())");

			step03_passed = true;

			TestContext.WriteLine($"{nameof(Test03_Matrix)} passed?: {step03_passed}");
			TestContext.WriteLine($"LEAVE: {nameof(Test03_Matrix)}");
		}

		[Order(4)]
		[Test]
		public void Test04_SquareRoot()
		{
			TestContext.WriteLine($"ENTER: {nameof(Test04_SquareRoot)}");

			Assert.IsTrue(step03_passed, "IsTrue(step03_passed)");
			Assert.IsNotNull(gnfs, "IsNotNull(gnfs)");

			int maxSetSize = gnfs.CurrentRelationsProgress.FreeRelations.Max(lst => lst.Count);

			List<Relation> choosenRelationSet = gnfs.CurrentRelationsProgress.FreeRelations.Where(lst => lst.Count == maxSetSize).First();

			SquareFinder squareRootFinder = new SquareFinder(gnfs, choosenRelationSet);

			squareRootFinder.CalculateRationalSide();
			bool sqrtSuccess = squareRootFinder.CalculateAlgebraicSide();

			Assert.IsTrue(sqrtSuccess, "Square Root Success");

			BigInteger min = BigInteger.Min(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(squareRootFinder.RationalSquareRootResidue, squareRootFinder.AlgebraicSquareRootResidue);

			BigInteger R = max - min;
			BigInteger S = max + min;

			BigInteger P = GCD.FindGCD(gnfs.N, S);
			BigInteger Q = GCD.FindGCD(gnfs.N, R);

			Assert.AreNotEqual(1, P, "AreNotEqual(1, P)");
			Assert.AreNotEqual(1, Q, "AreNotEqual(1, Q)");

			Assert.AreEqual(new BigInteger(1811), P, "AreEqual(1811, P)");
			Assert.AreEqual(new BigInteger(1777), Q, "AreEqual(1777, Q)");

			step04_passed = true;

			TestContext.WriteLine($"{nameof(Test04_SquareRoot)} passed?: {step04_passed}");
			TestContext.WriteLine($"LEAVE: {nameof(Test04_SquareRoot)}");
		}




	}
}
