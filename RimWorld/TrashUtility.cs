using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	internal static class TrashUtility
	{
		private const float ChanceHateInertBuilding = 0.02f;

		private static readonly IntRange TrashJobCheckOverrideInterval = new IntRange(450, 500);

		public static bool ShouldTrashPlant(Pawn pawn, Plant p)
		{
			if (!p.sown || !p.FlammableNow || !TrashUtility.CanTrash(pawn, p))
			{
				return false;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = p.Position + GenAdj.AdjacentCells[i];
				if (c.InBounds(p.Map) && c.ContainsStaticFire(p.Map))
				{
					return false;
				}
			}
			return p.Position.Roofed(p.Map) || p.Map.weatherManager.RainRate <= 0.25f;
		}

		public static bool ShouldTrashBuilding(Pawn pawn, Building b)
		{
			if (!b.def.useHitPoints)
			{
				return false;
			}
			if (b.def.building.isInert || b.def.building.isTrap)
			{
				Rand.PushSeed();
				int num = GenLocalDate.HourOfDay(pawn) / 3;
				Rand.Seed = (b.GetHashCode() * 612361 ^ pawn.GetHashCode() * 391 ^ num * 734273247);
				if (Rand.Value > 0.02f)
				{
					Rand.PopSeed();
					return false;
				}
				Rand.PopSeed();
			}
			return (!b.def.building.isTrap || !((Building_Trap)b).Armed) && TrashUtility.CanTrash(pawn, b) && pawn.HostileTo(b);
		}

		private static bool CanTrash(Pawn pawn, Thing t)
		{
			return pawn.CanReach(t, PathEndMode.Touch, Danger.Some, false, TraverseMode.ByPawn) && !t.IsBurning();
		}

		public static Job TrashJob(Pawn pawn, Thing t)
		{
			Plant plant = t as Plant;
			if (plant != null)
			{
				Job job = new Job(JobDefOf.Ignite, t);
				TrashUtility.FinalizeTrashJob(job);
				return job;
			}
			if (Rand.Value < 0.7f)
			{
				foreach (Verb current in pawn.equipment.AllEquipmentVerbs)
				{
					if (current.verbProps.ai_IsBuildingDestroyer)
					{
						Job job2 = new Job(JobDefOf.UseVerbOnThing, t);
						job2.verbToUse = current;
						TrashUtility.FinalizeTrashJob(job2);
						return job2;
					}
				}
			}
			float value = Rand.Value;
			Job job3;
			if (value < 0.35f && pawn.natives.IgniteVerb != null && t.FlammableNow && !t.IsBurning() && !(t is Building_Door))
			{
				job3 = new Job(JobDefOf.Ignite, t);
			}
			else
			{
				job3 = new Job(JobDefOf.AttackMelee, t);
			}
			TrashUtility.FinalizeTrashJob(job3);
			return job3;
		}

		private static void FinalizeTrashJob(Job job)
		{
			job.expiryInterval = TrashUtility.TrashJobCheckOverrideInterval.RandomInRange;
			job.checkOverrideOnExpire = true;
			job.expireRequiresEnemiesNearby = true;
		}
	}
}
