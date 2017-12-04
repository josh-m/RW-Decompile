using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_GatherAnimalBodyResources : WorkGiver_Scanner
	{
		protected abstract JobDef JobDef
		{
			get;
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		protected abstract CompHasGatherableBodyResource GetComp(Pawn animal);

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Pawn> pawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
			for (int i = 0; i < pawns.Count; i++)
			{
				yield return pawns[i];
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || !pawn2.RaceProps.Animal)
			{
				return false;
			}
			CompHasGatherableBodyResource comp = this.GetComp(pawn2);
			if (comp != null && comp.ActiveAndFull && !pawn2.Downed && pawn2.CanCasuallyInteractNow(false))
			{
				LocalTargetInfo target = pawn2;
				if (pawn.CanReserve(target, 1, -1, null, forced))
				{
					return true;
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(this.JobDef, t);
		}
	}
}
