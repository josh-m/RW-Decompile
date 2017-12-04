using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class SkyfallerMaker
	{
		public static Skyfaller MakeSkyfaller(ThingDef skyfaller)
		{
			return (Skyfaller)ThingMaker.MakeThing(skyfaller, null);
		}

		public static Skyfaller MakeSkyfaller(ThingDef skyfaller, ThingDef innerThing)
		{
			Thing innerThing2 = ThingMaker.MakeThing(innerThing, null);
			return SkyfallerMaker.MakeSkyfaller(skyfaller, innerThing2);
		}

		public static Skyfaller MakeSkyfaller(ThingDef skyfaller, Thing innerThing)
		{
			Skyfaller skyfaller2 = SkyfallerMaker.MakeSkyfaller(skyfaller);
			if (innerThing != null && !skyfaller2.innerContainer.TryAdd(innerThing, true))
			{
				Log.Error("Could not add " + innerThing.ToStringSafe<Thing>() + " to a skyfaller.");
				innerThing.Destroy(DestroyMode.Vanish);
			}
			return skyfaller2;
		}

		public static Skyfaller MakeSkyfaller(ThingDef skyfaller, IEnumerable<Thing> things)
		{
			Skyfaller skyfaller2 = SkyfallerMaker.MakeSkyfaller(skyfaller);
			if (things != null)
			{
				skyfaller2.innerContainer.TryAddRangeOrTransfer(things, false, true);
			}
			return skyfaller2;
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, IntVec3 pos, Map map)
		{
			Skyfaller newThing = SkyfallerMaker.MakeSkyfaller(skyfaller);
			return (Skyfaller)GenSpawn.Spawn(newThing, pos, map);
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, ThingDef innerThing, IntVec3 pos, Map map)
		{
			Skyfaller newThing = SkyfallerMaker.MakeSkyfaller(skyfaller, innerThing);
			return (Skyfaller)GenSpawn.Spawn(newThing, pos, map);
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, Thing innerThing, IntVec3 pos, Map map)
		{
			Skyfaller newThing = SkyfallerMaker.MakeSkyfaller(skyfaller, innerThing);
			return (Skyfaller)GenSpawn.Spawn(newThing, pos, map);
		}

		public static Skyfaller SpawnSkyfaller(ThingDef skyfaller, IEnumerable<Thing> things, IntVec3 pos, Map map)
		{
			Skyfaller newThing = SkyfallerMaker.MakeSkyfaller(skyfaller, things);
			return (Skyfaller)GenSpawn.Spawn(newThing, pos, map);
		}
	}
}
