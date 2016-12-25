using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PickUpOpportunisticWeapon : ThinkNode_JobGiver
	{
		private bool preferBuildingDestroyers;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_PickUpOpportunisticWeapon jobGiver_PickUpOpportunisticWeapon = (JobGiver_PickUpOpportunisticWeapon)base.DeepCopy(resolve);
			jobGiver_PickUpOpportunisticWeapon.preferBuildingDestroyers = this.preferBuildingDestroyers;
			return jobGiver_PickUpOpportunisticWeapon;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.equipment == null)
			{
				return null;
			}
			if (this.AlreadySatisfiedWithCurrentWeapon(pawn))
			{
				return null;
			}
			if (pawn.RaceProps.Humanlike && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
			{
				return null;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return null;
			}
			if (pawn.GetRegion() == null)
			{
				return null;
			}
			Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Weapon), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 8f, (Thing x) => pawn.CanReserve(x, 1) && this.ShouldEquip(x, pawn), null, 15, false);
			if (thing != null)
			{
				return new Job(JobDefOf.Equip, thing);
			}
			return null;
		}

		private bool AlreadySatisfiedWithCurrentWeapon(Pawn pawn)
		{
			ThingWithComps primary = pawn.equipment.Primary;
			if (primary == null)
			{
				return false;
			}
			if (this.preferBuildingDestroyers)
			{
				if (!pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
				{
					return false;
				}
			}
			else if (!primary.def.IsRangedWeapon)
			{
				return false;
			}
			return true;
		}

		private bool ShouldEquip(Thing newWep, Pawn pawn)
		{
			return this.GetWeaponScore(newWep) > this.GetWeaponScore(pawn.equipment.Primary);
		}

		private int GetWeaponScore(Thing wep)
		{
			if (wep == null)
			{
				return 0;
			}
			if (this.preferBuildingDestroyers && wep.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
			{
				return 3;
			}
			if (wep.def.IsRangedWeapon)
			{
				return 2;
			}
			return 1;
		}
	}
}
