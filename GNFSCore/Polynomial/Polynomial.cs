using System;
using System.Xml;
using System.Linq;
using System.Numerics;
using System.Xml.Schema;
using System.Collections;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using GNFSCore.IntegerMath;

namespace GNFSCore.Polynomials
{
	[Serializable]
	public class Polynomial : IPolynomial, IXmlSerializable
	{
		public static IPolynomial Zero = new Polynomial(Term.GetTerms(new BigInteger[] { 0 }));
		public static IPolynomial One = new Polynomial(Term.GetTerms(new BigInteger[] { 1 }));
		public static IPolynomial Two = new Polynomial(Term.GetTerms(new BigInteger[] { 2 }));

		[XmlArrayItem("Terms")]
		public ITerm[] Terms { get { return _terms.ToArray(); } }
		private List<ITerm> _terms = new List<ITerm>();
		public int Degree { get; private set; }

		#region Indexer

		public BigInteger this[int degree]
		{
			get
			{
				ITerm term = Terms.FirstOrDefault(t => t.Exponent == degree);

				if (term == default(ITerm))
				{
					return BigInteger.Zero;
				}
				else
				{
					return term.CoEfficient;
				}
			}
			set
			{
				ITerm term = Terms.FirstOrDefault(t => t.Exponent == degree);

				if (term == default(ITerm))
				{
					if (value != BigInteger.Zero)
					{
						ITerm newTerm = new Term(value, degree);
						List<ITerm> terms = _terms;
						terms.Add(newTerm);
						SetTerms(terms);
					}
				}
				else
				{
					term.CoEfficient = value;
				}
			}
		}

		#endregion

		private Polynomial() { _terms = new List<ITerm>(); }

		//public SparsePolynomial() { _terms = new List<ITerm>() { new PolyTerm(0, 0) }; Degree = 0; }

		public Polynomial(ITerm[] terms)
		{
			SetTerms(terms);
		}

		public Polynomial(BigInteger n, BigInteger polynomialBase)
		: this(n, polynomialBase, (int)Math.Truncate(BigInteger.Log(n, (double)polynomialBase) + 1))
		{
		}

		public Polynomial(BigInteger n, BigInteger polynomialBase, int forceDegree)
		{
			Degree = forceDegree;
			SetTerms(GetPolynomialTerms(n, polynomialBase, Degree));
		}

		private void SetTerms(IEnumerable<ITerm> terms)
		{
			_terms = terms.OrderBy(t => t.Exponent).ToList();
			RemoveZeros();
		}

		public void RemoveZeros()
		{
			_terms.RemoveAll(t => t.CoEfficient == 0);
			if (!_terms.Any())
			{
				_terms = Term.GetTerms(new BigInteger[] { 0 }).ToList();
			}
			SetDegree();
		}

		private void SetDegree()
		{
			if (_terms.Any())
			{
				Degree = _terms.Max(term => term.Exponent);
			}
			else
			{
				Degree = 0;
			}
		}


		private static List<ITerm> GetPolynomialTerms(BigInteger value, BigInteger polynomialBase, int degree)
		{
			int d = degree; // (int)Math.Truncate(BigInteger.Log(value, (double)polynomialBase)+ 1);
			BigInteger toAdd = value;
			List<ITerm> result = new List<ITerm>();
			while (d >= 0 && toAdd > 0)
			{
				BigInteger placeValue = BigInteger.Pow(polynomialBase, d);

				if (placeValue == 1)
				{
					result.Add(new Term(toAdd, d));
					toAdd = 0;
				}
				else if (placeValue == toAdd)
				{
					result.Add(new Term(1, d));
					toAdd -= placeValue;
				}
				else if (placeValue < BigInteger.Abs(toAdd))
				{
					BigInteger remainder = 0;
					BigInteger quotient = BigInteger.DivRem(toAdd, placeValue, out remainder);

					if (quotient > placeValue)
					{
						quotient = placeValue;
					}

					result.Add(new Term(quotient, d));
					BigInteger toSubtract = BigInteger.Multiply(quotient, placeValue);

					toAdd -= toSubtract;
				}

				d--;
			}
			return result.ToList();
		}

		public static IPolynomial FromRoots(params BigInteger[] roots)
		{
			return Polynomial.Product(
				roots.Select(
					zero => new Polynomial(
						new Term[]
						{
						new Term( 1, 1),
						new Term( BigInteger.Negate(zero), 0)
						}
					)
				)
			);
		}

		public BigInteger Evaluate(BigInteger indeterminateValue)
		{
			return Evaluate(Terms, indeterminateValue);
		}

		public double Evaluate(double indeterminateValue)
		{
			return EvaluateDouble(Terms, indeterminateValue);
		}

		public static BigInteger Evaluate(ITerm[] terms, BigInteger indeterminateValue)
		{
			BigInteger result = new BigInteger(0);
			foreach (ITerm term in terms)
			{
				BigInteger placeValue = BigInteger.Pow(indeterminateValue, term.Exponent);
				BigInteger addValue = BigInteger.Multiply(term.CoEfficient, placeValue);
				result = BigInteger.Add(result, addValue);
			}
			return result;
		}

		public static double EvaluateDouble(ITerm[] terms, double x)
		{
			double result = 0;

			int d = terms.Count() - 1;
			while (d >= 0)
			{
				double placeValue = Math.Pow(x, terms[d].Exponent);

				double addValue = (double)(terms[d].CoEfficient) * placeValue;

				result += addValue;

				d--;
			}

			return result;
		}

		public static IPolynomial GetDerivativePolynomial(IPolynomial poly)
		{
			int d = 0;
			List<ITerm> terms = new List<ITerm>();
			foreach (ITerm term in poly.Terms)
			{
				d = term.Exponent - 1;
				if (d < 0)
				{
					continue;
				}
				terms.Add(new Term(term.CoEfficient * term.Exponent, d));
			}

			IPolynomial result = new Polynomial(terms.ToArray());
			return result;
		}

		public static bool IsIrreducibleOverField(IPolynomial f, BigInteger m, BigInteger p)
		{
			IPolynomial splittingField = new Polynomial(
				new Term[] {
					new Term(  1, (int)p),
					new Term( -1, 1)
				});

			IPolynomial reducedField = Polynomial.ModMod(splittingField, f, p);
			if (!EisensteinsIrreducibilityCriterion(reducedField, p))
			{
				return false;
			}

			IPolynomial gcd = Polynomial.GCDMod(f, reducedField, m);
			return (gcd.CompareTo(Polynomial.One) == 0);
		}

		public static bool EisensteinsIrreducibilityCriterion(IPolynomial poly, BigInteger p)
		{
			List<BigInteger> coefficients = poly.Terms.Select(t => t.CoEfficient).ToList();

			BigInteger constantCoefficient = coefficients.First();
			BigInteger leadingCoefficient = coefficients.Last();
			coefficients.Remove(leadingCoefficient);

			BigInteger gcd = IntegerMath.GCD.FindGCD(coefficients);
			if (gcd == 1)
			{
				return false;
			}

			BigInteger p2 = gcd.Square();

			gcd = IntegerMath.GCD.FindGCD(gcd, leadingCoefficient);
			if (gcd != 1)
			{
				return false;
			}

			gcd = IntegerMath.GCD.FindGCD(p2, constantCoefficient);
			if (gcd == 1)
			{
				return false;
			}

			return true;
		}

		public static bool IsIrreducibleOverP(IPolynomial poly, BigInteger p)
		{
			List<BigInteger> coefficients = poly.Terms.Select(t => t.CoEfficient).ToList();

			BigInteger leadingCoefficient = coefficients.Last();
			BigInteger constantCoefficient = coefficients.First();

			coefficients.Remove(leadingCoefficient);
			coefficients.Remove(constantCoefficient);

			BigInteger leadingRemainder = -1;
			BigInteger.DivRem(leadingCoefficient, p, out leadingRemainder);

			BigInteger constantRemainder = -1;
			BigInteger.DivRem(constantCoefficient, p.Square(), out constantRemainder);

			bool result = (leadingRemainder != 0); // p does not divide leading coefficient

			result &= (constantRemainder != 0);    // p^2 does not divide constant coefficient

			coefficients.Add(p);
			result &= (IntegerMath.GCD.FindGCD(coefficients) == 1);

			return result;
		}

		public static bool IsIrreducible(IPolynomial f, BigInteger prime)
		{
			IPolynomial f_p =
				new Polynomial(new Term[]
				{
				new Term( 1, (int)prime),
				new Term( -1,  1)
				});

			IPolynomial rem = null;
			Polynomial.DivideMod(f_p, f, prime, out rem);

			BigInteger gcd = GetCommonRoot(f, rem, prime);

			bool result = (gcd == 1);
			return result;
		}

		public static BigInteger GetCommonRoot(IPolynomial left, IPolynomial right, BigInteger mod)
		{
			BigInteger counter = 0;

			while (++counter < mod)
			{
				if (left.Evaluate(counter).Mod(mod) == 0)
				{
					if (right.Evaluate(counter).Mod(mod) == 0)
					{
						return counter;
					}
				}
			}
			return BigInteger.One;
		}

		public static List<BigInteger> GetRootsMod(IPolynomial polynomial, BigInteger baseM, IEnumerable<BigInteger> modList)
		{
			BigInteger polyResult = polynomial.Evaluate(baseM);
			IEnumerable<BigInteger> result = modList.Where(mod => (polyResult % mod) == 0);
			return result.ToList();
		}

		public static void Swap(ref IPolynomial a, ref IPolynomial b)
		{
			if (b.CompareTo(a) > 0)
			{
				IPolynomial swap = b;
				b = a;
				a = swap;
			}
		}
		// GCDMod
		public static IPolynomial GCDMod(IPolynomial left, IPolynomial right, BigInteger polynomialBase)
		{
			IPolynomial a = left.Clone();
			IPolynomial b = right.Clone();

			Swap(ref a, ref b);

			while (a.Degree != b.Degree)
			{
				IPolynomial smallerA = Polynomial.ReduceDegree(a, polynomialBase);
				a = smallerA;

				Swap(ref a, ref b);
			}

			while (a.Degree != 1)
			{
				IPolynomial smallerA = Polynomial.ReduceDegree(a, polynomialBase);
				IPolynomial smallerB = Polynomial.ReduceDegree(b, polynomialBase);

				a = smallerA;
				b = smallerB;

				Swap(ref a, ref b);
			}

			while (a.Degree >= 1)
			{
				Swap(ref a, ref b);

				var bSign = b.Terms.Last().CoEfficient.Sign;
				if (bSign < 0)
				{
					break;
				}

				while (!(b.Terms.Length == 0 || b.Terms[0].CoEfficient == 0 || a.CompareTo(b) < 0))
				{
					var aSign = a.Terms.Last().CoEfficient.Sign;
					bSign = b.Terms.Last().CoEfficient.Sign;

					if (aSign < 0 || bSign < 0)
					{
						break;
					}

					a = Polynomial.Subtract(a, b);
				}

				//IPoly smallerA = SparsePolynomial.ReduceDegree(a, polynomialBase);
				//IPoly smallerB = SparsePolynomial.ReduceDegree(b, polynomialBase);

				//a=smallerA;
				//b=smallerB;
			}

			if (a.Degree == 0)
			{
				return Polynomial.One;
			}
			else
			{
				return a;
			}
		}

		// ExtendedGCD
		public static IPolynomial ExtendedGCD(IPolynomial left, IPolynomial right, BigInteger mod)
		{
			IPolynomial rem = Polynomial.Two;
			IPolynomial a = left.Clone();
			IPolynomial b = right.Clone();
			IPolynomial c = Polynomial.Zero;


			while (c.CompareTo(Polynomial.Zero) != 0 && rem.CompareTo(Polynomial.Zero) != 0 && rem.CompareTo(Polynomial.One) != 0)
			{
				c = Polynomial.Divide(a, b, out rem);

				a = b;
				b = rem;
			}

			if (rem.CompareTo(Polynomial.Zero) != 0 || rem.CompareTo(Polynomial.One) != 0)
			{
				return Polynomial.One;
			}

			return rem;
		}

		public static IPolynomial GCD(IPolynomial left, IPolynomial right)
		{
			IPolynomial a = left.Clone();
			IPolynomial b = right.Clone();

			if (b.Degree > a.Degree)
			{
				IPolynomial swap = b;
				b = a;
				a = swap;
			}

			while (!(b.Terms.Length == 0 || b.Terms[0].CoEfficient == 0))
			{
				IPolynomial temp = a;
				a = b;
				b = Polynomial.Mod(temp, b);
			}

			if (a.Degree == 0)
			{
				return Polynomial.One;
			}
			else
			{
				return a;
			}
		}

		public static IPolynomial GCD(IPolynomial left, IPolynomial right, BigInteger modulus)
		{
			IPolynomial a = left.Clone();
			IPolynomial b = right.Clone();

			if (b.Degree > a.Degree)
			{
				IPolynomial swap = b;
				b = a;
				a = swap;
			}

			while (!(b.Terms.Length == 0 || b.Terms[0].CoEfficient == 0))
			{
				IPolynomial temp = a;
				a = b;
				b = Polynomial.ModMod(temp, b, modulus);
			}

			if (a.Degree == 0)
			{
				return Polynomial.One;
			}
			else
			{
				return a;
			}
		}

		public static IPolynomial ModularInverse(IPolynomial poly, BigInteger mod)
		{
			return new Polynomial(Term.GetTerms(poly.Terms.Select(trm => (mod - trm.CoEfficient).Mod(mod)).ToArray()));
		}

		public static IPolynomial ModMod(IPolynomial toReduce, IPolynomial modPoly, BigInteger modPrime)
		{
			return Polynomial.Modulus(Polynomial.Mod(toReduce, modPoly), modPrime);
		}

		public static IPolynomial Mod(IPolynomial poly, IPolynomial mod)
		{
			int sortOrder = mod.CompareTo(poly);
			if (sortOrder > 0)
			{
				return poly;
			}
			else if (sortOrder == 0)
			{
				return Polynomial.Zero;
			}

			IPolynomial remainder = Polynomial.Zero;
			Divide(poly, mod, out remainder);

			return remainder;
		}

		public static IPolynomial Modulus(IPolynomial poly, BigInteger mod)
		{
			IPolynomial clone = poly.Clone();
			List<ITerm> terms = new List<ITerm>();

			foreach (ITerm term in clone.Terms)
			{
				BigInteger remainder = 0;
				BigInteger.DivRem(term.CoEfficient, mod, out remainder);

				if (remainder.Sign == -1)
				{
					remainder = (remainder + mod);
				}

				terms.Add(new Term(remainder, term.Exponent));
			}

			// Recalculate the degree
			ITerm[] termArray = terms.SkipWhile(t => t.CoEfficient.Sign == 0).ToArray();
			IPolynomial result = new Polynomial(termArray);
			return result;
		}

		public static IPolynomial Divide(IPolynomial left, IPolynomial right)
		{
			IPolynomial remainder = Polynomial.Zero;
			return Polynomial.Divide(left, right, out remainder);
		}

		public static IPolynomial Divide(IPolynomial left, IPolynomial right, out IPolynomial remainder)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));
			if (right.Degree > left.Degree || right.CompareTo(left) == 1)
			{
				remainder = Polynomial.Zero; return left;
			}

			int rightDegree = right.Degree;
			int quotientDegree = (left.Degree - rightDegree) + 1;
			BigInteger leadingCoefficent = new BigInteger(right[rightDegree].ToByteArray());

			IPolynomial rem = left.Clone();
			IPolynomial quotient = Polynomial.Zero;

			// The leading coefficient is the only number we ever divide by
			// (so if right is monic, polynomial division does not involve division at all!)
			for (int i = quotientDegree - 1; i >= 0; i--)
			{
				quotient[i] = BigInteger.Divide(rem[rightDegree + i], leadingCoefficent);
				rem[rightDegree + i] = BigInteger.Zero;

				for (int j = rightDegree + i - 1; j >= i; j--)
				{
					rem[j] = BigInteger.Subtract(rem[j], BigInteger.Multiply(quotient[i], right[j - i]));
				}
			}

			// Remove zeros
			rem.RemoveZeros();
			quotient.RemoveZeros();

			remainder = rem;
			return quotient;
		}

		public static IPolynomial DivideMod(IPolynomial left, IPolynomial right, BigInteger mod, out IPolynomial remainder)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));
			if (right.Degree > left.Degree || right.CompareTo(left) == 1)
			{
				remainder = Polynomial.Zero; return left;
			}

			int rightDegree = right.Degree;
			int quotientDegree = (left.Degree - rightDegree) + 1;
			BigInteger leadingCoefficent = new BigInteger(right[rightDegree].ToByteArray()).Mod(mod);

			IPolynomial rem = left.Clone();
			IPolynomial quotient = Polynomial.Zero;

			// The leading coefficient is the only number we ever divide by
			// (so if right is monic, polynomial division does not involve division at all!)
			for (int i = quotientDegree - 1; i >= 0; i--)
			{
				quotient[i] = BigInteger.Divide(rem[rightDegree + i], leadingCoefficent).Mod(mod);
				rem[rightDegree + i] = BigInteger.Zero;

				for (int j = rightDegree + i - 1; j >= i; j--)
				{
					rem[j] = BigInteger.Subtract(rem[j], BigInteger.Multiply(quotient[i], right[j - i]).Mod(mod)).Mod(mod);
				}
			}

			// Remove zeros
			rem.RemoveZeros();
			quotient.RemoveZeros();

			remainder = rem;
			return quotient;
		}

		public static IPolynomial Multiply(IPolynomial left, IPolynomial right)
		{
			if (left == null) { throw new ArgumentNullException(nameof(left)); }
			if (right == null) { throw new ArgumentNullException(nameof(right)); }

			BigInteger[] terms = new BigInteger[left.Degree + right.Degree + 1];

			for (int i = 0; i <= left.Degree; i++)
			{
				for (int j = 0; j <= right.Degree; j++)
				{
					terms[(i + j)] += BigInteger.Multiply(left[i], right[j]);
				}
			}

			ITerm[] newTerms = Term.GetTerms(terms);
			return new Polynomial(newTerms);
		}

		public static IPolynomial MultiplyMod(IPolynomial poly, BigInteger multiplier, BigInteger mod)
		{
			IPolynomial result = poly.Clone();

			foreach (ITerm term in result.Terms)
			{
				BigInteger newCoefficient = term.CoEfficient;
				if (newCoefficient != 0)
				{
					newCoefficient = (newCoefficient * multiplier);
					term.CoEfficient = (newCoefficient.Mod(mod));
				}
			}

			return result;
		}

		public static IPolynomial PowMod(IPolynomial poly, BigInteger exp, BigInteger mod)
		{
			IPolynomial result = poly.Clone();

			foreach (ITerm term in result.Terms)
			{
				BigInteger newCoefficient = term.CoEfficient;
				if (newCoefficient != 0)
				{
					newCoefficient = BigInteger.ModPow(newCoefficient, exp, mod);
					if (newCoefficient.Sign == -1)
					{
						throw new Exception("BigInteger.ModPow returned negative number");
					}
					term.CoEfficient = newCoefficient;
				}
			}

			return result;
		}

		public static IPolynomial Product(params IPolynomial[] polys)
		{
			return Product(polys.ToList());
		}

		public static IPolynomial Product(IEnumerable<IPolynomial> polys)
		{
			IPolynomial result = null;

			foreach (IPolynomial p in polys)
			{
				if (result == null)
				{
					result = p;
				}
				else
				{
					result = Polynomial.Multiply(result, p);
				}
			}

			return result;
		}

		public static IPolynomial Square(IPolynomial poly)
		{
			return Polynomial.Multiply(poly, poly);
		}

		public static IPolynomial Pow(IPolynomial poly, int exponent)
		{
			if (exponent < 0)
			{
				throw new NotImplementedException("Raising a polynomial to a negative exponent not supported. Build this functionality if it is needed.");
			}
			else if (exponent == 0)
			{
				return new Polynomial(new Term[] { new Term(1, 0) });
			}
			else if (exponent == 1)
			{
				return poly.Clone();
			}
			else if (exponent == 2)
			{
				return Square(poly);
			}

			IPolynomial total = Polynomial.Square(poly);

			int counter = exponent - 2;
			while (counter != 0)
			{
				total = Polynomial.Multiply(total, poly);
				counter -= 1;
			}

			return total;
		}

		public static IPolynomial ExponentiateMod(IPolynomial startPoly, BigInteger s2, IPolynomial f, BigInteger p)
		{
			IPolynomial result = Polynomial.One;
			if (s2 == 0) { return result; }

			IPolynomial A = startPoly.Clone();

			byte[] byteArray = s2.ToByteArray();
			bool[] bitArray = new BitArray(byteArray).Cast<bool>().ToArray();

			// Remove trailing zeros ?
			if (bitArray[0] == true)
			{
				result = startPoly;
			}

			int i = 1;
			int t = bitArray.Length;
			while (i < t)
			{
				A = Polynomial.ModMod(Polynomial.Square(A), f, p);
				if (bitArray[i] == true)
				{
					result = Polynomial.ModMod(Polynomial.Multiply(A, result), f, p);
				}
				i++;
			}

			return result;
		}

		public static IPolynomial ModPow(IPolynomial poly, BigInteger exponent, IPolynomial modulus)
		{
			if (exponent < 0)
			{
				throw new NotImplementedException("Raising a polynomial to a negative exponent not supported. Build this functionality if it is needed.");
			}
			else if (exponent == 0)
			{
				return Polynomial.One;
			}
			else if (exponent == 1)
			{
				return poly.Clone();
			}
			else if (exponent == 2)
			{
				return Polynomial.Square(poly);
			}

			IPolynomial total = Polynomial.Square(poly);

			BigInteger counter = exponent - 2;
			while (counter != 0)
			{
				total = Multiply(poly, total);

				if (total.CompareTo(modulus) < 0)
				{
					total = Polynomial.Mod(total, modulus);
				}

				counter -= 1;
			}

			return total;
		}

		public static IPolynomial Subtract(IPolynomial left, IPolynomial right)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			BigInteger[] terms = new BigInteger[Math.Min(left.Degree, right.Degree) + 1];
			for (int i = 0; i < terms.Length; i++)
			{
				BigInteger l = left[i];
				BigInteger r = right[i];
				terms[i] = (l - r);
			}

			IPolynomial result = new Polynomial(Term.GetTerms(terms.ToArray()));
			return result;
		}

		public static IPolynomial Sum(params IPolynomial[] polys)
		{
			return Sum(polys.ToList());
		}

		public static IPolynomial Sum(IEnumerable<IPolynomial> polys)
		{
			IPolynomial result = null;

			foreach (IPolynomial p in polys)
			{
				if (result == null)
				{
					result = p;
				}
				else
				{
					result = Polynomial.Add(result, p);
				}
			}

			return result;
		}

		public static IPolynomial Add(IPolynomial left, IPolynomial right)
		{
			if (left == null) throw new ArgumentNullException(nameof(left));
			if (right == null) throw new ArgumentNullException(nameof(right));

			BigInteger[] terms = new BigInteger[Math.Max(left.Degree, right.Degree) + 1];
			for (int i = 0; i < terms.Length; i++)
			{
				BigInteger l = left[i];
				BigInteger r = right[i];
				BigInteger ttl = (l + r);

				terms[i] = ttl;
			}

			IPolynomial result = new Polynomial(Term.GetTerms(terms.ToArray()));
			return result;
		}

		public static IPolynomial MakeMonic(IPolynomial polynomial, BigInteger polynomialBase)
		{
			int deg = polynomial.Degree;
			IPolynomial result = new Polynomial(polynomial.Terms.ToArray());
			if (BigInteger.Abs(result.Terms[deg].CoEfficient) > 1)
			{
				BigInteger toAdd = (result.Terms[deg].CoEfficient - 1) * polynomialBase;
				result.Terms[deg].CoEfficient = 1;
				result.Terms[deg - 1].CoEfficient += toAdd;
			}
			return result;
		}

		public static IPolynomial ReduceDegree(IPolynomial polynomial, BigInteger polynomialBase)
		{
			List<BigInteger> coefficients = polynomial.Terms.Select(t => t.CoEfficient).ToList();
			BigInteger leadingCoefficient = coefficients.Last();
			coefficients.Remove(leadingCoefficient);

			BigInteger toAdd = (leadingCoefficient * polynomialBase);

			leadingCoefficient = coefficients.Last();

			BigInteger newLeadingCoefficient = leadingCoefficient + toAdd;

			coefficients.Remove(leadingCoefficient);
			coefficients.Add(newLeadingCoefficient);

			return new Polynomial(Term.GetTerms(coefficients.ToArray()));
		}

		public static void MakeCoefficientsSmaller(IPolynomial polynomial, BigInteger polynomialBase, BigInteger maxCoefficientSize = default(BigInteger))
		{
			BigInteger maxSize = maxCoefficientSize;

			if (maxSize == default(BigInteger))
			{
				maxSize = polynomialBase;
			}

			int pos = 0;
			int deg = polynomial.Degree;

			while (pos < deg)
			{
				if (pos + 1 > deg)
				{
					return;
				}

				if (polynomial[pos] > maxSize &&
					polynomial[pos] > polynomial[pos + 1])
				{
					BigInteger diff = polynomial[pos] - maxSize;

					BigInteger toAdd = (diff / polynomialBase) + 1;
					BigInteger toRemove = toAdd * polynomialBase;

					polynomial[pos] -= toRemove;
					polynomial[pos + 1] += toAdd;
				}

				pos++;
			}
		}

		public static IPolynomial Parse(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) { throw new ArgumentException(); }

			string inputString = input.Replace(" ", "").Replace("-", "+-");
			string[] stringTerms = inputString.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

			if (!stringTerms.Any()) { throw new FormatException(); }

			List<Term> polyTerms = new List<Term>();
			foreach (string stringTerm in stringTerms)
			{
				string[] termParts = stringTerm.Split(new char[] { '*' });

				if (termParts.Count() != 2)
				{
					if (termParts.Count() != 1) { throw new FormatException(); }

					string temp = termParts[0];
					if (temp.All(c => char.IsDigit(c) || c == '-'))
					{
						termParts = new string[] { temp, "X^0" };
					}
					else if (temp.All(c => char.IsLetter(c) || c == '^' || c == '-' || char.IsDigit(c)))
					{
						if (temp.Contains("-"))
						{
							temp = temp.Replace("-", "");
							termParts = new string[] { "-1", temp };
						}
						else
						{
							termParts = new string[] { "1", temp };
						}
					}
					else { throw new FormatException(); }
				}

				BigInteger coefficient = BigInteger.Parse(termParts[0]);

				string[] variableParts = termParts[1].Split(new char[] { '^' });
				if (variableParts.Count() != 2)
				{
					if (variableParts.Count() != 1) { throw new FormatException(); }

					string tmp = variableParts[0];
					if (tmp.All(c => char.IsLetter(c)))
					{
						variableParts = new string[] { tmp, "1" };
					}
				}

				int exponent = int.Parse(variableParts[1]);

				polyTerms.Add(new Term(coefficient, exponent));
			}

			if (!polyTerms.Any()) { throw new FormatException(); }
			return new Polynomial(polyTerms.ToArray());
		}

		public IPolynomial Clone()
		{
			return new Polynomial(Terms.Select(pt => pt.Clone()).ToArray());
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.ToString();
		}

		public override string ToString()
		{
			return Polynomial.FormatString(this);
		}

		public static string FormatString(IPolynomial polynomial)
		{
			List<string> stringTerms = new List<string>();
			int degree = polynomial.Terms.Length;
			while (--degree >= 0)
			{
				ITerm term = polynomial.Terms[degree];

				if (term.CoEfficient == 0)
				{
					if (term.Exponent == 0)
					{
						if (stringTerms.Count == 0) { stringTerms.Add("0"); }
					}
					continue;
				}

				switch (term.Exponent)
				{
					case 0:
						stringTerms.Add($"{term.CoEfficient}");
						break;

					case 1:
						if (term.CoEfficient == 1) stringTerms.Add("X");
						else if (term.CoEfficient == -1) stringTerms.Add("-X");
						else stringTerms.Add($"{term.CoEfficient}*X");
						break;

					default:
						if (term.CoEfficient == 1) stringTerms.Add($"X^{term.Exponent}");
						else if (term.CoEfficient == -1) stringTerms.Add($"-X^{term.Exponent}");
						else stringTerms.Add($"{term.CoEfficient}*X^{term.Exponent}");
						break;
				}
			}
			return string.Join(" + ", stringTerms).Replace("+ -", "- ");
		}

		public bool Equals(IPolynomial other)
		{
			return (this.CompareTo(other) == 0);
		}

		public bool Equals(Polynomial other)
		{
			return (this.CompareTo(other) == 0);
		}

		public bool Equals(IPolynomial x, IPolynomial y)
		{
			return (x.CompareTo(y) == 0);
		}

		public bool Equals(Polynomial x, Polynomial y)
		{
			return (x.CompareTo(y) == 0);
		}

		public int GetHashCode(IPolynomial obj)
		{
			return obj.ToString().GetHashCode();
		}

		public int GetHashCode(Polynomial obj)
		{
			IPolynomial poly = obj as IPolynomial;
			return poly.GetHashCode(poly);
		}

		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				throw new NullReferenceException();
			}

			IPolynomial other = obj as IPolynomial;

			if (other == null)
			{
				throw new ArgumentException();
			}

			return this.CompareTo(other);
		}

		public int CompareTo(IPolynomial other)
		{
			if (other == null)
			{
				throw new ArgumentException();
			}

			if (other.Degree != this.Degree)
			{
				if (other.Degree > this.Degree)
				{
					return -1;
				}
				else
				{
					return 1;
				}
			}
			else
			{
				int counter = this.Degree;

				while (counter >= 0)
				{
					BigInteger thisCoefficient = this[counter];
					BigInteger otherCoefficient = other[counter];

					if (thisCoefficient < otherCoefficient)
					{
						return -1;
					}
					else if (thisCoefficient > otherCoefficient)
					{
						return 1;
					}

					counter--;
				}

				return 0;
			}
		}

		#region IXmlSerializable

		private static XmlSerializer TermXmlSerializer = new XmlSerializer(typeof(Term));
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("Degree", Degree.ToString());
			writer.WriteStartElement("Terms");

			foreach (Term term in Terms)
			{
				TermXmlSerializer.Serialize(writer, term);
			}

			writer.WriteEndElement();
			writer.WriteElementString("PolynomialString", this.ToString());
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();

			Degree = int.Parse(reader.ReadElementString("Degree"));
			reader.ReadStartElement("Terms");

			List<Term> terms = new List<Term>();
			do
			{
				Term term = (Term)Serializer.Deserialize(reader, typeof(Term));
				terms.Add(term);
			}
			while (reader.ReadToNextSibling("Term"));

			reader.ReadEndElement();

			_terms = terms.Select(trm => (ITerm)trm).ToList();
			if (_terms.Count - 1 != Degree)
			{
				throw new Exception("Element Degree does not agree with number of terms. Degree should equal #terms - 1.");
			}
		}

		public XmlSchema GetSchema() { return null; }

		#endregion
	}
}