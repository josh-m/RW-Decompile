using System;
using Verse;

namespace RimWorld
{
	public static class MapConditionMaker
	{
		public static MapCondition MakeConditionPermanent(MapConditionDef def)
		{
			return MapConditionMaker.MakeCondition(def, 2147483647, -180000);
		}

		public static MapCondition MakeCondition(MapConditionDef def, int duration = -1, int startTickOffset = 0)
		{
			MapCondition mapCondition = (MapCondition)Activator.CreateInstance(def.conditionClass);
			mapCondition.startTick = Find.TickManager.TicksGame + startTickOffset;
			mapCondition.def = def;
			mapCondition.duration = duration;
			return mapCondition;
		}
	}
}
