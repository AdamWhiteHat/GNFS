using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GNFSCore.SquareRoot
{
	using Polynomial;
	using IntegerMath;

	public partial class SquareFinder
	{
		public Relation[] RelationsSet;

		public BigInteger PolynomialDerivative;
		public BigInteger PolynomialDerivativeSquared;

		public BigInteger RationalProduct;
		public BigInteger RationalSquare;
		public BigInteger RationalSquareRoot;
		public BigInteger RationalSquareRootResidue;
		public bool IsRationalSquare;
		public bool IsRationalIrreducible;

		public BigInteger AlgebraicProduct;
		public BigInteger AlgebraicProductMod;
		public BigInteger AlgebraicSquareResidue;
		public bool IsAlgebraicSquare;
		public bool IsAlgebraicIrreducible;

		public List<Complex> AlgebraicComplexSet;

		public BigInteger Y2;
		public BigInteger Y2_S;

		private GNFS gnfs;
		private BigInteger N;
		private BigInteger polyBase;
		private Func<BigInteger, BigInteger> f;
		private IEnumerable<BigInteger> rationalSet;
		private IEnumerable<BigInteger> algebraicSet;

		public SquareFinder(GNFS sieve, List<Relation> relations)
		{
			RationalSquareRootResidue = -1;

			gnfs = sieve;
			N = gnfs.N;
			polyBase = gnfs.CurrentPolynomial.Base;
			f = (x) => gnfs.CurrentPolynomial.Evaluate(x);


			AlgebraicComplexSet = new List<Complex>();
			RelationsSet = relations.ToArray();
			PolynomialDerivative = gnfs.CurrentPolynomial.Derivative(gnfs.CurrentPolynomial.Base);
			PolynomialDerivativeSquared = BigInteger.Multiply(PolynomialDerivative, PolynomialDerivative);
		}

		private static bool IsIrreducible(IEnumerable<BigInteger> coefficients)
		{
			return (GCD.FindGCD(coefficients) == 1);
		}

		//        ________________
		// y = ( √  S(m) * f'(m)^2 ) mod N
		//
		// y = 2860383 (for example)


		// S(x) mod f(x)
		// 
		// a*x^3+b*x^2+c*x^1+d*x^0

		public void CalculateRationalSide()
		{
			rationalSet = RelationsSet.Select(rel => rel.RationalNorm);

			RationalProduct = rationalSet.Product();
			RationalSquare = BigInteger.Multiply(RationalProduct, PolynomialDerivativeSquared);
			RationalSquareRoot = RationalSquare.SquareRoot();
			RationalSquareRootResidue = (RationalSquareRoot % N);

			IsRationalIrreducible = IsIrreducible(rationalSet);
			IsRationalSquare = RationalSquareRootResidue.IsSquare();
		}

		public void CalculateAlgebraicSide()
		{
			algebraicSet = RelationsSet.Select(rel => rel.AlgebraicNorm);

			AlgebraicProduct = algebraicSet.Product();
			AlgebraicProductMod = AlgebraicProduct % f(polyBase);
			AlgebraicSquareResidue = AlgebraicProductMod % N;

			IsAlgebraicIrreducible = IsIrreducible(algebraicSet); // Irreducible check
			IsAlgebraicSquare = AlgebraicSquareResidue.IsSquare();

			AlgebraicComplexSet = RelationsSet.Select(rel => new Complex(rel.A, rel.B)).ToList();
		}

		public List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>> CalculateRootProducts()
		{
			List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>> results = new List<Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>>();

			IEnumerable<int> roots = gnfs.AFB.Select(fp => fp.R).Distinct();
			foreach (int root in roots)
			{
				BigInteger f = gnfs.CurrentPolynomial.Evaluate(root);

				IEnumerable<BigInteger> rootSet = RelationsSet.Select(rel => rel.Apply(root));

				BigInteger rootProduct = rootSet.Product();

				results.Add(new Tuple<int, BigInteger[], BigInteger, BigInteger, BigInteger>(root, rootSet.ToArray(), rootProduct, rootProduct % f, f));
			}

			return results.OrderBy(tup => BigInteger.Abs(tup.Item4)).ThenBy(tup => tup.Item1).ThenBy(tup => tup.Item5).ToList();
		}

		private BigInteger LogExpand(int logBase, double logExponent)
		{
			int logWhole = (int)Math.Floor(logExponent);
			double logDecimal = logExponent % 1;

			BigInteger resultWhole = BigInteger.Pow(logBase, logWhole);
			double resultDecimal = Math.Pow(logBase, logDecimal);

			BigInteger result = BigInteger.Multiply(resultWhole, (int)Math.Floor(resultDecimal));

			double diffDecimal = (resultDecimal % 1);

			int resultDiff = (int)Math.Floor(1 / diffDecimal);
			BigInteger resultAdd = BigInteger.Divide(result, resultDiff);

			result += resultAdd;
			return result;
		}






		public static class SquareRoot
		{
			private static Func<BigInteger, BigInteger, BigInteger, BigInteger> eval = (n, increment, prime) =>
			{
				return 1 + (n * prime * increment);
			};

			public static List<BigInteger> Newtons(Func<BigInteger, BigInteger> f,
													Func<BigInteger, BigInteger> fD,
													BigInteger toSquareRoot,
													BigInteger numberToFactor,
													Relation[] relations,
													BigInteger relationPolynomial,
													int polynomialDegree,
													int maxIterations
												   )
			{
				int k = 1;
				int counter = -1;
				BigInteger mod = 1;
				BigInteger rational = -1;
				BigInteger lastValue = 1;
				BigInteger m = toSquareRoot;
				BigInteger N = numberToFactor;
				BigInteger currentValue = 400;
				int degree = polynomialDegree;
				BigInteger S = relationPolynomial;
				List<BigInteger> results = new List<BigInteger>();
				if (maxIterations < 1) maxIterations = UInt16.MaxValue;

				// Check
				do { rational = (eval(N, k++, 2)) % f(m); } while (rational != 1 && counter++ <= maxIterations);


				// compute the (p ^ d - 1) / 2 power of r - y modulo y ^ 2 - S, with each coefficient of y reduced modulo f and p.


				BigInteger n = 0;
				BigInteger r = 0;
				BigInteger p = 0;
				BigInteger R_0 = 0;
				BigInteger value = 0;
				BigInteger exponent = 0;
				BigInteger prime128 = 0;
				BigInteger sizeOf127bits = BigInteger.Pow(2, 127);
				BigInteger sizeOf128bits = BigInteger.Pow(2, 128);

				RandomPolynomial randomPolynomial = new RandomPolynomial(degree, 0, sizeOf128bits);

				counter = -1;
				while (counter++ <= maxIterations && currentValue != 0)
				{
					lastValue = currentValue;

					prime128 = StaticRandom.NextBigInteger(sizeOf127bits, sizeOf128bits);

					randomPolynomial.RandomizeCoefficients();



					Relation rel = relations[StaticRandom.Next(relations.Length)];
					BigInteger Y = rel.B;
					rel = relations[StaticRandom.Next(relations.Length)];
					r = randomPolynomial.Evaluate(m);


					mod = (BigInteger.Pow(Y, 2) - S);
					exponent = BigInteger.Divide(BigInteger.Pow(prime128, degree - 1), 2);


					value = BigInteger.ModPow(r - Y, exponent, mod);


					// (r-y)^((p^d-1)/2) % (y^2 - S)
					R_0 = (value % mod) % f(m); ;


					results.Add(currentValue);



					p = BigInteger.Pow(prime128, counter);

					currentValue = BigInteger.Pow(R_0 % p, 2);


					if (currentValue == S)
					{
						break;
					}


				}

				return results;
			}
		}



		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			/*
			result.AppendLine($"IsRationalIrreducible  ? {IsRationalIrreducible}");
			result.AppendLine($"IsAlgebraicIrreducible ? {IsAlgebraicIrreducible}");

			result.AppendLine("Square finder, Rational:");
			result.AppendLine($"γ² = √(  Sᵣ(m)  *  ƒ'(m)²  )");
			result.AppendLine($"γ² = √( {RationalProduct} * {PolynomialDerivativeSquared} )");
			result.AppendLine($"γ² = √( {RationalSquare} )");
			result.AppendLine($"γ  =    {RationalSquareRoot} mod N");
			result.AppendLine($"γ  =    {RationalSquareRootResidue}"); // δ mod N 

			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Algebraic:");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicProduct} * {PolynomialDerivative}");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicSquare}");
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareResidue}");
			*/

			result.AppendLine($"γ = {RationalSquareRootResidue}");
			result.AppendLine($"χ = {AlgebraicSquareResidue}");

			result.AppendLine($"IsRationalSquare  ? {IsRationalSquare}");
			result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");



			BigInteger min = BigInteger.Min(RationalSquareRoot, AlgebraicSquareResidue);
			BigInteger max = BigInteger.Max(RationalSquareRoot, AlgebraicSquareResidue);

			BigInteger add = max + min;
			BigInteger sub = max - min;

			BigInteger gcdAdd = GCD.FindGCD(N, add);
			BigInteger gcdSub = GCD.FindGCD(N, sub);

			BigInteger answer = BigInteger.Max(gcdAdd, gcdSub);


			result.AppendLine();
			result.AppendLine($"GCD(N, γ+χ) = {gcdAdd}");
			result.AppendLine($"GCD(N, γ-χ) = {gcdSub}");
			result.AppendLine();
			result.AppendLine($"Solution? {(answer != 1).ToString().ToUpper()}");

			if (answer != 1)
			{
				result.AppendLine();
				result.AppendLine();
				result.AppendLine("*********************");
				result.AppendLine();
				result.AppendLine($" SOLUTION = {answer} ");
				result.AppendLine();
				result.AppendLine("*********************");
				result.AppendLine();
				result.AppendLine();
			}

			result.AppendLine();



			return result.ToString();
		}
	}
}
