using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class GenStuff
	{
		public static ThingDef DefaultStuffFor(ThingDef td)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			if (ThingDefOf.WoodLog.stuffProps.CanMake(td))
			{
				return ThingDefOf.WoodLog;
			}
			if (ThingDefOf.Steel.stuffProps.CanMake(td))
			{
				return ThingDefOf.Steel;
			}
			if (ThingDefOf.Cloth.stuffProps.CanMake(td))
			{
				return ThingDefOf.Cloth;
			}
			ThingDef leatherDef = ThingDefOf.Cow.race.leatherDef;
			if (leatherDef.stuffProps.CanMake(td))
			{
				return leatherDef;
			}
			if (ThingDefOf.BlocksGranite.stuffProps.CanMake(td))
			{
				return ThingDefOf.BlocksGranite;
			}
			if (ThingDefOf.Plasteel.stuffProps.CanMake(td))
			{
				return ThingDefOf.Plasteel;
			}
			return GenStuff.RandomStuffFor(td);
		}

		public static ThingDef RandomStuffFor(ThingDef td)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			return GenStuff.AllowedStuffsFor(td).RandomElement<ThingDef>();
		}

		[DebuggerHidden]
		public static IEnumerable<ThingDef> AllowedStuffsFor(ThingDef td)
		{
			if (td.MadeFromStuff)
			{
				List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int i = 0; i < allDefs.Count; i++)
				{
					ThingDef d = allDefs[i];
					if (d.IsStuff && d.stuffProps.CanMake(td))
					{
						yield return d;
					}
				}
			}
		}
	}
}
