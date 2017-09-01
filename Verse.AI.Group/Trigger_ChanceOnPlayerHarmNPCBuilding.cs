using RimWorld;
using System;

namespace Verse.AI.Group
{
	public class Trigger_ChanceOnPlayerHarmNPCBuilding : Trigger
	{
		private float chance = 1f;

		public Trigger_ChanceOnPlayerHarmNPCBuilding(float chance)
		{
			this.chance = chance;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			return signal.type == TriggerSignalType.BuildingDamaged && signal.dinfo.Def.externalViolence && signal.thing.def.category == ThingCategory.Building && signal.dinfo.Instigator != null && signal.dinfo.Instigator.Faction == Faction.OfPlayer && signal.thing.Faction != Faction.OfPlayer && Rand.Value < this.chance;
		}
	}
}
