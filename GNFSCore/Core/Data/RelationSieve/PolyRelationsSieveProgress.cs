using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GNFSCore.Core.Data.RelationSieve
{
	using GNFSCore.Core.Algorithm;
	using GNFSCore.Core.Algorithm.ExtensionMethods;
	using GNFSCore.Core.Algorithm.IntegerMath;
	using GNFSCore.Core.Data;
	using Algorithm.IntegerMath;

	[DataContract]
	public class PolyRelationsSieveProgress
	{
		[DataMember]
		public BigInteger A { get; set; }
		[DataMember]
		public BigInteger B { get; set; }
		[DataMember]
		public int SmoothRelations_TargetQuantity { get; set; }

		[DataMember]
		public BigInteger ValueRange { get; set; }

		public List<List<Relation>> FreeRelations { get { return Relations.FreeRelations; } }
		public List<Relation> SmoothRelations { get { return Relations.SmoothRelations; } }
		public List<Relation> RoughRelations { get { return Relations.RoughRelations; } }

		public RelationContainer Relations { get; set; }

		[DataMember]
		public BigInteger MaxB { get; set; }
		public int SmoothRelationsCounter { get; set; }
		public int FreeRelationsCounter { get; set; }

		public int SmoothRelationsRequiredForMatrixStep
		{
			get
			{
				return PrimeFactory.GetIndexFromValue(_gnfs.PrimeFactorBase.RationalFactorBaseMax)
					  + PrimeFactory.GetIndexFromValue(_gnfs.PrimeFactorBase.AlgebraicFactorBaseMax)
					  + _gnfs.QuadraticFactorPairCollection.Count + 3;
			}
		}

		internal GNFS _gnfs;

		#region Constructors

		public PolyRelationsSieveProgress()
		{
			Relations = new RelationContainer();
		}

		public PolyRelationsSieveProgress(GNFS gnfs, BigInteger valueRange)
			: this(gnfs, -1, valueRange)
		{
		}

		public PolyRelationsSieveProgress(GNFS gnfs, int smoothRelationsTargetQuantity, BigInteger valueRange)
		{
			_gnfs = gnfs;
			Relations = new RelationContainer();

			A = 0;
			B = 3;
			ValueRange = valueRange;

			if (smoothRelationsTargetQuantity == -1)
			{
				SmoothRelations_TargetQuantity = SmoothRelationsRequiredForMatrixStep;
			}
			else
			{
				SmoothRelations_TargetQuantity = Math.Max(smoothRelationsTargetQuantity, SmoothRelationsRequiredForMatrixStep);
			}

			if (MaxB == 0)
			{
				MaxB = (uint)gnfs.PrimeFactorBase.AlgebraicFactorBaseMax;
			}
		}

		#endregion

		#region Misc

		public void IncreaseTargetQuantity()
		{
			IncreaseTargetQuantity(SmoothRelations_TargetQuantity - SmoothRelationsRequiredForMatrixStep);
		}

		public void IncreaseTargetQuantity(int ammount)
		{
			SmoothRelations_TargetQuantity += ammount;
			Serialization.Save.Gnfs(_gnfs);
		}

		public void PurgePrimeRoughRelations()
		{
			List<Relation> roughRelations = Relations.RoughRelations.ToList();

			IEnumerable<Relation> toRemoveAlg = roughRelations
				.Where(r => r.AlgebraicQuotient != 1 && FactorizationFactory.IsProbablePrime(r.AlgebraicQuotient));

			roughRelations = roughRelations.Except(toRemoveAlg).ToList();

			Relations.RoughRelations = roughRelations;

			IEnumerable<Relation> toRemoveRational = roughRelations
				.Where(r => r.RationalQuotient != 1 && FactorizationFactory.IsProbablePrime(r.RationalQuotient));

			roughRelations = roughRelations.Except(toRemoveRational).ToList();

			Relations.RoughRelations = roughRelations;
		}

		public void AddFreeRelationSolution(List<Relation> freeRelationSolution)
		{
			Relations.FreeRelations.Add(freeRelationSolution);
			Serialization.Save.Relations.Free.SingleSolution(_gnfs, freeRelationSolution);
			_gnfs.LogMessage($"Added free relation solution: Relation count = {freeRelationSolution.Count}");
		}

		#endregion

		#region ToString

		public string FormatRelations(IEnumerable<Relation> relations)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine($"Smooth relations:");
			result.AppendLine("\t_______________________________________________");
			result.AppendLine($"\t|   A   |  B | ALGEBRAIC_NORM | RATIONAL_NORM | \t\tRelations count: {Relations.SmoothRelations.Count} Target quantity: {SmoothRelations_TargetQuantity}");
			result.AppendLine("\t```````````````````````````````````````````````");
			foreach (Relation rel in relations.OrderByDescending(rel => rel.A * rel.B))
			{
				result.AppendLine(rel.ToString());
				result.AppendLine("Algebraic " + rel.AlgebraicFactorization.FormatStringAsFactorization());
				result.AppendLine("Rational  " + rel.RationalFactorization.FormatStringAsFactorization());
				result.AppendLine();
			}
			result.AppendLine();

			return result.ToString();
		}

		public override string ToString()
		{
			if (Relations.FreeRelations.Any())
			{
				StringBuilder result = new StringBuilder();

				List<Relation> relations = Relations.FreeRelations.First();

				result.AppendLine(FormatRelations(relations));

				BigInteger algebraic = relations.Select(rel => rel.AlgebraicNorm).Product();
				BigInteger rational = relations.Select(rel => rel.RationalNorm).Product();

				bool isAlgebraicSquare = algebraic.IsSquare();
				bool isRationalSquare = rational.IsSquare();

				CountDictionary algCountDict = new CountDictionary();
				foreach (Relation rel in relations)
				{
					algCountDict.Combine(rel.AlgebraicFactorization);
				}

				result.AppendLine("---");
				result.AppendLine($"Rational  ∏(a+mb): IsSquare? {isRationalSquare} : {rational}");
				result.AppendLine($"Algebraic ∏ƒ(a/b): IsSquare? {isAlgebraicSquare} : {algebraic}");
				result.AppendLine();
				result.AppendLine($"Algebraic factorization (as prime ideals): {algCountDict.FormatStringAsFactorization()}");
				result.AppendLine();

				result.AppendLine();
				result.AppendLine("");
				result.AppendLine(string.Join(Environment.NewLine,
					relations.Select(rel =>
					{
						BigInteger f = _gnfs.CurrentPolynomial.Evaluate(rel.A);
						if (rel.B == 0)
						{
							return "";
						}
						return $"ƒ({rel.A}) ≡ {f} ≡ {f % rel.B} (mod {rel.B})";
					}
					)));
				result.AppendLine();



				return result.ToString();
			}
			else
			{
				return FormatRelations(Relations.SmoothRelations);
			}
		}

		#endregion
	}
}
