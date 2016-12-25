using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public class DebugLogsUtility
	{
		public static string ThingListToUniqueCountString(IEnumerable<Thing> things)
		{
			if (things == null)
			{
				return "null";
			}
			Dictionary<ThingDef, int> dictionary = new Dictionary<ThingDef, int>();
			foreach (Thing current in things)
			{
				if (!dictionary.ContainsKey(current.def))
				{
					dictionary.Add(current.def, 0);
				}
				Dictionary<ThingDef, int> dictionary2;
				Dictionary<ThingDef, int> expr_44 = dictionary2 = dictionary;
				ThingDef def;
				ThingDef expr_4D = def = current.def;
				int num = dictionary2[def];
				expr_44[expr_4D] = num + 1;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Registered things in dynamic draw list:");
			foreach (KeyValuePair<ThingDef, int> current2 in from k in dictionary
			orderby k.Value descending
			select k)
			{
				stringBuilder.AppendLine(current2.Key + " - " + current2.Value);
			}
			return stringBuilder.ToString();
		}
	}
}
