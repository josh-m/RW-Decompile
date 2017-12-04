using System;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_GameCondition : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.SpawnedOrAnyParentSpawned && p.MapHeld.gameConditionManager.ConditionIsActive(this.def.gameCondition))
			{
				return true;
			}
			if (Find.World.gameConditionManager.ConditionIsActive(this.def.gameCondition))
			{
				return true;
			}
			return false;
		}
	}
}
