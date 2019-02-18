using System;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Matrix;
	using GNFSCore.Factors;
	using GNFSCore.Polynomials;
	using GNFSCore.IntegerMath;

	public partial class GnfsUiBridge
	{
		public GNFS FindRelations(bool oneRound, GNFS gnfs, CancellationToken cancelToken)
		{
			int relationsDisplayLimit = 1;

			while (!cancelToken.IsCancellationRequested)
			{
				//List<RoughPair> knownRough = gnfs.CurrentRelationsProgress.RoughRelations;
				IEnumerable<Relation> smoothRelations = gnfs.CurrentRelationsProgress.GenerateRelations(cancelToken);
				if (smoothRelations == null)
				{
					break;
				}
				if (smoothRelations.Any())
				{
					//mainForm.LogOutput(gnfs.CurrentRelationsProgress.FormatRelations(smoothRelations.OrderByDescending(rel => rel.A * rel.B).Take(relationsDisplayLimit)));
					mainForm.LogOutput($"(Result set restricted to only {relationsDisplayLimit}) relations");
					mainForm.LogOutput();
					mainForm.LogOutput($"{smoothRelations.Count()} relations found this round.");
					mainForm.LogOutput();
				}

				if (oneRound)
				{
					break;
				}

				//IEnumerable<RoughPair> rough = gnfs.CurrentRelationsProgress.RoughRelations.Except(knownRough);
				//if (rough.Any())
				//{
				//	knownRough.AddRange(rough);
				//
				//	mainForm.LogOutput($"Rough numbers (Relations with remainders, i.e. not fully factored):");
				//	mainForm.LogOutput(rough/*.Skip(gnfs.RoughNumbers.Count-5)*/.FormatString());
				//	//mainForm.LogOutput("(restricted result set to top 5)");
				//	mainForm.LogOutput();
				//}
				/*
				var roughGroups = GNFS.GroupRoughNumbers(gnfs.RoughRelations);
				if (roughGroups.Any())
				{
					List<Relation> newRelations = GNFS.MultiplyLikeRoughNumbers(gnfs, roughGroups);
					Tuple<List<Relation>, List<RoughPair>> newSievedRelations = GNFS.SieveRelations(gnfs, newRelations);

					var stillRough = newSievedRelations.Item2;
					var newSmooth = newSievedRelations.Item1;

					int max = roughGroups.Count;
					int c2 = newRelations.Count;
					int c3 = stillRough.Count;

					max = Math.Min(max, Math.Min(c2, c3));

					mainForm.LogOutput($"COUNT: {roughGroups.Count} ({newSmooth.Count}) / {roughGroups.Count} / {gnfs.RoughRelations.Count}");
					mainForm.LogOutput();

					max = 4;

					int counter = 0;
					while (counter < max)
					{
						mainForm.LogOutput(
							$"{string.Join(" ; ", roughGroups[counter].Select(rg => rg.ToString()))}" + Environment.NewLine +
							$"{newRelations[counter]}" + Environment.NewLine +
							$"{stillRough[counter]}" + Environment.NewLine + Environment.NewLine
						);
						counter++;
					}

					//mainForm.LogOutput($"Rough numbers (Relations with remainders, i.e. not fully factored)");
					//mainForm.LogOutput($"Count: {roughGroups.Count}");
					//mainForm.LogOutput(roughGroups.FormatString());
					//mainForm.LogOutput();

					//mainForm.LogOutput($"New relations (Like rough relations multiplied together)");
					//mainForm.LogOutput($"Count: {newRelations.Count}");
					//mainForm.LogOutput(newRelations.FormatString());
					//mainForm.LogOutput();

					mainForm.LogOutput($"New smooth relations (from sieving rough relations)");
					mainForm.LogOutput($"Count: {newSmooth.Count}");
					mainForm.LogOutput(newSmooth.Take(5).FormatString());
					mainForm.LogOutput();
			    }
			    //mainForm.LogOutput($"Still rough relations (from sieving rough relations)");
			    //mainForm.LogOutput($"Count: {newSievedRelations.Item2.Count}");
			    //mainForm.LogOutput(newSievedRelations.Item2.FormatString());
			    mainForm.LogOutput();
			    mainForm.LogOutput();
			    */
			}

			return gnfs;
		}
	}
}
