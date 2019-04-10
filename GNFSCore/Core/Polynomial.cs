using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace GNFSCore
{
	using Interfaces;

	[DataContract]
	public class Polynomial : IPolynomial
	{
		public static IPolynomial Zero = new Polynomial(Term.GetTerms(new BigInteger[] { 0 }));
		public static IPolynomial One = new Polynomial(Term.GetTerms(new BigInteger[] { 1 }));
		public static IPolynomial Two = new Polynomial(Term.GetTerms(new BigInteger[] { 2 }));

		public int Degree { get; private set; }

		public ITerm[] Terms { get { return _terms.ToArray(); } }

		[DataMember(Name = "Terms")]
		private TermCollection _terms = new TermCollection();

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
						TermCollection terms = _terms;
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

		internal Polynomial() { _terms = new TermCollection(); }

		public Polynomial(Term[] terms)
			: this(terms.Select(t => (ITerm)t).ToArray())
		{
		}

		public Polynomial(ITerm[] terms)
		{
			SetTerms(terms);
		}

		public Polynomial(BigInteger n, BigInteger polynomialBase, int forceDegree)
		{
			Degree = forceDegree;
			SetTerms(GetPolynomialTerms(n, polynomialBase, Degree));
		}

		private void SetTerms(IEnumerable<ITerm> terms)
		{
			_terms = new TermCollection(terms.OrderBy(t => t.Exponent).ToList());
			RemoveZeros();
		}

		public void RemoveZeros()
		{
			_terms.RemoveAll(t => t.CoEfficient == 0);
			if (!_terms.Any())
			{
				_terms = new TermCollection(Term.GetTerms(new BigInteger[] { 0 }).ToList());
			}
			SetDegree();
		}

		internal void SetDegree()
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
		
		private static TermCollection GetPolynomialTerms(BigInteger value, BigInteger polynomialBase, int degree)
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
			return new TermCollection(result.ToList());
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
		
		public static List<BigInteger> GetRootsMod(IPolynomial polynomial, BigInteger baseM, IEnumerable<BigInteger> modList)
		{
			BigInteger polyResult = polynomial.Evaluate(baseM);
			IEnumerable<BigInteger> result = modList.Where(mod => (polyResult % mod) == 0);
			return result.ToList();
		}
		
		// ExtendedGCD
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

		public static IPolynomial MakeMonic(IPolynomial polynomial, BigInteger polynomialBase)
		{
			int deg = polynomial.Degree;
			IPolynomial result = new Polynomial(polynomial.Terms.ToArray());
			if (BigInteger.Abs(result.Terms[deg].CoEfficient) > 1)
			{
				BigInteger toAdd = (result.Terms[deg].CoEfficient - 1) * polynomialBase;
				result[deg] = 1;
				result[deg - 1] += toAdd;
			}
			return result;
		}

		public IPolynomial Clone()
		{
			return new Polynomial(Terms.Select(pt => pt.Clone()).ToArray());
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
	}
}