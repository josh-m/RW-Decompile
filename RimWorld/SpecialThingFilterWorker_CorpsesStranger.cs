using System;
using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_CorpsesStranger : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			Corpse corpse = t as Corpse;
			return corpse != null && corpse.innerPawn.def.race.Humanlike && corpse.innerPawn.Faction != Faction.OfPlayer;
		}
	}
}
