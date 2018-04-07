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

			mainForm.LogOutput();
			mainForm.LogOutput($"Product(roots): {squareRootFinder.RelationsSet.Select(rel => rel.B).Product()}");
			mainForm.LogOutput($"Roots (Norms): {string.Join(", ", squareRootFinder.RelationsSet.Select(rel => rel.B))}");
			mainForm.LogOutput();
			mainForm.LogOutput($"Y^2 - S: {BigInteger.Pow(Y, 2) - S} % N = {(BigInteger.Pow(Y, 2) - S) % N} ");
			mainForm.LogOutput();


			var a1 = squareRootFinder.RelationsSet.Where(rel => rel.B == 1).Select(rel => rel.A).ToList();
			var a3 = squareRootFinder.RelationsSet.Where(rel => rel.B == 3).Select(rel => rel.A);
			var a5 = squareRootFinder.RelationsSet.Where(rel => rel.B == 5).Select(rel => rel.A);

			a1.Remove(0);

			mainForm.LogOutput($"");
			mainForm.LogOutput($"{a1.Product()}: {string.Join(", ", a1)}");
			mainForm.LogOutput($"{a3.Product()}: {string.Join(", ", a3)}");
			mainForm.LogOutput($"{a5.Product()}: {string.Join(", ", a5)}");
			mainForm.LogOutput($"");


			mainForm.LogOutput(string.Join(" + ", squareRootFinder.AlgebraicComplexSet));



			// (Zp[x]/f(x))[y]/(y^2 − S)
			// compute the ((p^d)-1)/2  power of r(x) modulo (y^2 − S)
			// (r(x) − y) ^ ((p^d)-1)/2) 


			return gnfs;
		}

	}
}
