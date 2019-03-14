using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GNFSCore
{
	using Factors;
	using Interfaces;
	using IntegerMath;

	[DataContract]
	public partial class GNFS
	{
		#region Properties

		[DataMember]
		public BigInteger N { get; set; }
		[DataMember]
		public int PolynomialDegree { get; private set; }
		[DataMember]
		public BigInteger PolynomialBase { get; private set; }

		[JsonProperty(ItemConverterType = typeof(Serialization.PolynomialConverter))]
		public List<IPolynomial> PolynomialCollection { get; set; }
		public IPolynomial CurrentPolynomial { get; private set; }

		[DataMember]
		public PolyRelationsSieveProgress CurrentRelationsProgress { get; set; }

		public CancellationToken CancelToken { get; set; }

		[DataMember]
		public FactorBase PrimeFactorBase { get; set; }

		/// <summary>
		/// Array of (p, m % p)
		/// </summary>
		public FactorPairCollection RationalFactorPairCollection { get; set; }

		/// <summary>
		/// Array of (p, r) where ƒ(r) % p == 0
		/// </summary>
		public FactorPairCollection AlgebraicFactorPairCollection { get; set; }
		public FactorPairCollection QuadradicFactorPairCollection { get; set; }

		public DirectoryLocations SaveLocations { get; private set; }

		#endregion

		#region Constructors 

		public GNFS()
		{
			PrimeFactorBase = new FactorBase();
			PolynomialCollection = new List<IPolynomial>();
			RationalFactorPairCollection = new FactorPairCollection();
			AlgebraicFactorPairCollection = new FactorPairCollection();
			QuadradicFactorPairCollection = new FactorPairCollection();
		}

		public GNFS(CancellationToken cancelToken, BigInteger n, BigInteger polynomialBase, int polyDegree, BigInteger primeBound, int relationQuantity, int relationValueRange)
			: this()
		{
			CancelToken = cancelToken;
			N = n;

			SaveLocations = new DirectoryLocations(N, polynomialBase, polyDegree);

			if (!Directory.Exists(SaveLocations.SaveDirectory))
			{
				// New GNFS instance
				Directory.CreateDirectory(SaveLocations.SaveDirectory);

				if (polyDegree == -1)
				{
					this.PolynomialDegree = CalculateDegree(n);
				}
				else
				{
					this.PolynomialDegree = polyDegree;
				}

				this.PolynomialBase = polynomialBase;

				if (CancelToken.IsCancellationRequested) { return; }

				CaclulatePrimeBounds(primeBound);

				if (CancelToken.IsCancellationRequested) { return; }

				ConstructNewPolynomial(this.PolynomialBase, this.PolynomialDegree);

				NewFactorBases();

				if (CancelToken.IsCancellationRequested) { return; }

				CurrentRelationsProgress = new PolyRelationsSieveProgress(this, relationQuantity, relationValueRange);

				//SaveGnfsProgress();
			}
		}

		#endregion

		public bool IsFactor(BigInteger toCheck)
		{
			return ((N % toCheck) == 0);
		}


		#region New Factorization

		// Values were obtained from the paper:
		// "Polynomial Selection for the Number Field Sieve Integer factorization Algorithm" - by Brian Antony Murphy
		// Table 3.1, page 44
		private static int CalculateDegree(BigInteger n)
		{
			int result = 2;
			int base10 = n.ToString().Length;

			if (base10 < 65)
			{
				result = 3;
			}
			else if (base10 >= 65 && base10 < 125)
			{
				result = 4;
			}
			else if (base10 >= 125 && base10 < 225)
			{
				result = 5;
			}
			else if (base10 >= 225 && base10 < 315)
			{
				result = 6;
			}
			else if (base10 >= 315)
			{
				result = 7;
			}

			return result;
		}

		private void CaclulatePrimeBounds()
		{
			BigInteger bound = new BigInteger();

			int base10 = N.ToString().Length; //N.NthRoot(10, ref remainder);
			if (base10 <= 10)
			{
				bound = 100;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 <= 18)
			{
				bound = base10 * 1000;//(int)((int)N.NthRoot(_degree, ref remainder) * 1.5); // 60;
			}
			else if (base10 <= 100)
			{
				bound = 100000;
			}
			else if (base10 > 100 && base10 <= 150)
			{
				bound = 250000;
			}
			else if (base10 > 150 && base10 <= 200)
			{
				bound = 125000000;
			}
			else if (base10 > 200)
			{
				bound = 250000000;
			}

			CaclulatePrimeBounds(bound);
		}

		private void CaclulatePrimeBounds(BigInteger bound)
		{
			PrimeFactorBase = new FactorBase();

			PrimeFactorBase.RationalFactorBaseMax = bound;
			PrimeFactorBase.AlgebraicFactorBaseMax = (PrimeFactorBase.RationalFactorBaseMax) * 3;

			PrimeFactorBase.QuadraticBaseCount = CalculateQuadraticBaseSize(PolynomialDegree);

			PrimeFactorBase.QuadraticFactorBaseMin = PrimeFactorBase.AlgebraicFactorBaseMax + 20;
			PrimeFactorBase.QuadraticFactorBaseMax = PrimeFactory.GetApproximateValueFromIndex((UInt64)(PrimeFactorBase.QuadraticFactorBaseMin + PrimeFactorBase.QuadraticBaseCount));

			PrimeFactory.IncreaseMaxValue(PrimeFactorBase.QuadraticFactorBaseMax);

			PrimeFactorBase.RationalFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.RationalFactorBaseMax).ToList();
			PrimeFactorBase.AlgebraicFactorBase = PrimeFactory.GetPrimesTo(PrimeFactorBase.AlgebraicFactorBaseMax).ToList();
			PrimeFactorBase.QuadraticFactorBase = PrimeFactory.GetPrimesFrom(PrimeFactorBase.QuadraticFactorBaseMin).Take(PrimeFactorBase.QuadraticBaseCount).ToList();
		}

		private static int CalculateQuadraticBaseSize(int polyDegree)
		{
			int result = -1;

			if (polyDegree <= 3)
			{
				result = 10;
			}
			else if (polyDegree == 4)
			{
				result = 20;
			}
			else if (polyDegree == 5 || polyDegree == 6)
			{
				result = 40;
			}
			else if (polyDegree == 7)
			{
				result = 80;
			}
			else if (polyDegree >= 8)
			{
				result = 100;
			}

			return result;
		}

		private void ConstructNewPolynomial(BigInteger polynomialBase, int polyDegree)
		{
			CurrentPolynomial = new Polynomial(N, polynomialBase, polyDegree);
			PolynomialCollection.Add(CurrentPolynomial);
			//SavePolynomial(CurrentPolynomial, polynomialBase);
		}

		private void NewFactorBases()
		{
			if (!RationalFactorPairCollection.Any())
			{
				RationalFactorPairCollection = FactorPairCollection.Factory.BuildRationalFactorBase(this);
			}

			if (CancelToken.IsCancellationRequested) { return; }

			if (!AlgebraicFactorPairCollection.Any())
			{
				AlgebraicFactorPairCollection = FactorPairCollection.Factory.BuildAlgebraicFactorBase(this);
			}

			if (CancelToken.IsCancellationRequested) { return; }

			if (!QuadradicFactorPairCollection.Any())
			{
				QuadradicFactorPairCollection = FactorPairCollection.Factory.BuildQuadradicFactorBase(this);
			}

			if (CancelToken.IsCancellationRequested) { return; }
		}

		#endregion

		public static List<Relation[]> GroupRoughNumbers(List<Relation> roughNumbers)
		{
			IEnumerable<Relation> input1 = roughNumbers.OrderBy(rp => rp.AlgebraicQuotient).ThenBy(rp => rp.RationalQuotient);
			//IEnumerable<Relation> input2 = roughNumbers.OrderBy(rp => rp.RationalQuotient).ThenBy(rp => rp.AlgebraicQuotient);

			Relation lastItem = null;
			List<Relation[]> results = new List<Relation[]>();
			foreach (Relation pair in input1)
			{
				if (lastItem == null)
				{
					lastItem = pair;
				}
				else if (pair.AlgebraicQuotient == lastItem.AlgebraicQuotient && pair.RationalQuotient == lastItem.RationalQuotient)
				{
					results.Add(new Relation[] { pair, lastItem });
					lastItem = null;
				}
				else
				{
					lastItem = pair;
				}
			}

			return results;
		}

		public static List<Relation> MultiplyLikeRoughNumbers(GNFS gnfs, List<Relation[]> likeRoughNumbersGroups)
		{
			List<Relation> result = new List<Relation>();

			foreach (Relation[] likePair in likeRoughNumbersGroups)
			{
				var As = likePair.Select(lp => lp.A).ToList();
				var Bs = likePair.Select(lp => lp.B).ToList();

				int a = (As[0] + Bs[0]) * (As[0] - Bs[0]);//(int)Math.Round(Math.Sqrt(As.Sum()));
				int b = (As[1] + Bs[1]) * (As[1] - Bs[1]);//(int)Math.Round(Math.Sqrt(Bs.Sum()));

				if (a > 0 && b > 0)
				{
					result.Add(new Relation(gnfs, a, b));
				}
			}

			return result;
		}

		#region ToString

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine($"N = {N}");
			result.AppendLine();
			result.AppendLine($"Polynomial(degree: {PolynomialDegree}, base: {PolynomialBase}):");
			result.AppendLine("ƒ(m) = " + CurrentPolynomial.ToString());
			result.AppendLine();
			result.AppendLine("Prime Factor Base Bounds:");
			result.AppendLine($"RationalFactorBase : {PrimeFactorBase.RationalFactorBaseMax}");
			result.AppendLine($"AlgebraicFactorBase: {PrimeFactorBase.AlgebraicFactorBaseMax}");
			result.AppendLine($"QuadraticPrimeBase Range: {PrimeFactorBase.QuadraticFactorBaseMin} - {PrimeFactorBase.QuadraticFactorBaseMax}");
			result.AppendLine($"QuadraticPrimeBase Count: {PrimeFactorBase.QuadraticFactorBase.Count}");
			result.AppendLine();
			result.AppendLine($"RFB - Rational Factor Base - Count: {RationalFactorPairCollection.Count} - Array of (p, m % p) with prime p");
			result.AppendLine(RationalFactorPairCollection.ToString(200));
			result.AppendLine();
			result.AppendLine($"AFB - Algebraic Factor Base - Count: {AlgebraicFactorPairCollection.Count} - Array of (p, r) such that ƒ(r) ≡ 0 (mod p) and p is prime");
			result.AppendLine(AlgebraicFactorPairCollection.ToString(200));
			result.AppendLine();
			result.AppendLine($"QFB - Quadratic Factor Base - Count: {QuadradicFactorPairCollection.Count} - Array of (p, r) such that ƒ(r) ≡ 0 (mod p) and p is prime");
			result.AppendLine(QuadradicFactorPairCollection.ToString());
			result.AppendLine();

			result.AppendLine();
			result.AppendLine();

			return result.ToString();
		}

		#endregion

		#region JSON Load/Save

		public static void JsonSave(GNFS gnfs)
		{
			Serialization.Save(gnfs, gnfs.SaveLocations.GnfsParameters_SaveFile);

			Serialization.Save(gnfs.PrimeFactorBase.RationalFactorBase, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.PrimeFactorBase.RationalFactorBase)}.json"));
			Serialization.Save(gnfs.PrimeFactorBase.AlgebraicFactorBase, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.PrimeFactorBase.AlgebraicFactorBase)}.json"));
			Serialization.Save(gnfs.PrimeFactorBase.QuadraticFactorBase, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.PrimeFactorBase.QuadraticFactorBase)}.json"));

			Serialization.Save(gnfs.RationalFactorPairCollection, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.RationalFactorPairCollection)}.json"));
			Serialization.Save(gnfs.AlgebraicFactorPairCollection, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.AlgebraicFactorPairCollection)}.json"));
			Serialization.Save(gnfs.QuadradicFactorPairCollection, Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.QuadradicFactorPairCollection)}.json"));

			gnfs.CurrentRelationsProgress.Relations.Save(gnfs.SaveLocations.SaveDirectory);
		}

		public static GNFS JsonLoad(CancellationToken cancelToken, string filename)
		{
			string loadJson = File.ReadAllText(filename);
			GNFS gnfs = JsonConvert.DeserializeObject<GNFS>(loadJson);
			gnfs.CancelToken = cancelToken;

			gnfs.SaveLocations = new DirectoryLocations(Path.GetDirectoryName(filename));

			gnfs.CurrentPolynomial = gnfs.PolynomialCollection.Last();

			gnfs.PrimeFactorBase.RationalFactorBase = Serialization.Load<List<BigInteger>>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.PrimeFactorBase.RationalFactorBase)}.json"));
			gnfs.PrimeFactorBase.AlgebraicFactorBase = Serialization.Load<List<BigInteger>>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.PrimeFactorBase.AlgebraicFactorBase)}.json"));
			gnfs.PrimeFactorBase.QuadraticFactorBase = Serialization.Load<List<BigInteger>>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.PrimeFactorBase.QuadraticFactorBase)}.json"));

			gnfs.RationalFactorPairCollection = Serialization.Load<FactorPairCollection>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.RationalFactorPairCollection)}.json"));
			gnfs.AlgebraicFactorPairCollection = Serialization.Load<FactorPairCollection>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.AlgebraicFactorPairCollection)}.json"));
			gnfs.QuadradicFactorPairCollection = Serialization.Load<FactorPairCollection>(Path.Combine(gnfs.SaveLocations.SaveDirectory, $"{nameof(gnfs.QuadradicFactorPairCollection)}.json"));

			gnfs.CurrentRelationsProgress._gnfs = gnfs;
			gnfs.CurrentRelationsProgress.Relations.Load(gnfs.SaveLocations.SaveDirectory);
			return gnfs;
		}

		#endregion
	}
}
