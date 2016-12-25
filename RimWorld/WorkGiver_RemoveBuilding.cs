using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_RemoveBuilding : WorkGiver_Scanner
	{
		protected abstract DesignationDef Designation
		{
			get;
		}

		protected abstract JobDef RemoveBuildingJob
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

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation des in Find.DesignationManager.DesignationsOfDef(this.Designation))
			{
				yield return des.target.Thing;
			}
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t)
		{
			if (t.def.CanHaveFaction)
			{
				if (t.Faction != pawn.Faction)
				{
					return false;
				}
			}
			else if (pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			return pawn.CanReserve(t, 1) && Find.DesignationManager.DesignationOn(t, this.Designation) != null;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			return new Job(this.RemoveBuildingJob, t);
		}
	}
}
