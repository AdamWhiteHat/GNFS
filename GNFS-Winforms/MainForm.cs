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
			LogOutput(smoothRelations.FormatString());
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


			int polyDerivative = (int)Math.Pow(gnfs.AlgebraicPolynomial.FormalDerivative, 2.0f);
			int polyValue = (int)gnfs.AlgebraicPolynomial.Eval((int)gnfs.AlgebraicPolynomial.Base);
			LogOutput("Polynomial value f(x):");
			LogOutput(polyValue.ToString());
			LogOutput();
			LogOutput("Polynomial derivative f'(m)^2:");
			LogOutput(polyDerivative.ToString());
			LogOutput();

			Relation[] exampleFromBook = new Relation[]
			{
				new Relation(-127, 1, gnfs.AlgebraicPolynomial),
				new Relation(-2, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(23, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(-65, 3, gnfs.AlgebraicPolynomial) ,
				new Relation(-137, 5, gnfs.AlgebraicPolynomial),
				new Relation(-126, 1, gnfs.AlgebraicPolynomial),
				new Relation(0, 1, gnfs.AlgebraicPolynomial)   ,
				new Relation(24, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(-62, 3, gnfs.AlgebraicPolynomial) ,
				new Relation(-68, 5, gnfs.AlgebraicPolynomial) ,
				new Relation(-12, 1, gnfs.AlgebraicPolynomial) ,
				new Relation(2, 1, gnfs.AlgebraicPolynomial)   ,
				new Relation(37, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(86, 3, gnfs.AlgebraicPolynomial)  ,
				new Relation(-46, 5, gnfs.AlgebraicPolynomial) ,
				new Relation(-5, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(19, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(81, 1, gnfs.AlgebraicPolynomial)  ,
				new Relation(181, 3, gnfs.AlgebraicPolynomial) ,
				new Relation(31, 5, gnfs.AlgebraicPolynomial)
			};

			string relsString = exampleFromBook.FormatString();
			LogOutput("Example Relations (From book):");
			LogOutput(relsString);
			LogOutput();

			SquareFinder sqFinder = new SquareFinder(gnfs, exampleFromBook);
			sqFinder.CalculateRationalSide();

			LogOutput("Square finder, rational:");
			LogOutput($"√( {sqFinder.rationalSetProduct} * {sqFinder.SquarePolynomialDerivative} )");
			LogOutput("=");
			LogOutput(sqFinder.RationalSquareRoot.ToString());
			LogOutput("" + sqFinder.IsRationalSquare.ToString());
			LogOutput("" + sqFinder.IsRationalIrreducible.ToString());
			LogOutput();

			sqFinder.CalculateAlgebraicSide();

			LogOutput("Square finder, rational:");
			LogOutput($"{sqFinder.algebraicSet}");
			LogOutput("=");
			LogOutput(sqFinder.AlgebraicSquareRoot.ToString());
			LogOutput("" + sqFinder.IsAlgebraicSquare.ToString());
			LogOutput("" + sqFinder.IsAlgebraicIrreducible.ToString());
			LogOutput();

		}
	}
}

