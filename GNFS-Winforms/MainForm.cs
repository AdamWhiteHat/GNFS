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
			tbN.Text = "3218147";//"1522605027922533360535618378132637429718068114961380688657908494580122963258952897654000350692006139"; //"1001193673991790373"; //"45113";//"3218147"; //"3580430111"
			tbBase.Text = "117";//"29668737024"; //"11875";//"117";//"31";"127";
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
			LogOutput(gnfs.Algebraic.ToString());
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

			Relation[] smoothRelations = gnfs.GenerateRelations(200);
			smoothRelations = smoothRelations.OrderBy(rel => QuadraticResidue.IsQuadraticResidue(rel.A, rel.B)).ToArray();

			LogOutput($"Smooth relations:");
			LogOutput("\t_______________________________________________");
			LogOutput($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tQuantity: {(gnfs.RFB.Count() + gnfs.AFB.Count() + gnfs.QFB.Count() + 1).ToString()}");
			LogOutput("\t```````````````````````````````````````````````");
			LogOutput(smoothRelations.FormatString());
			LogOutput();

			Relation[] exampleFromThesis = new Relation[]
			{
				new Relation(-127, 1,       gnfs.Algebraic),
				new Relation(-126, 1,       gnfs.Algebraic),
				new Relation(-12, 1,        gnfs.Algebraic),
				new Relation(-5, 1,         gnfs.Algebraic),
				new Relation(-2, 1,         gnfs.Algebraic),
				new Relation(0, 1,          gnfs.Algebraic),
				new Relation(2, 1,          gnfs.Algebraic),
				new Relation(19, 1,         gnfs.Algebraic),
				new Relation(23, 1,         gnfs.Algebraic),
				new Relation(24, 1,         gnfs.Algebraic),
				new Relation(37, 1,         gnfs.Algebraic),
				new Relation(81, 1,         gnfs.Algebraic),
				new Relation(-65, 3,        gnfs.Algebraic),
				new Relation(-62, 3,        gnfs.Algebraic),
				new Relation(86, 3,         gnfs.Algebraic),
				new Relation(181, 3,        gnfs.Algebraic),
				new Relation(-137, 5,       gnfs.Algebraic),
				new Relation(-68, 5,        gnfs.Algebraic),
				new Relation(-46, 5,        gnfs.Algebraic),
				new Relation(31, 5,         gnfs.Algebraic)
			};

			//exampleFromThesis = smoothRelations.ToArray();
			smoothRelations = exampleFromThesis;

			//var cCollection = exampleFromThesis.Select(rel => new Tuple<int,int>(rel.A, rel.B));
			//smoothRelations = smoothRelations.Where(rel => cCollection.Contains(new Tuple<int,int>(rel.A, rel.B)) ).ToArray();

			IEnumerable<int> factoringExample = smoothRelations.Select(rel => (int)rel.RationalNorm);
			factoringExample = factoringExample.Where(i => !PrimeFactory.IsPrime(i));

			LogOutput($"Prime factorization example:");
			LogOutput(string.Join(Environment.NewLine, factoringExample.Select(i => $"{i}: ".PadRight(5) + FactorizationFactory.FormatString.PrimeFactorization(FactorizationFactory.GetPrimeFactorizationTuple(i, gnfs.PrimeBound)))));
			LogOutput();


			var signatureMatrix = smoothRelations.Select(rel => (int)rel.AlgebraicNorm);
			BitMatrix primeSignatureMatrix = new BitMatrix(signatureMatrix, gnfs.PrimeBound);

			LogOutput($"Prime signature binary matrix:");
			LogOutput(primeSignatureMatrix.ToString());
			LogOutput();

			LogOutput("Example Relations (From thesis):");
			LogOutput(smoothRelations.FormatString());
			LogOutput();

			BigInteger polyDerivative = BigInteger.Multiply((BigInteger)gnfs.Algebraic.FormalDerivative, (BigInteger)gnfs.Algebraic.FormalDerivative);
			BigInteger polyValue = AlgebraicPolynomial.Evaluate(gnfs.Algebraic, gnfs.Algebraic.Base);
			LogOutput("Polynomial value f(x):");
			LogOutput(polyValue.ToString());
			LogOutput();
			LogOutput("Polynomial derivative f'(m)^2:");
			LogOutput(polyDerivative.ToString());
			LogOutput();
			LogOutput(gnfs.Algebraic.FormalDerivative.ToString());
			LogOutput();

			LogOutput("Prime Bound:");
			LogOutput(gnfs.PrimeBound.ToString());
			LogOutput();

			BigInteger productC = smoothRelations.Select(rel => rel.C).Where(i => !i.IsZero).ProductMod(n);
			BigInteger gcd = GCD.FindGCD(n, productC % n);

			LogOutput();
			LogOutput($"relations.Select(rel => f(rel.C)).Product(): {productC}");
			LogOutput();
			LogOutput($"Product(C)%N: {productC % n}");
			LogOutput();
			LogOutput($"GCD(N,ProductC): {gcd}");
			LogOutput();



			SquareFinder sqFinder = new SquareFinder(gnfs, smoothRelations);
			sqFinder.CalculateRationalSide();
			sqFinder.CalculateRationalModPolynomial();
			//sqFinder.CalculateAlgebraicSide();

			LogOutput("SquareFinder.ToString():");
			LogOutput(sqFinder.ToString());
			LogOutput();

			LogOutput("Rational Square Root:");
			LogOutput(sqFinder.RationalProductMod.ToString());
			LogOutput();

			LogOutput("Algebraic Square Root:");
			LogOutput(sqFinder.AlgebraicProductMod.ToString());
			LogOutput();

			BigInteger x = (sqFinder.RationalProduct * (BigInteger)gnfs.Algebraic.FormalDerivative) % n;

			BigInteger S2lessS = (sqFinder.RationalProductMod * sqFinder.RationalProductMod) - sqFinder.RationalProduct;

			LogOutput($"{sqFinder.RationalProduct} /");
			LogOutput($"γ^2-S: {S2lessS}");
			LogOutput();
			LogOutput($"s(m) * f'(m) % n = {x}");
			LogOutput();


			/*

			List<BigInteger> terms = new List<BigInteger>();
			terms.Add(BigInteger.Parse("33707643386048967064886978071322595680303104670451605589553615208517742239145583274137"));
			terms.Add(BigInteger.Parse("1012438783385021395408772861725005923451102945520342680286858174520561778089965352712171"));
			terms.Add(BigInteger.Parse("1484280452534851932191188732252856860031306910058907052137946073002617221365360609425453"));


			BigInteger p = BigInteger.Parse("42000000000000000000000000000000000000000043");

			RationalPolynomial ratPoly = new RationalPolynomial(n, degree - 1, terms.ToArray());//new RationalPolynomial(n, degree - 1, p, polyBase);

			//

			LogOutput("Rational Polynomial:");
			LogOutput(ratPoly.ToString());
			LogOutput();
			LogOutput("Terms(Alg): " + string.Join(", ", gnfs.Algebraic.Terms.Select(d => d.ToString())));
			LogOutput();

			

						LogOutput();
						LogOutput();
						LogOutput();
						LogOutput();
						*/
		}
	}
}

