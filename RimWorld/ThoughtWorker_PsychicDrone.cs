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
			Building_PsychicEmanator building_PsychicEmanator = ThoughtWorker_PsychicDrone.ExtantShipPart(p.Map);
			if (building_PsychicEmanator != null)
			{
				psychicDroneLevel = building_PsychicEmanator.droneLevel;
			}
			MapCondition_PsychicEmanation activeCondition = p.Map.mapConditionManager.GetActiveCondition<MapCondition_PsychicEmanation>();
			if (activeCondition != null && activeCondition.gender == p.gender && activeCondition.def.droneLevel > psychicDroneLevel)
			{
				psychicDroneLevel = activeCondition.def.droneLevel;
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

		private static Building_PsychicEmanator ExtantShipPart(Map map)
		{
			List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.CrashedPsychicEmanatorShipPart);
			if (list.Count == 0)
			{
				return null;
			}
			return (Building_PsychicEmanator)list[0];
		}
	}
}
