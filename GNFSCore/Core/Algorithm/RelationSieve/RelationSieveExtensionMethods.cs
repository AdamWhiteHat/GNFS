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
	using GNFSCore.Core.Data;
	using GNFSCore.Core.Data.RelationSieve;
	using Core.Algorithm.IntegerMath;
	using GNFSCore.Core.Algorithm;

	public static class RelationSieveExtensionMethods
	{
		
		public static void GenerateRelations(this PolyRelationsSieveProgress rel, CancellationToken cancelToken, GNFS _gnfs)
		{
			if (_gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Any())
			{
				// SmoothRelationsCounter should reflect accurately
				Serialization.Save.Relations.Smooth.Append(_gnfs); // This method updates SmoothRelationsCounter correctly
																   //_gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Clear();
			}

			/*
            int roughRelationCounter = 0;
            if (_gnfs.CurrentRelationsProgress.Relations.RoughRelations.Any())
            {
                Serialization.Save.Relations.Rough.Append(_gnfs);
                _gnfs.CurrentRelationsProgress.Relations.RoughRelations.Clear();
            }
            */

			rel.SmoothRelations_TargetQuantity = Math.Max(rel.SmoothRelations_TargetQuantity, rel.SmoothRelationsRequiredForMatrixStep); 


			if (rel.A >= rel.ValueRange)
			{
				rel.ValueRange += 200;
			}

			rel.ValueRange = (rel.ValueRange % 2 == 0) ? rel.ValueRange + 1 : rel.ValueRange;
			rel.A = (rel.A % 2 == 0) ? rel.A + 1 : rel.A;

			BigInteger startA = rel.A;

			while (rel.B >= rel.MaxB)
			{
				rel.MaxB += 100;
			}

			_gnfs.LogMessage($"GenerateRelations: TargetQuantity = {rel.SmoothRelations_TargetQuantity}, ValueRange = {rel.ValueRange}, A = {rel.A}, B = {rel.B}, Max B = {rel.MaxB}");

			while (rel.SmoothRelationsCounter < rel.SmoothRelations_TargetQuantity)
			{
				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				if (rel.B > rel.MaxB)
				{
					break;
				}

				foreach (BigInteger a in SieveRange.GetSieveRangeContinuation(rel.A, rel.ValueRange))
				{
					if (cancelToken.IsCancellationRequested)
					{
						break;
					}

					rel.A = a;
					if (GCD.AreCoprime(rel.A, rel.B))
					{
						Relation relation = new Relation(_gnfs, rel.A, rel.B);

						Sieve.Relation(_gnfs.CurrentRelationsProgress, relation);

						bool smooth = relation.IsSmooth;
						if (smooth)
						{
							Serialization.Save.Relations.Smooth.Append(_gnfs, relation);

							_gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Add(relation);

							//_gnfs.LogMessage($"Found smooth relation: A = {rel.A}, B = {rel.B}");
						}
						else
						{
							/*
							_gnfs.CurrentRelationsProgress.Relations.RoughRelations.Add(rel);
							roughRelationCounter++;

							if (roughRelationCounter > 1000)
							{
								Serialization.Save.Relations.Rough.AppendList(_gnfs, _gnfs.CurrentRelationsProgress.Relations.RoughRelations);
								_gnfs.CurrentRelationsProgress.Relations.RoughRelations.Clear();
								roughRelationCounter = 0;
							}
							*/
						}
					}
				}

				if (cancelToken.IsCancellationRequested)
				{
					break;
				}

				rel.B += 1;
				rel.A = startA;

				//if (B % 11 == 0)
				_gnfs.LogMessage($"B = {rel.B}");
				_gnfs.LogMessage($"SmoothRelations.Count: {_gnfs.CurrentRelationsProgress.Relations.SmoothRelations.Count}");

			}
		}



	}
}
