using System;
using Verse;

namespace RimWorld
{
	public static class GameConditionMaker
	{
		public static GameCondition MakeConditionPermanent(GameConditionDef def)
		{
			return GameConditionMaker.MakeCondition(def, 2147483647, -180000);
		}

		public static GameCondition MakeCondition(GameConditionDef def, int duration = -1, int startTickOffset = 0)
		{
			GameCondition gameCondition = (GameCondition)Activator.CreateInstance(def.conditionClass);
			gameCondition.startTick = Find.TickManager.TicksGame + startTickOffset;
			gameCondition.def = def;
			gameCondition.duration = duration;
			return gameCondition;
		}
	}
}
