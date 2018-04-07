using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Polynomial;
	using GNFSCore.Factors;
	using GNFSCore.IntegerMath;
	using GNFSCore.Matrix;
	using GNFSCore.Polynomial.Internal;
	using GNFSCore.SquareRoot;

	public partial class GnfsUiBridge
	{
		public GNFS FindSquares(GNFS gnfs, CancellationToken cancelToken)
		{
			if (cancelToken.IsCancellationRequested)
			{
				return gnfs;
			}

			BigInteger N = gnfs.N;
			IPolynomial poly = gnfs.CurrentPolynomial;
			Func<BigInteger, BigInteger> f = poly.Evaluate;
			Func<BigInteger, BigInteger> fD = poly.Derivative;


			List<Relation> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations.First().ToList();

			SquareFinder squareRootFinder = new SquareFinder(gnfs, freeRelations);
			squareRootFinder.CalculateRationalSide();
			squareRootFinder.CalculateAlgebraicSide();

			BigInteger Y = squareRootFinder.AlgebraicProductMod;
			BigInteger S = squareRootFinder.AlgebraicProductMod;

			mainForm.LogOutput($"f'(m)  = {squareRootFinder.PolynomialDerivative}");
			mainForm.LogOutput($"f'(m)^2= {squareRootFinder.PolynomialDerivativeSquared}");
			mainForm.LogOutput($"γ²     = {squareRootFinder.RationalProduct}");
			mainForm.LogOutput($"γ      = {squareRootFinder.RationalSquareRootResidue}");
			mainForm.LogOutput($"S(a,b) = {squareRootFinder.AlgebraicProduct}");
			mainForm.LogOutput($"Sₐ(m)  = {squareRootFinder.AlgebraicProductMod}");
			mainForm.LogOutput($"S (x)  = {squareRootFinder.AlgebraicSquareResidue}");
			mainForm.LogOutput();



			AlgebraicPolynomial Srat = new AlgebraicPolynomial(squareRootFinder.RationalProduct, N, (poly.Degree - 1));
			AlgebraicPolynomial Salg1 = new AlgebraicPolynomial(N, poly.Base, (poly.Degree - 1));
			AlgebraicPolynomial Salg2 = new AlgebraicPolynomial(squareRootFinder.AlgebraicProduct, squareRootFinder.AlgebraicProduct.SquareRoot(), (poly.Degree - 1));

			mainForm.LogOutput("Polynomial RationalProduct     = " + Srat.ToString());
			mainForm.LogOutput("Polynomial AlgebraicProductMod = " + Salg1.ToString());
			mainForm.LogOutput("Polynomial AlgebraicProduct    = " + Salg2.ToString());


			mainForm.LogOutput();
			mainForm.LogOutput("(a+b*x): " + string.Join(" * ", freeRelations.Select(rel => $"({rel.A}+({rel.B}*x))")));
			mainForm.LogOutput();
			mainForm.LogOutput("RationalNorms: " + string.Join(" * ", freeRelations.Select(rel => $"{rel.RationalNorm}")));
			mainForm.LogOutput();
			mainForm.LogOutput("AlgebraicNorms: " + string.Join(" * ", freeRelations.Select(rel => $"{rel.AlgebraicNorm}")));
			mainForm.LogOutput();

			var range = Enumerable.Range(-2000, 4000).ToList();
			var fofrange = range.Select(x => f(x)).ToList();
			var roots = fofrange.Where(n => n == 0);


			mainForm.LogOutput($"");
			mainForm.LogOutput($"f(x):");
			mainForm.LogOutput($"{f(0)}");
			mainForm.LogOutput($"{f(1)}");
			mainForm.LogOutput($"{f(3)}");
			mainForm.LogOutput($"{f(5)}");
			mainForm.LogOutput($"{f(62)}");
			mainForm.LogOutput($"{f(poly.Base)}");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"fD(x):");
			mainForm.LogOutput($"{fD(0)}");
			mainForm.LogOutput($"{fD(1)}");
			mainForm.LogOutput($"{fD(3)}");
			mainForm.LogOutput($"{fD(5)}");
			mainForm.LogOutput($"{fD(62)}");
			mainForm.LogOutput($"{fD(poly.Base)}");
			mainForm.LogOutput($"{fD(poly.Base + 1)}");
			mainForm.LogOutput($"{fD(poly.Base + 2)}");
			mainForm.LogOutput($"{fD(poly.Base + 3)}");
			mainForm.LogOutput($"{fD(poly.Base + 4)}");

			mainForm.LogOutput($"");
			mainForm.LogOutput($"Product(roots): {squareRootFinder.RelationsSet.Select(rel => rel.B).Product()}");
			mainForm.LogOutput($"Roots (Norms): {string.Join(", ", squareRootFinder.RelationsSet.Select(rel => rel.B))}");
			mainForm.LogOutput($"");
			mainForm.LogOutput($"Y^2 - S: {BigInteger.Pow(Y, 2) - S} % N = {(BigInteger.Pow(Y, 2) - S) % N} ");
			mainForm.LogOutput($"");
			//mainForm.LogOutput($"{string.Join(", ", range)}");
			//mainForm.LogOutput($"{string.Join(", ", fofrange)}");
			//mainForm.LogOutput($"Roots (calculated): {string.Join(", ", roots)}");
			mainForm.LogOutput($"Product    : {roots.Product()}");
			mainForm.LogOutput($"Product % N: {roots.ProductMod(N)}");
			//var min = fofrange.Select(n => BigInteger.Abs(n)).Min();
			//var index = fofrange.Select(n => BigInteger.Abs(n)).ToList().IndexOf(min);
			//mainForm.LogOutput($"{range[index]}");
			//mainForm.LogOutput($"");


			var a1 = squareRootFinder.RelationsSet.Where(rel => rel.B == 1).Select(rel => rel.A).ToList();
			var a3 = squareRootFinder.RelationsSet.Where(rel => rel.B == 3).Select(rel => rel.A);
			var a5 = squareRootFinder.RelationsSet.Where(rel => rel.B == 5).Select(rel => rel.A);

			a1.Remove(0);

			mainForm.LogOutput($"");
			mainForm.LogOutput($"{a1.Product()}: {string.Join(", ", a1)}");
			mainForm.LogOutput($"{a3.Product()}: {string.Join(", ", a3)}");
			mainForm.LogOutput($"{a5.Product()}: {string.Join(", ", a5)}");
			mainForm.LogOutput($"");







			// (Zp[x]/f(x))[y]/(y^2 − S)
			// compute the ((p^d)-1)/2  power of r(x) modulo (y^2 − S)

			// (r(x) − y) ^ ((p^d)-1)/2) 



			S = squareRootFinder.RationalSquareRootResidue;

			List<BigInteger> result = SquareFinder.SquareRoot.Newtons(f, fD, poly.Base, S, squareRootFinder.RelationsSet, S, poly.Degree, 10000);

			BigInteger solution = result.Last();

			mainForm.LogOutput($"SOLUTION: {solution}");
			










			//BigInteger P = 5;
			//List<BigInteger> result = new List<BigInteger>() { BigInteger.Parse("1") };

			//BigInteger newValue = result.Last();
			//BigInteger lastValue = -1;
			//while (newValue != lastValue)
			//{
			//	lastValue = newValue;
			//	result = Newtons(0, P, 0);
			//	newValue = result.Last();
			//}




			//bool success = squareRootFinder.HenselsLemma();
			//mainForm.LogOutput("HenselsLemma().Success ? " + success.ToString());
			//	SquareRootStep.BeginSquareRoot(gnfs.CurrentPolynomial, freeRelations, gnfs.N, squareRootFinder.AlgebraicSquare);

			/*
		foreach (Relation[] group in gnfs.CurrentRelationsProgress.FreeRelations)
		{
			SquareFinder squareRootFinder = new SquareFinder(gnfs, group.ToList());
			squareRootFinder.CalculateRationalSide();
			squareRootFinder.CalculateAlgebraicSide();

			mainForm.LogOutput(squareRootFinder.ToString());
		}
		*/
			//if (Program.IsDebug())
			//{
			//	// root, rootSet[], rootProduct, rootProduct % ƒ, ƒ(x)
			//	List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>> rootProductModTuples = squareRootFinder.CalculateRootProducts();

			//	int rootPadLength = rootProductModTuples.Select(tup => tup.Item1).Max().ToString().Length;
			//	int productPadLength = rootProductModTuples.Select(tup => BigInteger.Abs(tup.Item4)).Max().ToString().Length + 1;

			//	char tab = '\t';
			//	string rootProductsString = string.Join(Environment.NewLine,
			//		// root, rootSet.ToArray(), rootProduct, rootProduct % f, f
			//		rootProductModTuples.Select(tup => $"a + b∙{tup.Item1.ToString().PadRight(rootPadLength)} = {tup.Item3}{tab}≡{tab}{tup.Item4.ToString().PadLeft(productPadLength)} (mod {tup.Item5})")); //[{tup.Item2.FormatString(false)}]

			//	mainForm.LogOutput("Root products:");
			//	mainForm.LogOutput("( (a₁ + b₁∙x) * … * (aᵢ + bᵢ∙x) ) mod ƒ(x) =");
			//	mainForm.LogOutput(rootProductsString);
			//	mainForm.LogOutput();
			//}

			return gnfs;
		}





		List<BigInteger> Newtons(Func<BigInteger, BigInteger> f, Func<BigInteger, BigInteger> fPrime, BigInteger x, BigInteger p, int n = 0)
		{
			List<BigInteger> results = new List<BigInteger>();
			BigInteger previous = x;
			BigInteger current = 0;
			BigInteger next = 0;
			BigInteger t = 1;

			BigInteger a = 0;
			BigInteger b = -1;

			a = f(previous) % p;
			// Check that:  f(0) ≡ 0 (mod p)
			if (a != 0) throw new ArithmeticException("f(0) ≠ 0 (mod p)");

			b = fD(previous) % p;
			// Check that: f'(0) ≠ 3 (mod p)
			if (b == 0) throw new ArithmeticException("f'(0) ≡ 0 (mod p)");

			previous = b;
			if (n == 0) // Firstheck if we can proceed
			{
				while (true)
				{
					n = n + 1;

					BigInteger j = f(previous);
					BigInteger jD = fD(previous);
					BigInteger pn = BigInteger.Pow(p, n);

					current = j + (pn * jD);

					next = current % pn;


					t = (j + (current)) % (pn * pn);





					BigInteger x2 = ((j / pn) + jD) % pn;
					x2 = a + (x2 * pn);
					if (x2 % pn != 0) break; //throw new ArithmeticException("x2 % p != 0");

					if (t != 0) break;// throw new ArithmeticException("t != 0");


					results.Add(current);

					previous = current;
					current = next;
					next = -1;

					break;
				}
			}
			return results;
		}







		Func<BigInteger, BigInteger> f = (x) => BigInteger.Pow(x, 3) + (8 * x) - 5; // x^3 + 8*x - 5
		Func<BigInteger, BigInteger> fD = (x) => (3 * BigInteger.Pow(x, 2)) + 8; // 3 * x^2 + 8


		BigInteger Newtons2(List<BigInteger> xN, BigInteger p, int n = 0)
		{
			List<BigInteger> results = new List<BigInteger>(1);
			BigInteger x0 = 0;
			BigInteger x1 = -1;
			if (n == 0) // First iteration, check if we can proceed
			{
				n = n + 1;

				x0 = f(0) % p;
				// Check that:  f(0) ≡ 0 (mod p)
				if (x0 != 0) return results.First();//throw new ArithmeticException("f(0) ≠ 0 (mod p)");

				x1 = fD(0) % p;
				// Check that: f'(0) ≠ 3 (mod p)
				if (x1 == 0) return results.First();// throw new ArithmeticException("f'(0) ≡ 0 (mod p)");

				BigInteger x2 =

				((f(x1) / p) + fD(x1)) % p;

				x2 = x0 + (x2 * p);

				if (x2 % p != 0) return results.First();// throw new ArithmeticException("x2 % p != 0");

				results.Add(x2);
				return Newtons2(results, p, n + 1);
			}

			BigInteger xn_1 = xN.Last();

			double a = 1 / (double)(fD(xn_1) % p);

			double t = -a * (double)(f(xn_1) / BigInteger.Pow(p, n)) % (double)p;


			var result = (double)xn_1 - (a * (double)f(xn_1)) % Math.Pow((double)p, n);

			BigInteger tmp = new BigInteger(result);

			results.Add(tmp);

			return tmp;
		}
	}
}
