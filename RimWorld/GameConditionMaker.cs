using System;
using Verse;

namespace RimWorld
{
	public static class GameConditionMaker
	{
		public static GameCondition MakeConditionPermanent(GameConditionDef def)
		{
			GameCondition gameCondition = GameConditionMaker.MakeCondition(def, -1, -180000);
			gameCondition.Permanent = true;
			return gameCondition;
		}

		public static GameCondition MakeCondition(GameConditionDef def, int duration = -1, int startTickOffset = 0)
		{
			GameCondition gameCondition = (GameCondition)Activator.CreateInstance(def.conditionClass);
			gameCondition.startTick = Find.TickManager.TicksGame + startTickOffset;
			gameCondition.def = def;
			gameCondition.Duration = duration;
			return gameCondition;
		}
	}
}
