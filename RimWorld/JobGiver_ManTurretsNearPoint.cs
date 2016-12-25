using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	internal class JobGiver_ManTurretsNearPoint : ThinkNode_JobGiver
	{
		public float maxDistFromPoint = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_ManTurretsNearPoint jobGiver_ManTurretsNearPoint = (JobGiver_ManTurretsNearPoint)base.DeepCopy(resolve);
			jobGiver_ManTurretsNearPoint.maxDistFromPoint = this.maxDistFromPoint;
			return jobGiver_ManTurretsNearPoint;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (!t.def.hasInteractionCell)
				{
					return false;
				}
				bool flag = false;
				for (int i = 0; i < t.def.comps.Count; i++)
				{
					if (t.def.comps[i].compClass == typeof(CompMannable))
					{
						flag = true;
						break;
					}
				}
				return flag && pawn.CanReserve(t, 1) && JobDriver_ManTurret.FindAmmoForTurret(pawn, t) != null;
			};
			Thing thing = GenClosest.ClosestThingReachable(pawn.GetLord().CurLordToil.FlagLoc, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), this.maxDistFromPoint, validator, null, -1, false);
			if (thing != null)
			{
				return new Job(JobDefOf.ManTurret, thing)
				{
					expiryInterval = 2000,
					checkOverrideOnExpire = true
				};
			}
			return null;
		}
	}
}
