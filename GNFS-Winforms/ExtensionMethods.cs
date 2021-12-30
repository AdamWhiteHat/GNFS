using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GNFS_Winforms
{
	public static class TimeSpanExtensionMethods
	{
		public static string FormatString(this TimeSpan source)
		{
			bool subSecond = true;
			List<string> elapsedString = new List<string>();
			if (source.Days > 0)
			{
				elapsedString.Add($"{source.Days} Days");
				subSecond = false;
			}
			if (source.Hours > 0)
			{
				elapsedString.Add($"{source.Hours} Hours");
				subSecond = false;
			}
			if (source.Minutes > 0)
			{
				elapsedString.Add($"{source.Minutes} Minutes");
				subSecond = false;
			}
			if (source.Seconds > 0)
			{
				elapsedString.Add($"{source.Seconds}.{source.Milliseconds} Seconds");
				subSecond = false;
			}
			if (subSecond)
			{
				elapsedString.Add($"{source.Milliseconds} Milliseconds");
			}
			return string.Join(", ", elapsedString);
		}
	}
}
