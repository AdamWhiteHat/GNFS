using System.Collections.Generic;
using Newtonsoft.Json;

namespace GNFSCore.Data.RelationSieve
{
	public class RelationContainer
	{
		[JsonIgnore]
		public List<Relation> SmoothRelations { get; internal set; }
		[JsonIgnore]
		public List<Relation> RoughRelations { get; internal set; }
		[JsonIgnore]
		public List<List<Relation>> FreeRelations { get; internal set; }

		public RelationContainer()
		{
			SmoothRelations = new List<Relation>();
			RoughRelations = new List<Relation>();
			FreeRelations = new List<List<Relation>>();
		}
	}
}
