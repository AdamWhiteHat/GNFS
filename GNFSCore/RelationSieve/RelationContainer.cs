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
