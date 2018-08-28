using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_PsychicDrone : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			PsychicDroneLevel psychicDroneLevel = PsychicDroneLevel.None;
			CompPsychicDrone compPsychicDrone = ThoughtWorker_PsychicDrone.PsychicDroneEmanator(p.Map);
			if (compPsychicDrone != null)
			{
				psychicDroneLevel = compPsychicDrone.DroneLevel;
			}
			GameCondition_PsychicEmanation activeCondition = p.Map.gameConditionManager.GetActiveCondition<GameCondition_PsychicEmanation>();
			if (activeCondition != null && activeCondition.gender == p.gender && activeCondition.level > psychicDroneLevel)
			{
				psychicDroneLevel = activeCondition.level;
			}
			switch (psychicDroneLevel)
			{
			case PsychicDroneLevel.None:
				return false;
			case PsychicDroneLevel.GoodMedium:
				return ThoughtState.ActiveAtStage(0);
			case PsychicDroneLevel.BadLow:
				return ThoughtState.ActiveAtStage(1);
			case PsychicDroneLevel.BadMedium:
				return ThoughtState.ActiveAtStage(2);
			case PsychicDroneLevel.BadHigh:
				return ThoughtState.ActiveAtStage(3);
			case PsychicDroneLevel.BadExtreme:
				return ThoughtState.ActiveAtStage(4);
			default:
				throw new NotImplementedException();
			}
		}

		private static CompPsychicDrone PsychicDroneEmanator(Map map)
		{
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.PsychicDroneEmanator);
			if (!list.Any<Thing>())
			{
				return null;
			}
			return list[0].TryGetComp<CompPsychicDrone>();
		}
	}
}
