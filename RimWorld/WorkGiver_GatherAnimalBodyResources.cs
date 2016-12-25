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

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			Pawn pawn2 = t as Pawn;
			return pawn2 != null && pawn2.RaceProps.Animal && pawn2.CanCasuallyInteractNow(false) && this.GetComp(pawn2) != null && this.GetComp(pawn2).ActiveAndFull && pawn.CanReserve(pawn2, 1);
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(this.JobDef, t);
		}
	}
}
