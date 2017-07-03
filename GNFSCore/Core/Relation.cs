using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace GNFSCore
{
	using FactorBase;
	using PrimeSignature;

	public class Relation : IXmlSerializable
	{
		public int A { get; set; }
		public int B { get; set; }
		public BigInteger C;
		public BigInteger AlgebraicNorm { get; set; }
		public BigInteger RationalNorm { get; set; }

		[NonSerialized]
		private GNFS _gnfs;
		internal BigInteger AlgebraicQuotient { get; set; }
		internal BigInteger RationalQuotient { get; set; }

		public bool IsSmooth
		{
			get
			{
				return BigInteger.Abs(AlgebraicQuotient) == 1 && BigInteger.Abs(RationalQuotient) == 1;
			}
		}

		public Tuple<BigInteger, BigInteger> GetRoughRemainders()
		{
			return new Tuple<BigInteger, BigInteger>(AlgebraicQuotient, RationalQuotient);
		}

		public Relation() { }

		public Relation(GNFS gnfs, int a, int b)
		{
			A = a;
			B = b;
			_gnfs = gnfs;

			AlgebraicNorm = Normal.Algebraic(A, B, _gnfs.CurrentPolynomial); // b^deg * f( a/b )
			RationalNorm = Normal.Rational(A, B, _gnfs.CurrentPolynomial.Base); // a + bm

			AlgebraicQuotient = AlgebraicNorm;
			RationalQuotient = RationalNorm;

			BigInteger rationalEval = _gnfs.CurrentPolynomial.Evaluate(RationalNorm);
			C = rationalEval % _gnfs.N;
		}

		public void Sieve()
		{
			AlgebraicQuotient = Factor(_gnfs.AlgebraicPrimeBase, AlgebraicNorm, AlgebraicQuotient);
			RationalQuotient = Factor(_gnfs.RationalPrimeBase, RationalNorm, RationalQuotient);
		}

		private static BigInteger Factor(IEnumerable<BigInteger> factors, BigInteger norm, BigInteger quotient)
		{
			BigInteger sqrt = BigInteger.Abs(norm).SquareRoot();

			BigInteger result = quotient;
			foreach (BigInteger factor in factors)
			{
				if (result == 0 || result == -1 || result == 1 || factor > sqrt)
				{
					break;
				}
				while (result % factor == 0 && result != 1 && result != -1)
				{
					result /= factor;

					BigInteger absResult = BigInteger.Abs(result);
					if (absResult > 1 /*&& absResult < int.MaxValue - 1*/)
					{
						//int intValue = (int)absResult;
						if (factors.Contains(absResult))
						{
							result = 1;
						}
					}
				}
			}
			return result;
		}

		public Tuple<BitVector, BitVector> GetMatrixRowVector()
		{
			BitVector rationalBitVector = new BitVector(RationalNorm, _gnfs.RationalFactorBase);
			BitVector algebraicBitVector = new BitVector(AlgebraicNorm, _gnfs.AlgebraicFactorBase);
			//bool[] quadraticBitVector = QuadraticResidue.GetQuadraticCharacters(this, _gnfs.QFB);
			//List<bool> combinedVector = new List<bool>();
			//combinedVector.AddRange(rationalBitVector.Elements);
			//combinedVector.AddRange(algebraicBitVector.Elements);
			//combinedVector.AddRange(quadraticBitVector);
			//return new BitVector(RationalNorm, combinedVector.ToArray());
			return new Tuple<BitVector, BitVector>(rationalBitVector, algebraicBitVector);
		}

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t" +
				$"[ƒ(b) ≡ 0 mod a:{AlgebraicNorm.ToString().PadLeft(10)} ({AlgebraicNorm.IsSquare()}),\ta+b*m={RationalNorm.ToString().PadLeft(4)} ({RationalNorm.IsSquare()})]\t" +
				$"ƒ({RationalNorm}) =".PadRight(8) + $"{C.ToString().PadLeft(6)}";
		}

		public static List<Relation> LoadRelations(string saveDirectory)
		{
			List<Relation> result = new List<Relation>();
			// Load Relations
			if (Directory.Exists(saveDirectory))
			{
				IEnumerable<string> relationFiles = Directory.EnumerateFiles(saveDirectory, "*.relation");
				if (relationFiles.Any())
				{
					foreach (string file in relationFiles)
					{
						Relation relation = (Relation)Serializer.Deserialize(file, typeof(Relation));
						result.Add(relation);
					}
				}
			}
			return result;
		}

		public static void Serialize(string filePath, Relation relation)
		{
			Serializer.Serialize($"{filePath}\\{relation.A}_{relation.B}.relation", relation);
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("A", A.ToString());
			writer.WriteElementString("B", B.ToString());
			writer.WriteElementString("C", C.ToString());
			writer.WriteElementString("AlgebraicNorm", AlgebraicNorm.ToString());
			writer.WriteElementString("RationalNorm", RationalNorm.ToString());
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			reader.ReadStartElement();
			A = int.Parse(reader.ReadElementString("A"));
			B = int.Parse(reader.ReadElementString("B"));
			C = BigInteger.Parse(reader.ReadElementString("C"));
			AlgebraicNorm = BigInteger.Parse(reader.ReadElementString("AlgebraicNorm"));
			RationalNorm = BigInteger.Parse(reader.ReadElementString("RationalNorm"));
			reader.ReadEndElement();
		}

		public XmlSchema GetSchema() { return null; }
	}
}
