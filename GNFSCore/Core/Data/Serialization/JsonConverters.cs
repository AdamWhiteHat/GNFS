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
        public class JsonPolynomialConverter : CustomCreationConverter<Polynomial>
        {
            public override Polynomial Create(Type objectType)
            {
                return new Polynomial();
            }
        }

        public class JsonTermConverter : CustomCreationConverter<Term>
        {
            public override Term Create(Type objectType)
            {
                return new Term();
            }
        }
    }
}
