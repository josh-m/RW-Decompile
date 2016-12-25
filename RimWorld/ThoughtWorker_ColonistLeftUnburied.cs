using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ColonistLeftUnburied : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer)
			{
				return false;
			}
			List<Thing> list = p.Map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Corpse));
			for (int i = 0; i < list.Count; i++)
			{
				Corpse corpse = (Corpse)list[i];
				if ((float)corpse.Age > 90000f && corpse.InnerPawn.Faction == Faction.OfPlayer && corpse.InnerPawn.def.race.Humanlike && !corpse.IsInAnyStorage())
				{
					return true;
				}
			}
			return false;
		}
	}
}
