using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore.Core.Data
{
	using ExtendedNumerics.Internal;
	using GNFSCore.Core.Algorithm;
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Algorithm.IntegerMath;
	using GNFSCore.Core.Algorithm.SquareRoot;
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.RelationSieve;

	using System.IO;
	using System.Net.Http.Headers;
	using System.Text.RegularExpressions;
	using static GNFSCore.Core.Data.GNFS;

	public partial class SquareRoot
	{
		public BigInteger RationalProduct { get; set; }
		public BigInteger RationalSquare { get; set; }
		public BigInteger RationalSquareRootResidue { get; set; }
		public bool IsRationalSquare { get; set; }
		public bool IsRationalIrreducible { get; set; }

		public BigInteger AlgebraicProduct { get; set; }
		public BigInteger AlgebraicSquare { get; set; }
		public BigInteger AlgebraicProductModF { get; set; }
		public BigInteger AlgebraicSquareResidue { get; set; }
		public BigInteger AlgebraicSquareRootResidue { get; set; }
		public List<BigInteger> AlgebraicPrimes { get; set; }
		public List<BigInteger> AlgebraicResults { get; set; }
		public bool IsAlgebraicSquare { get; set; }
		public bool IsAlgebraicIrreducible { get; set; }

		public BigInteger N { get; set; }
		public Polynomial S { get; set; }
		public Polynomial TotalS { get; set; }
		public List<Tuple<BigInteger, BigInteger>> RootsOfS { get; set; }
		public Polynomial PolynomialRing { get; set; }
		public List<Polynomial> PolynomialRingElements { get; set; }

		public BigInteger PolynomialBase { get; set; }
		public Polynomial MonicPolynomial { get; set; }
		public Polynomial PolynomialDerivative { get; set; }
		public Polynomial MonicPolynomialDerivative { get; set; }

		public Polynomial PolynomialDerivativeSquared { get; set; }
		public Polynomial PolynomialDerivativeSquaredInField { get; set; }

		public BigInteger PolynomialDerivativeValue { get; set; }
		public BigInteger PolynomialDerivativeValueSquared { get; set; }


		public Polynomial MonicPolynomialDerivativeSquared { get; set; }
		public Polynomial MonicPolynomialDerivativeSquaredInField { get; set; }

		public BigInteger MonicPolynomialDerivativeValue { get; set; }
		public BigInteger MonicPolynomialDerivativeValueSquared { get; set; }

		private GNFS gnfs { get; set; }
		public List<BigInteger> RationalNormCollection { get; set; }
		public List<BigInteger> AlgebraicNormCollection { get; set; }
		public List<Relation> RelationsSet { get; set; }

		public BigInteger InertPrime_LastValue
		{
			get { return gnfs.SquareRoot_Progress_InertPrime_LastValue; }
			set { gnfs.SquareRoot_Progress_InertPrime_LastValue = value; }
		}

		public LogMessageDelegate LogFunction;

		public SquareRoot(GNFS sieve)
		{
			LogFunction = sieve.LogMessage;

			RationalSquareRootResidue = -1;
			RootsOfS = new List<Tuple<BigInteger, BigInteger>>();

			gnfs = sieve;
			N = gnfs.N;
			PolynomialBase = gnfs.PolynomialBase;

			PolynomialDerivative = Polynomial.GetDerivativePolynomial(gnfs.CurrentPolynomial);
			PolynomialDerivativeSquared = Polynomial.Square(PolynomialDerivative);
			PolynomialDerivativeSquaredInField = Polynomial.Field.Modulus(PolynomialDerivativeSquared, gnfs.CurrentPolynomial);

			LogFunction.Invoke("");
			LogFunction.Invoke($"ƒ'(θ) = {PolynomialDerivative}");
			LogFunction.Invoke($"ƒ'(θ)² = {PolynomialDerivativeSquared}");
			LogFunction.Invoke($"ƒ'(θ)² ∈ ℤ[θ] = {PolynomialDerivativeSquaredInField}");

			PolynomialDerivativeValue = PolynomialDerivative.Evaluate(gnfs.PolynomialBase);
			PolynomialDerivativeValueSquared = BigInteger.Pow(PolynomialDerivativeValue, 2);

			LogFunction.Invoke("");
			LogFunction.Invoke($"ƒ'(m) = {PolynomialDerivativeValue}");
			LogFunction.Invoke($"ƒ'(m)² = {PolynomialDerivativeValueSquared}");


			MonicPolynomial = Polynomial.MakeMonic(gnfs.CurrentPolynomial, PolynomialBase);
			MonicPolynomialDerivative = Polynomial.GetDerivativePolynomial(MonicPolynomial);
			MonicPolynomialDerivativeSquared = Polynomial.Square(MonicPolynomialDerivative);
			MonicPolynomialDerivativeSquaredInField = Polynomial.Field.Modulus(MonicPolynomialDerivativeSquared, MonicPolynomial);

			MonicPolynomialDerivativeValue = MonicPolynomialDerivative.Evaluate(gnfs.PolynomialBase);
			MonicPolynomialDerivativeValueSquared = MonicPolynomialDerivativeSquared.Evaluate(gnfs.PolynomialBase);

			LogFunction.Invoke("");
			LogFunction.Invoke($"MonicPolynomial: {MonicPolynomial}");
			LogFunction.Invoke($"MonicPolynomialDerivative: {MonicPolynomialDerivative}");
			LogFunction.Invoke($"MonicPolynomialDerivativeSquared: {MonicPolynomialDerivativeSquared}");
			LogFunction.Invoke($"MonicPolynomialDerivativeSquaredInField: {MonicPolynomialDerivativeSquaredInField}");
		}

		private static bool IsPrimitive(IEnumerable<BigInteger> coefficients)
		{
			return GCD.FindGCD(coefficients) == 1;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("Polynomial ring:");
			result.AppendLine($"({string.Join(") * (", PolynomialRingElements.Select(ply => ply.ToString()))})");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ =");
			result.AppendLine($"{PolynomialRing}");
			result.AppendLine();
			result.AppendLine($"ƒ         = {gnfs.CurrentPolynomial}");
			result.AppendLine($"ƒ(m)      = {MonicPolynomial}");
			result.AppendLine($"ƒ'(m)     = {MonicPolynomialDerivative}");
			result.AppendLine($"ƒ'(m)^2   = {MonicPolynomialDerivativeSquared}");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ(m)  *  ƒ'(m)² =");
			result.AppendLine($"{TotalS}");
			result.AppendLine();
			result.AppendLine($"∏ Sᵢ(m)  *  ƒ'(m)² (mod ƒ) =");
			result.AppendLine($"{S}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Rational:");
			result.AppendLine($"γ² = √(  Sᵣ(m)  *  ƒ'(m)²  )");
			result.AppendLine($"γ² = √( {RationalProduct} * {PolynomialDerivativeValueSquared} )");
			result.AppendLine($"γ² = √( {RationalSquare} )");
			result.AppendLine($"IsRationalSquare  ? {IsRationalSquare}");
			result.AppendLine($"γ  =    {RationalSquareRootResidue} mod N"); // δ mod N 
			result.AppendLine($"IsRationalIrreducible  ? {IsRationalIrreducible}");
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("Square finder, Algebraic:");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicProduct} * {PolynomialDerivativeValue}");
			result.AppendLine($"    Sₐ(m) * ƒ'(m)  =  {AlgebraicSquare}");
			result.AppendLine($"IsAlgebraicSquare ? {IsAlgebraicSquare}");
			result.AppendLine($"χ = Sₐ(m) * ƒ'(m) mod N = {AlgebraicSquareRootResidue}");
			result.AppendLine($"IsAlgebraicIrreducible ? {IsAlgebraicIrreducible}");
			result.AppendLine();
			result.AppendLine($"X² / ƒ(m) = {AlgebraicProductModF}  IsSquare? {AlgebraicProductModF.IsSquare()}");
			result.AppendLine($"S (x)       = {AlgebraicSquareResidue}  IsSquare? {AlgebraicSquareResidue.IsSquare()}");
			result.AppendLine($"AlgebraicResults:");
			result.AppendLine($"{AlgebraicResults.FormatString(false)}");
			result.AppendLine();
			result.AppendLine();

			result.AppendLine("Primes:");
			result.AppendLine($"{string.Join(" * ", AlgebraicPrimes)}"); // .RelationsSet.Select(rel => rel.B).Distinct().OrderBy(relB => relB))
			result.AppendLine();
			result.AppendLine();
			//result.AppendLine("Roots of S(x):");
			//result.AppendLine($"{{{string.Join(", ", RootsOfS.Select(tup => (tup.Item2 > 1) ? $"{tup.Item1}/{tup.Item2}" : $"{tup.Item1}"))}}}");
			//result.AppendLine();
			//result.AppendLine();
			//result.AppendLine($"∏(a + mb) = {squareRootFinder.RationalProduct}");
			//result.AppendLine($"∏ƒ(a/b)   = {squareRootFinder.AlgebraicProduct}");
			//result.AppendLine();

			BigInteger min = BigInteger.Min(RationalSquareRootResidue, AlgebraicSquareRootResidue);
			BigInteger max = BigInteger.Max(RationalSquareRootResidue, AlgebraicSquareRootResidue);

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
