using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_BuryCorpses : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.Corpse);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Count == 0;
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			Corpse corpse = t as Corpse;
			if (corpse == null)
			{
				return null;
			}
			if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, t))
			{
				return null;
			}
			Building_Grave building_Grave = this.FindBestGrave(pawn, corpse);
			if (building_Grave == null)
			{
				JobFailReason.Is("NoEmptyGraveLower".Translate());
				return null;
			}
			return new Job(JobDefOf.BuryCorpse, t, building_Grave)
			{
				count = corpse.stackCount
			};
		}

		private Building_Grave FindBestGrave(Pawn p, Corpse corpse)
		{
			Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(p) && p.CanReserve(m, 1) && ((Building_Grave)m).Accepts(corpse);
			if (corpse.InnerPawn.ownership != null && corpse.InnerPawn.ownership.AssignedGrave != null)
			{
				Building_Grave assignedGrave = corpse.InnerPawn.ownership.AssignedGrave;
				if (predicate(assignedGrave) && p.Map.reachability.CanReach(corpse.Position, assignedGrave, PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false)))
				{
					return assignedGrave;
				}
			}
			Func<Thing, float> priorityGetter = (Thing t) => (float)((IStoreSettingsParent)t).GetStoreSettings().Priority;
			Predicate<Thing> validator = predicate;
			return (Building_Grave)GenClosest.ClosestThing_Global_Reachable(corpse.Position, corpse.Map, corpse.Map.listerThings.ThingsInGroup(ThingRequestGroup.Grave), PathEndMode.ClosestTouch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, priorityGetter);
		}
	}
}
