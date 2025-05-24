using System;
using System.Linq;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	public static class Settings
	{
		public static string Log_FileName = SettingsReader.GetSettingValue<string>("Log.FileName");

		public static string N = SettingsReader.GetSettingValue<string>("N");
		public static string Degree = SettingsReader.GetSettingValue<string>("Degree");
		public static string Base = SettingsReader.GetSettingValue<string>("Base");
		public static string Bound = SettingsReader.GetSettingValue<string>("Bound");
		public static string RelationQuantity = SettingsReader.GetSettingValue<string>("RelationQuantity");
		public static string RelationValueRange = SettingsReader.GetSettingValue<string>("RelationValueRange");
	}
}