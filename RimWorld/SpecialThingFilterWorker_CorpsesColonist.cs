using System;
using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_CorpsesColonist : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			Corpse corpse = t as Corpse;
			return corpse != null && corpse.InnerPawn.def.race.Humanlike && corpse.InnerPawn.Faction == Faction.OfPlayer;
		}
	}
}
