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
			tbN.Text = "3218147"; //"45113";//"3218147"; //"3580430111"
			tbBase.Text = "117";//"31";"127";
			tbDegree.Text = "3";
			//tbBound.Text = "35";//"60";
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

			LogOutput($"Smooth relations:");			
			LogOutput("______________________________________________");
			LogOutput($"| A | B | ALGEBRAIC_NORM | RATIONAL_NORM |    Quantity: {(gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count() + 1).ToString()}");
			LogOutput("``````````````````````````````````````````````");
			LogOutput(string.Join(Environment.NewLine, smoothRelations.Select(rel => rel.ToString())));
			LogOutput();
			
			IEnumerable<int> factoringExample = smoothRelations.Select(rel => Math.Abs(rel.A)).Distinct().OrderBy(i => i);
			factoringExample = factoringExample.Where(i => i > 1 && !PrimeFactory.IsPrime(i));

			LogOutput($"Prime factorization example:");
			LogOutput(string.Join(Environment.NewLine, factoringExample.Select(i => $"{i}: ".PadRight(5) + FactorizationFactory.FormatString.PrimeFactorization(FactorizationFactory.GetPrimeFactorizationTuple(i, gnfs.PrimeBound)))));
			LogOutput();

			BitMatrix primeSignatureMatrix = new BitMatrix(factoringExample, gnfs.PrimeBound);

			LogOutput($"Prime signature binary matrix:");
			LogOutput(primeSignatureMatrix.ToString());
			LogOutput();


			IEnumerable<int[]> squareCombos = primeSignatureMatrix.GetSquareCombinations();

			LogOutput($"Perfect squares:");
			LogOutput(string.Join(Environment.NewLine, squareCombos.Select(i => $"{string.Join("*", i)} = {i.Select(m => new BigInteger(m)).Product()}")));
			LogOutput();

			var algebraicNorms = smoothRelations.Where(rel => rel.B < 6).Select(rel => rel.AlgebraicNorm);
			var rationalNorms = smoothRelations.Where(rel => rel.B < 6).Select(rel => rel.RationalNorm);

			//string algebraicExponents = string.Join(Environment.NewLine, algebraicNorms.Select(i => FactorizationFactory.GetFactorizationExponents(i, gnfs.PrimeBound)).Select(arr => string.Join(",", arr)));
			//string rationalExponents = string.Join(Environment.NewLine, rationalNorms.Select(i => FactorizationFactory.GetFactorizationExponents(i, gnfs.PrimeBound)).Select(arr => string.Join(",", arr)));
			//string algebraicFactorization = string.Join(Environment.NewLine, algebraicNorms.Select(i => FactorizationFactory.FormatString.PrimeFactorization(FactorizationFactory.GetPrimeFactorizationTuple(i, gnfs.PrimeBound))));
			//string rationalFactorization = string.Join(Environment.NewLine, rationalNorms.Select(i => FactorizationFactory.FormatString.PrimeFactorization(FactorizationFactory.GetPrimeFactorizationTuple(i, gnfs.PrimeBound))));

			// Where every norm's exponent vector is even (i.e. a square number)
			algebraicNorms = algebraicNorms.Where(i => (FactorizationFactory.GetFactorizationExponents(i, gnfs.PrimeBound).Sum() % 2 == 0));
			rationalNorms = rationalNorms.Where(i => (FactorizationFactory.GetFactorizationExponents(i, gnfs.PrimeBound).Sum() % 2 == 0));

			//LogOutput($"Algebraic norms factorization:");
			//LogOutput(algebraicFactorization);
			//LogOutput();

			//LogOutput($"Rational norms factorization:");
			//LogOutput(rationalExponents);
			////LogOutput(string.Join(Environment.NewLine, rationalNorms.Select(i => $"{i}: ".PadRight(5) + FactorizationFactory.FormatString.PrimeFactorization(FactorizationFactory.GetPrimeFactorizationTuple(i, gnfs.PrimeBound)))));
			//LogOutput();

			int polyDerivative = (int)Math.Pow(gnfs.AlgebraicPolynomial.Derivative((int)gnfs.AlgebraicPolynomial.Base), 2);
			int polyValue = (int)gnfs.AlgebraicPolynomial.Eval((int)gnfs.AlgebraicPolynomial.Base);
			BigInteger algebraicProduct = algebraicNorms.Product();
			BigInteger rationalProduct = rationalNorms.Product();

			LogOutput("Polynomial value f(x):");
			LogOutput(polyValue.ToString());
			LogOutput();
			LogOutput("Polynomial derivative f'(m)^2:");
			LogOutput(polyDerivative.ToString());
			LogOutput();


			BigInteger rationalSquareRoot = BigInteger.Multiply(rationalProduct, polyDerivative).SquareRoot();
			rationalSquareRoot = rationalSquareRoot % n;

			BigInteger algebraicSquareRoot = BigInteger.Multiply(algebraicProduct, polyDerivative);//.SquareRoot();
			algebraicSquareRoot = algebraicSquareRoot % n;

			LogOutput($"Large rational number (product of sequence of rational norms):");
			LogOutput(rationalProduct.ToString());
			LogOutput($"IsSquare: {BigInteger.Abs(rationalProduct).IsSquare()}");
			LogOutput();
			LogOutput("mod n:");
			LogOutput((rationalProduct % n).ToString());
			LogOutput();

			LogOutput($"Large algebraic number (product of sequence of algebraic norms):");
			LogOutput(algebraicProduct.ToString());
			LogOutput($"IsSquare: {BigInteger.Abs(algebraicProduct).IsSquare()}");
			LogOutput();
			LogOutput("mod n:");
			LogOutput((algebraicProduct % n).ToString());
			LogOutput();

			LogOutput("Rational Square-Root:");
			LogOutput(rationalSquareRoot.ToString());
			LogOutput();

			LogOutput("Algebraic Square-Root:");
			LogOutput(algebraicSquareRoot.ToString());
			LogOutput();





			IEnumerable<string> rows = null;


			




		}
	}
}

