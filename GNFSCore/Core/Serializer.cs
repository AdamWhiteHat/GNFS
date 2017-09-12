using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace GNFSCore
{
	public static class Serializer
	{
		public static void Serialize(string Filename, object obj)
		{
			using (StreamWriter streamWriter = new StreamWriter(Filename))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
				xmlSerializer.Serialize(streamWriter, obj);
			}
		}

		public static object Deserialize(string Filename, Type type)
		{
			object result = null;
			using (StreamReader reader = new StreamReader(Filename))
			{
				XmlSerializer serializer = new XmlSerializer(type);
				result = serializer.Deserialize(reader);
			}
			return result;
		}

		public static class JSON
		{
			public static void Serialize<T>(T obj, string filename)
			{
				string content = JsonConvert.SerializeObject(obj);
				File.WriteAllText(filename, content);
			}
		}
	}
}