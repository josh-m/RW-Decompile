using System;
using Verse;

namespace RimWorld
{
	public class Building_Sarcophagus : Building_Grave
	{
		private bool everNonEmpty;

		private bool thisIsFirstBodyEver;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<bool>(ref this.everNonEmpty, "everNonEmpty", false, false);
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				this.thisIsFirstBodyEver = !this.everNonEmpty;
				this.everNonEmpty = true;
				return true;
			}
			return false;
		}

		public override void Notify_CorpseBuried(Pawn worker)
		{
			base.Notify_CorpseBuried(worker);
			if (this.thisIsFirstBodyEver && worker.IsColonist && base.Corpse.InnerPawn.def.race.Humanlike && !base.Corpse.everBuriedInSarcophagus)
			{
				base.Corpse.everBuriedInSarcophagus = true;
				foreach (Pawn current in base.Map.mapPawns.FreeColonists)
				{
					if (current.needs.mood != null)
					{
						current.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.KnowBuriedInSarcophagus, null);
					}
				}
			}
		}
	}
}
