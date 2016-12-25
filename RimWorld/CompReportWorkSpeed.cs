using System;
using Verse;

namespace RimWorld
{
	public class CompReportWorkSpeed : ThingComp
	{
		public override string CompInspectStringExtra()
		{
			bool flag = StatPart_WorkTableOutdoors.Applies(this.parent.def, this.parent.Map, this.parent.Position);
			bool flag2 = StatPart_WorkTableTemperature.Applies(this.parent);
			bool flag3 = StatPart_WorkTableUnpowered.Applies(this.parent);
			if (flag || flag2 || flag3)
			{
				string text = "WorkSpeedPenalty".Translate() + ": ";
				bool flag4 = false;
				if (flag)
				{
					text += "Outdoors".Translate().ToLower();
					flag4 = true;
				}
				if (flag2)
				{
					if (flag4)
					{
						text += ", ";
					}
					text += "BadTemperature".Translate().ToLower();
					flag4 = true;
				}
				if (flag3)
				{
					if (flag4)
					{
						text += ", ";
					}
					text += "NoPower".Translate().ToLower();
				}
				return text;
			}
			return null;
		}
	}
}
