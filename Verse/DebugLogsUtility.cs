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
				ThingDef def;
				(dictionary2 = dictionary)[def = current.def] = dictionary2[def] + 1;
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
