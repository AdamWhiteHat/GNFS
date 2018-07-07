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

namespace GNFSCore
{
	using Factors;
	using IntegerMath;
	using Matrix;
	using Polynomial;

	public class RelationContainer
	{
		public List<List<Relation>> FreeRelations { get; internal set; }
		public List<Relation> SmoothRelations { get; internal set; }
		public List<Relation> UnFactored { get; internal set; }
		public List<RoughPair> RoughRelations { get; internal set; }

		public RelationContainer()
		{
			UnFactored = new List<Relation>();
			RoughRelations = new List<RoughPair>();
			SmoothRelations = new List<Relation>();
			FreeRelations = new List<List<Relation>>();
		}
	}
}
