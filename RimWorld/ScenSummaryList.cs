using System;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class ScenSummaryList
	{
		public static string SummaryWithList(Scenario scen, string tag, string intro)
		{
			string text = ScenSummaryList.SummaryList(scen, tag);
			if (!text.NullOrEmpty())
			{
				return "\n" + intro + ":\n" + text;
			}
			return null;
		}

		private static string SummaryList(Scenario scen, string tag)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (ScenPart current in scen.AllParts)
			{
				if (!current.summarized)
				{
					foreach (string current2 in current.GetSummaryListEntries(tag))
					{
						if (!flag)
						{
							stringBuilder.Append("\n");
						}
						stringBuilder.Append("   -" + current2);
						current.summarized = true;
						flag = false;
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
