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
				string str = "WorkSpeedPenalty".Translate() + ": ";
				string text = string.Empty;
				if (flag)
				{
					text += "Outdoors".Translate().ToLower();
				}
				if (flag2)
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += "BadTemperature".Translate().ToLower();
				}
				if (flag3)
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += "NoPower".Translate().ToLower();
				}
				return str + text.CapitalizeFirst();
			}
			return null;
		}
	}
}
