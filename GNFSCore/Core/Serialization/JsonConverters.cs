using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace GNFSCore
{
	using Interfaces;

	public static partial class Serialization
	{
		public class PolynomialConverter : CustomCreationConverter<IPolynomial>
		{
			public override IPolynomial Create(Type objectType)
			{
				return new Polynomial();
			}
		}

		public class TermConverter : CustomCreationConverter<ITerm>
		{
			public override ITerm Create(Type objectType)
			{
				return new Term();
			}
		}
	}
}
