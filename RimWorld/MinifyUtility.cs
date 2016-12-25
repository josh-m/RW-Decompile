using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class MinifyUtility
	{
		public static MinifiedThing MakeMinified(this Thing thing)
		{
			if (!thing.def.Minifiable)
			{
				Log.Warning("Tried to minify " + thing + " which is not minifiable.");
				return null;
			}
			Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(thing);
			MinifiedThing minifiedThing = (MinifiedThing)ThingMaker.MakeThing(thing.def.minifiedDef, null);
			minifiedThing.InnerThing = thing;
			if (blueprint_Install != null)
			{
				blueprint_Install.SetThingToInstallFromMinified(minifiedThing);
			}
			if (minifiedThing.InnerThing.stackCount > 1)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to minify ",
					thing.LabelCap,
					" with stack count ",
					minifiedThing.InnerThing.stackCount,
					". Clamped stack count to 1."
				}));
				minifiedThing.InnerThing.stackCount = 1;
			}
			if (thing.Spawned)
			{
				thing.DeSpawn();
			}
			return minifiedThing;
		}

		public static Thing GetInnerIfMinified(this Thing outerThing)
		{
			MinifiedThing minifiedThing = outerThing as MinifiedThing;
			if (minifiedThing != null)
			{
				return minifiedThing.InnerThing;
			}
			return outerThing;
		}

		public static MinifiedThing Uninstall(this Thing th)
		{
			if (!th.Spawned)
			{
				Log.Warning("Can't uninstall unspawned thing " + th);
				return null;
			}
			Map map = th.Map;
			MinifiedThing minifiedThing = th.MakeMinified();
			GenPlace.TryPlaceThing(minifiedThing, th.Position, map, ThingPlaceMode.Near, null);
			SoundDef.Named("ThingUninstalled").PlayOneShot(new TargetInfo(th.Position, map, false));
			return minifiedThing;
		}
	}
}
