using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedJoySources : Alert
	{
		public Alert_NeedJoySources()
		{
			this.defaultLabel = "NeedJoySource".Translate();
		}

		public override string GetExplanation()
		{
			Map map = this.BadMap();
			int num = JoyUtility.JoyKindsOnMapCount(map);
			string label = map.info.parent.Label;
			ExpectationDef expectationDef = ExpectationsUtility.CurrentExpectationFor(map);
			int joyKindsNeeded = expectationDef.joyKindsNeeded;
			string text = "AvailableRecreationTypes".Translate() + ":\n\n" + JoyUtility.JoyKindsOnMapString(map);
			string text2 = "MissingRecreationTypes".Translate() + ":\n\n" + JoyUtility.JoyKindsNotOnMapString(map);
			return "NeedJoySourceDesc".Translate(new object[]
			{
				num,
				label,
				expectationDef.label,
				joyKindsNeeded,
				text,
				text2
			});
		}

		public override AlertReport GetReport()
		{
			return this.BadMap() != null;
		}

		private Map BadMap()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (this.NeedJoySource(maps[i]))
				{
					return maps[i];
				}
			}
			return null;
		}

		private bool NeedJoySource(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			if (!map.mapPawns.AnyColonistSpawned)
			{
				return false;
			}
			int num = JoyUtility.JoyKindsOnMapCount(map);
			int joyKindsNeeded = ExpectationsUtility.CurrentExpectationFor(map).joyKindsNeeded;
			return num < joyKindsNeeded;
		}
	}
}
