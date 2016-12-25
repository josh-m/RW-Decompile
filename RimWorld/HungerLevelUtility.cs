using System;
using Verse;

namespace RimWorld
{
	public static class HungerLevelUtility
	{
		public static string GetLabel(this HungerCategory hunger)
		{
			switch (hunger)
			{
			case HungerCategory.Fed:
				return "HungerLevel_Fed".Translate();
			case HungerCategory.Hungry:
				return "HungerLevel_Hungry".Translate();
			case HungerCategory.UrgentlyHungry:
				return "HungerLevel_UrgentlyHungry".Translate();
			case HungerCategory.Starving:
				return "HungerLevel_Starving".Translate();
			default:
				throw new InvalidOperationException();
			}
		}
	}
}
