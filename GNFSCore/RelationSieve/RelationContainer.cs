using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace GNFSCore
{	
	public class RelationContainer
	{
		[JsonIgnore]
		public List<Relation> SmoothRelations { get; internal set; }
		[JsonIgnore]
		public List<Relation> UnFactoredRelations { get; internal set; }
		[JsonIgnore]
		public List<Relation> RoughRelations { get; internal set; }
		[JsonIgnore]
		public List<List<Relation>> FreeRelations { get; internal set; }

		public RelationContainer()
		{
			UnFactoredRelations = new List<Relation>();
			RoughRelations = new List<Relation>();
			SmoothRelations = new List<Relation>();
			FreeRelations = new List<List<Relation>>();
		}

		public void Save(string saveDirectory)
		{
			if (SmoothRelations.Any())
			{
				Serialization.Save(SmoothRelations, Path.Combine(saveDirectory, $"{nameof(SmoothRelations)}.json"));
			}

			if (UnFactoredRelations.Any())
			{
				Serialization.Save(UnFactoredRelations, Path.Combine(saveDirectory, $"{nameof(UnFactoredRelations)}.json"));
			}

			if (RoughRelations.Any())
			{
				Serialization.Save(RoughRelations, Path.Combine(saveDirectory, $"{nameof(RoughRelations)}.json"));
			}

			if (FreeRelations.Any())
			{
				int counter = 1;
				foreach (List<Relation> solution in FreeRelations)
				{
					Serialization.Save(solution, Path.Combine(saveDirectory, $"{nameof(FreeRelations)}_{counter}.json"));
					counter++;
				}
			}
		}

		public void Load(string saveDirectory)
		{
			string filename = Path.Combine(saveDirectory, $"{nameof(SmoothRelations)}.json");
			if (File.Exists(filename))
			{
				SmoothRelations = Serialization.Load<List<Relation>>(filename);
			}

			filename = Path.Combine(saveDirectory, $"{nameof(UnFactoredRelations)}.json");
			if (File.Exists(filename))
			{
				UnFactoredRelations = Serialization.Load<List<Relation>>(filename);
			}

			filename = Path.Combine(saveDirectory, $"{nameof(RoughRelations)}.json");
			if (File.Exists(filename))
			{
				RoughRelations = Serialization.Load<List<Relation>>(filename);
			}

			IEnumerable<string> freeRelations = Directory.EnumerateFiles(saveDirectory, $"{nameof(FreeRelations)}_*.json");
			foreach (string solution in freeRelations)
			{				
				FreeRelations.Add(Serialization.Load<List<Relation>>(solution));
			}
		}
	}
}
