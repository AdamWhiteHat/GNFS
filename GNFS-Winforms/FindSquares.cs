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
			BigInteger polyBase = gnfs.PolynomialBase;

			Func<BigInteger, BigInteger> f = poly.Evaluate;
			Func<BigInteger, BigInteger> fD = poly.Derivative;

			List<Relation> freeRelations = gnfs.CurrentRelationsProgress.FreeRelations.First().ToList();

			SquareFinder squareRootFinder = new SquareFinder(gnfs, freeRelations);
			squareRootFinder.CalculateRationalSide();
			squareRootFinder.CalculateAlgebraicSide();



			mainForm.LogOutput();
			mainForm.LogOutput("(a+b*x): " + string.Join(" * ", freeRelations.Select(rel => $"({rel.A}+({rel.B}*x))")));
			mainForm.LogOutput();
			mainForm.LogOutput("RationalNorms: " + string.Join(" * ", freeRelations.Select(rel => $"{rel.RationalNorm}")));
			mainForm.LogOutput();
			mainForm.LogOutput("AlgebraicNorms: " + string.Join(" * ", freeRelations.Select(rel => $"{rel.AlgebraicNorm}")));
			mainForm.LogOutput();
			mainForm.LogOutput("AlgebraicNorms (mod N): " + string.Join(" * ", freeRelations.Select(rel => $"{rel.AlgebraicNorm % N}")));
			mainForm.LogOutput();
			mainForm.LogOutput();
			mainForm.LogOutput($"Sum of rationalNorms: {freeRelations.Select(rel => rel.RationalNorm).Sum()}");
			mainForm.LogOutput();
			mainForm.LogOutput($"Sum  of algebraicNorms (mod N) {freeRelations.Select(rel => rel.AlgebraicNorm % N).Sum()}");
			mainForm.LogOutput();


			Complex cplxAlgSum = squareRootFinder.AlgebraicComplexSet.Sum();
			Complex cplxAlgProd = squareRootFinder.AlgebraicComplexSet.Product();
			mainForm.LogOutput();
			mainForm.LogOutput("AlgebraicComplexSet:");
			mainForm.LogOutput(string.Join(" + ", squareRootFinder.AlgebraicComplexSet.Select(cmplx => $"({cmplx.Real} + {cmplx.Imaginary}i)")));
			mainForm.LogOutput();
			mainForm.LogOutput("Sum:");
			mainForm.LogOutput($"{cplxAlgSum}");
			mainForm.LogOutput();
			mainForm.LogOutput("Product:");
			mainForm.LogOutput($"{cplxAlgProd}");
			mainForm.LogOutput();

			Complex sqrdCplxAlgSum = Complex.Abs(cplxAlgSum); //Complex.Pow(cplxAlgSum, 2);
			Complex sqrdCplxAlgProd = Complex.Abs(cplxAlgProd); //Complex.Pow(cplxAlgProd, 2);
			mainForm.LogOutput();
			mainForm.LogOutput("Sum squared:");
			mainForm.LogOutput($"{sqrdCplxAlgSum}");
			mainForm.LogOutput();
			mainForm.LogOutput("Product squared:");
			mainForm.LogOutput($"{sqrdCplxAlgProd}");
			mainForm.LogOutput();

			BigInteger S = squareRootFinder.S.Evaluate(polyBase);

			mainForm.LogOutput();
			mainForm.LogOutput($"f'(m)  = {squareRootFinder.PolynomialDerivative}");
			mainForm.LogOutput($"f'(m)^2= {squareRootFinder.PolynomialDerivativeSquared}");
			mainForm.LogOutput($"γ²     = {squareRootFinder.RationalProduct}");
			mainForm.LogOutput($"γ      = {squareRootFinder.RationalSquareRootResidue}");
			mainForm.LogOutput($"S(a,b) = {S}");
			mainForm.LogOutput($"Sₐ(m)  = {S}");
			mainForm.LogOutput($"S (x)  = {S % N}");//{squareRootFinder.AlgebraicSquareResidue}");
			mainForm.LogOutput();

			BigInteger X = S % N;
			BigInteger Y = squareRootFinder.RationalSquareRootResidue;

			BigInteger gcd1 = GCD.FindGCD(N, Y + S);
			BigInteger gcd2 = GCD.FindGCD(N, Y - S);

			mainForm.LogOutput("GCD: " + gcd1);
			mainForm.LogOutput("GCD: " + gcd2);
			mainForm.LogOutput();

			if (gcd1 != 1 || gcd2 != 1)
			{
				mainForm.LogOutput();
				mainForm.LogOutput("################");
				mainForm.LogOutput("##  SUCCESS!  ##");
				mainForm.LogOutput("################");
				mainForm.LogOutput();
			}


			//   ƒ(r) ≡ 0(mod p)

			//   ƒ(-p / r) * -p ^ deg

			//squareRootFinder.AlgebraicComplexSet.Product()

			//   (Zp[x]/f(x))[y]/(y^2 − S)
			//   compute the ((p^d)-1)/2  power of r(x) modulo (y^2 − S)
			//   (r(x) − y) ^ ((p^d)-1)/2) 


			return gnfs;
		}

	}
}
