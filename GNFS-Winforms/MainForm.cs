using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Polynomial;
	using GNFSCore.FactorBase;
	using GNFSCore.IntegerMath;
	using GNFSCore.PrimeSignature;

	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			tbN.Text = "3218147"; //"1001193673991790373"; //"45113";//"3218147"; //"3580430111"
			tbBase.Text = "117"; //"11875";//"117";//"31";"127";
			tbDegree.Text = "3"; // "5";
		}

		public void LogOutput(string message = "")
		{
			if (tbOutput.InvokeRequired)
			{
				tbOutput.Invoke(new MethodInvoker(() => LogOutput(message)));
			}
			else
			{
				tbOutput.AppendText(message + Environment.NewLine);
			}
		}

		private void tbOutput_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				if (e.KeyCode == Keys.A)
				{
					tbOutput.SelectAll();
				}
			}
		}

		private static string FormatTupleCollection(IEnumerable<Tuple<int, int>> tuples)
		{
			return string.Join("\t", tuples.Select(tup => $"({tup.Item1},{tup.Item2})"));
		}

		private static string FormatArrayList(IEnumerable<int[]> array)
		{
			return string.Join("\t", array.Select(tup => $"({string.Join(",", tup.Select(i => i.ToString()))})"));

		}

		private static string FormatList(IEnumerable<int> array)
		{
			return $"({string.Join(",", array.Select(i => i.ToString()))})";

		}

		private void btnGetFactorBases_Click(object sender, EventArgs e)
		{
			BigInteger n = BigInteger.Parse(tbN.Text);
			BigInteger polyBase = BigInteger.Parse(tbBase.Text);
			int degree = int.Parse(tbDegree.Text);

			GNFS gnfs = new GNFS(n, polyBase, degree);

			tbBound.Text = gnfs.PrimeBound.ToString();

			LogOutput($"N = {gnfs.N}");
			LogOutput();

			LogOutput($"Polynomial(degree: {degree}, base: {polyBase}):");
			LogOutput(gnfs.AlgebraicPolynomial.ToString());
			LogOutput();

			LogOutput($"Rational Factor Base (RFB; Smoothness-bound: {gnfs.PrimeBound}):");
			LogOutput(FormatTupleCollection(gnfs.RFB));
			LogOutput();

			LogOutput($"Algebraic Factor Base (AFB):");
			LogOutput(FormatTupleCollection(gnfs.AFB));
			LogOutput();

			LogOutput($"Quadratic Factor Base (QFB):");
			LogOutput(FormatTupleCollection(gnfs.QFB));
			LogOutput();

			IEnumerable<Relation> smoothRelations = gnfs.GenerateRelations(200);
			smoothRelations = smoothRelations.OrderBy(rel => QuadraticResidue.IsQuadraticResidue(rel.A, rel.B));

			LogOutput($"Smooth relations:");
			LogOutput("\t_______________________________________________");
			LogOutput($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tQuantity: {(gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count() + 1).ToString()}");
			LogOutput("\t```````````````````````````````````````````````");
			LogOutput(smoothRelations.FormatString());
			LogOutput();

			Relation[] exampleFromThesis = new Relation[]
			{
				new Relation(-127, 1,       gnfs.AlgebraicPolynomial),
				new Relation(-2, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(23, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(-65, 3,        gnfs.AlgebraicPolynomial) ,
				new Relation(-137, 5,       gnfs.AlgebraicPolynomial),
				new Relation(-126, 1,       gnfs.AlgebraicPolynomial),
				new Relation(0, 1,          gnfs.AlgebraicPolynomial)   ,
				new Relation(24, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(-62, 3,        gnfs.AlgebraicPolynomial) ,
				new Relation(-68, 5,        gnfs.AlgebraicPolynomial) ,
				new Relation(-12, 1,        gnfs.AlgebraicPolynomial) ,
				new Relation(2, 1,          gnfs.AlgebraicPolynomial)   ,
				new Relation(37, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(86, 3,         gnfs.AlgebraicPolynomial)  ,
				new Relation(-46, 5,        gnfs.AlgebraicPolynomial) ,
				new Relation(-5, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(19, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(81, 1,         gnfs.AlgebraicPolynomial)  ,
				new Relation(181, 3,        gnfs.AlgebraicPolynomial) ,
				new Relation(31, 5,         gnfs.AlgebraicPolynomial)
			};

			exampleFromThesis = smoothRelations.ToArray();

			IEnumerable<int> factoringExample = exampleFromThesis.Select(rel => (int)BigInteger.Abs(rel.RationalNorm)); //smoothRelations.Select(rel => Math.Abs(rel.A)).Distinct().OrderBy(i => i);
			factoringExample = factoringExample.Where(i => i > 1 && !PrimeFactory.IsPrime(i));

			LogOutput($"Prime factorization example:");
			LogOutput(string.Join(Environment.NewLine, factoringExample.Select(i => $"{i}: ".PadRight(5) + FactorizationFactory.FormatString.PrimeFactorization(FactorizationFactory.GetPrimeFactorizationTuple(i, gnfs.PrimeBound)))));
			LogOutput();


			var signatureMatrix = smoothRelations.Select(rel => (int)BigInteger.Abs(rel.RationalNorm));
			BitMatrix primeSignatureMatrix = new BitMatrix(signatureMatrix, gnfs.PrimeBound);

			LogOutput($"Prime signature binary matrix:");
			LogOutput(primeSignatureMatrix.ToString());
			LogOutput();

			/*
			IEnumerable<int[]> squareCombos = primeSignatureMatrix.GetSquareCombinations();
			LogOutput($"Perfect squares:");
			LogOutput(string.Join(Environment.NewLine, squareCombos.Select(i => $"{string.Join("*", i)} = {i.Select(m => new BigInteger(m)).Product()}")));
			LogOutput();
			*/

			LogOutput("Example Relations (From thesis):");
			LogOutput(exampleFromThesis.FormatString());
			LogOutput();

			var algebraicNormQuadraticCharacter = exampleFromThesis.Select(rel => Legendre.Symbol(BigInteger.Abs(rel.AlgebraicNorm), n));
			var rationalNormQuadraticCharacter = exampleFromThesis.Select(rel => Legendre.Symbol(BigInteger.Abs(rel.RationalNorm), n));

			LogOutput("Algebraic Norm (From thesis) Quadratic Character:");
			LogOutput(algebraicNormQuadraticCharacter.FormatString());
			LogOutput();

			LogOutput("Rational Norm (From thesis) Quadratic Character:");
			LogOutput(rationalNormQuadraticCharacter.FormatString());
			LogOutput();


			BigInteger polyDerivative = BigInteger.Multiply((BigInteger)gnfs.AlgebraicPolynomial.FormalDerivative, (BigInteger)gnfs.AlgebraicPolynomial.FormalDerivative);
			BigInteger polyValue = Irreducible.Evaluate(gnfs.AlgebraicPolynomial, gnfs.AlgebraicPolynomial.Base);
			LogOutput("Polynomial value f(x):");
			LogOutput(polyValue.ToString());
			LogOutput();
			LogOutput("Polynomial derivative f'(m)^2:");
			LogOutput(polyDerivative.ToString());
			LogOutput();

			LogOutput("Prime Bound:");
			LogOutput(gnfs.PrimeBound.ToString());
			LogOutput();

			SquareFinder sqFinder = new SquareFinder(gnfs, exampleFromThesis);
			sqFinder.CalculateRationalSide();
			sqFinder.CalculateRationalModPolynomial();
			sqFinder.CalculateAlgebraicSide();

			LogOutput("IsIrreducible:");
			LogOutput((sqFinder.IsAlgebraicIrreducible && sqFinder.IsRationalIrreducible).ToString());
			LogOutput();
			
			BigInteger productC = exampleFromThesis.Select(rel => rel.C).Where(i => !i.IsZero).ProductMod(n);
			BigInteger gcd = GCD.FindGCD(n, productC);

			LogOutput();
			LogOutput("GCD(N, relations.Select(rel => f(rel.A)).Product() ):");
			LogOutput($"Product: {productC}");
			LogOutput();
			LogOutput($"Product%N: {productC%n}");
			LogOutput();
			LogOutput($"GCD(N,Product): {gcd}");
			LogOutput();
		}

		private void PrintSquareResults(SquareFinder sqFinder)
		{

			LogOutput("Square finder, rational:");
			LogOutput($"  √( {sqFinder.RationalProduct} * {sqFinder.SquarePolynomialDerivative} )");
			LogOutput($"= √( {sqFinder.RationalInverseSquare} )");
			LogOutput($"=    {sqFinder.RationalInverseSquareRoot}\n");
			LogOutput($"Product: {sqFinder.RationalProduct}");
			LogOutput($"ProductMod: {sqFinder.RationalProductMod}");
			LogOutput($"*InverseSquare: {sqFinder.RationalInverseSquare}");
			LogOutput($"Sum: {sqFinder.RationalSum}");
			LogOutput($"SumOfNorms: {sqFinder.RationalNormSum}");
			LogOutput($"IsRationalSquare ? {sqFinder.IsRationalSquare}");
			LogOutput($"IsRationalIrreducible ? {sqFinder.IsRationalIrreducible}");

			LogOutput();
			LogOutput($"RationalModPolynomial: {sqFinder.RationalModPolynomial}");
			LogOutput();



			LogOutput("Square finder, algebraic:");
			LogOutput($"Product: {sqFinder.AlgebraicProduct}");
			LogOutput($"ProductMod: {sqFinder.AlgebraicProductMod}");
			LogOutput($"Sum: {sqFinder.AlgebraicSum}");
			LogOutput($"SumOfNorms: {sqFinder.AlgebraicNormSum}");
			LogOutput($"IsAlgebraicSquare ? {sqFinder.IsAlgebraicSquare}");
			LogOutput($"IsAlgebraicIrreducible ? {sqFinder.IsAlgebraicIrreducible}");
			LogOutput();
			LogOutput();

		}
	}
}

