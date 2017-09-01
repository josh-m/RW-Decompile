using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_ManTurrets : ThinkNode_JobGiver
	{
		public float maxDistFromPoint = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_ManTurrets jobGiver_ManTurrets = (JobGiver_ManTurrets)base.DeepCopy(resolve);
			jobGiver_ManTurrets.maxDistFromPoint = this.maxDistFromPoint;
			return jobGiver_ManTurrets;
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
				return flag && pawn.CanReserve(t, 1, -1, null, false) && JobDriver_ManTurret.FindAmmoForTurret(pawn, t) != null;
			};
			Thing thing = GenClosest.ClosestThingReachable(this.GetRoot(pawn), pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), this.maxDistFromPoint, validator, null, 0, -1, false, RegionType.Set_Passable, false);
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

		protected abstract IntVec3 GetRoot(Pawn pawn);
	}
}
