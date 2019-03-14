using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GNFSCore
{
	using Interfaces;

	[JsonArray(ItemConverterType = typeof(Serialization.TermConverter))]
	public class TermCollection : List<ITerm>
	{
		public TermCollection()
			: base()
		{ }

		public TermCollection(List<ITerm> terms)
			: base(terms)
		{
		}

		public TermCollection(Term[] terms)
			: base(terms.Select(t => (ITerm)t))
		{
		}
	}
}
