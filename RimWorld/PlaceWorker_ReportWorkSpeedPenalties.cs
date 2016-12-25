using System;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ReportWorkSpeedPenalties : PlaceWorker
	{
		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			ThingDef thingDef = def as ThingDef;
			if (thingDef == null)
			{
				return;
			}
			bool flag = StatPart_WorkTableOutdoors.Applies(thingDef, map, loc);
			bool flag2 = StatPart_WorkTableTemperature.Applies(thingDef, map, loc);
			if (flag || flag2)
			{
				string text = "WillGetWorkSpeedPenalty".Translate(new object[]
				{
					def.label
				}).CapitalizeFirst() + ": ";
				if (flag)
				{
					text += "Outdoors".Translate().ToLower();
				}
				if (flag2)
				{
					if (flag)
					{
						text += ", ";
					}
					text += "BadTemperature".Translate().ToLower();
				}
				text += ".";
				Messages.Message(text, MessageSound.Negative);
			}
		}
	}
}
