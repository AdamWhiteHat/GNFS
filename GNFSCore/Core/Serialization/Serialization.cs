using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace GNFSCore
{
	using Interfaces;
	public static class Serialization
	{
		public static void Save(object obj, string filename)
		{
			string saveJson = JsonConvert.SerializeObject(obj, Formatting.Indented);
			File.WriteAllText(filename, saveJson);
		}

		public static T Load<T>(string filename)
		{
			string loadJson = File.ReadAllText(filename);
			return JsonConvert.DeserializeObject<T>(loadJson);
		}

		public class ConcreteTypeConverter<TConcrete> : JsonConverter
		{
			public override bool CanConvert(Type objectType)
			{
				return true;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				return serializer.Deserialize<TConcrete>(reader);
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				serializer.Serialize(writer, value);
			}
		}

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

		public class IPolynomialConverter : CustomCreationConverter<Polynomial>
		{
			public override Polynomial Create(Type objectType)
			{
				return new Polynomial();
			}
		}

		public class PolynomialCollectionConverter : JsonConverter
		{
			public override bool CanConvert(Type objectType)
			{
				return true;
			}

			public override bool CanRead { get { return true; } }
			//public override bool CanWrite { get { return true; } }

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				if (CanConvert(objectType))
				{
					List<IPolynomial> results = new List<IPolynomial>();

					JArray polyCollection = (JArray)serializer.Deserialize(reader);
					IEnumerable<JToken> polynomials = polyCollection.Values().Select(tok => tok.First());
					foreach (JToken property in polynomials)
					{
						List<Term> terms = new List<Term>();

						JTokenType propType = property.Type;
						switch (propType)
						{
							case JTokenType.Array:

								IJEnumerable<JToken> polyTerms = property.Children();
								foreach (JToken term in polyTerms)
								{
									string exp = term["Exponent"].ToString();
									string coeff = term["CoEfficient"].ToString();

									int exponent = int.Parse(exp);
									BigInteger coefficient = BigInteger.Parse(coeff);

									terms.Add(new Term(coefficient, exponent));
								}
								break;
						}


						IPolynomial poly = new Polynomial(terms.ToArray());
						results.Add(poly);
					}

					return results;
				}
				else
				{
					throw new JsonReaderException($"Cannot convert type {objectType.FullName} to {typeof(List<Polynomial>).FullName}");
				}
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				//serializer.Serialize(writer, JsonConvert.SerializeObject(value, typeof(List<Polynomial>), new JsonSerializerSettings() { }));
				serializer.Serialize(writer, value);
			}
		}
	}
}
