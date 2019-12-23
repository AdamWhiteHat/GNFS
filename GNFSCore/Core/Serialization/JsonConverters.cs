using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using ExtendedArithmetic;

namespace GNFSCore
{
    public static partial class Serialization
    {
        public class JsonPolynomialConverter : CustomCreationConverter<IPolynomial>
        {
            public override IPolynomial Create(Type objectType)
            {
                return new Polynomial();
            }
        }

        public class JsonTermConverter : CustomCreationConverter<ITerm>
        {
            public override ITerm Create(Type objectType)
            {
                return new Term();
            }
        }
    }
}
