using System;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DeepDrillUtility
	{
		public static ThingDef GetNextResource(IntVec3 p, Map map)
		{
			ThingDef result;
			int num;
			IntVec3 intVec;
			DeepDrillUtility.GetNextResource(p, map, out result, out num, out intVec);
			return result;
		}

		public static bool GetNextResource(IntVec3 p, Map map, out ThingDef resDef, out int countPresent, out IntVec3 cell)
		{
			for (int i = 0; i < 9; i++)
			{
				IntVec3 intVec = p + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map))
				{
					ThingDef thingDef = map.deepResourceGrid.ThingDefAt(intVec);
					if (thingDef != null)
					{
						resDef = thingDef;
						countPresent = map.deepResourceGrid.CountAt(intVec);
						cell = intVec;
						return true;
					}
				}
			}
			resDef = DeepDrillUtility.GetBaseResource(map);
			countPresent = 2147483647;
			cell = p;
			return false;
		}

		public static ThingDef GetBaseResource(Map map)
		{
			if (!map.Biome.hasBedrock)
			{
				return null;
			}
			return (from rock in Find.World.NaturalRockTypesIn(map.Tile)
			select rock.building.mineableThing).FirstOrDefault<ThingDef>();
		}
	}
}
