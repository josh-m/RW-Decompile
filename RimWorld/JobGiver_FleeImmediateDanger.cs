using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_FleeImmediateDanger : ThinkNode_JobGiver
	{
		private const int FleeDistance = 18;

		private const int DistToDangerToFlee = 18;

		private const int DistToFireToFlee = 10;

		private const int MinFiresNearbyToFlee = 60;

		private const int MinFiresNearbyRadius = 20;

		private const int MinFiresNearbyRegionsToScan = 18;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
			{
				return null;
			}
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
			for (int i = 0; i < list.Count; i++)
			{
				if (pawn.Position.InHorDistOf(list[i].Position, 18f))
				{
					Job job = this.FleeJob(pawn, list[i]);
					if (job != null)
					{
						return job;
					}
				}
			}
			if (pawn.RaceProps.Animal && pawn.Faction == null)
			{
				Job job2 = this.FleeLargeFireJob(pawn);
				if (job2 != null)
				{
					return job2;
				}
			}
			return null;
		}

		private Job FleeJob(Pawn pawn, Thing danger)
		{
			IntVec3 intVec;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.Flee)
			{
				intVec = pawn.CurJob.targetA.Cell;
			}
			else
			{
				intVec = CellFinderLoose.GetFleeDest(pawn, new List<Thing>
				{
					danger
				}, 18f);
			}
			if (intVec != pawn.Position)
			{
				return new Job(JobDefOf.Flee, intVec, danger);
			}
			return null;
		}

		private Job FleeLargeFireJob(Pawn pawn)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Fire);
			if (list.Count < 60)
			{
				return null;
			}
			TraverseParms tp = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
			Fire closestFire = null;
			float closestDistSq = -1f;
			int firesCount = 0;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, false), delegate(Region x)
			{
				List<Thing> list2 = x.ListerThings.ThingsInGroup(ThingRequestGroup.Fire);
				for (int i = 0; i < list2.Count; i++)
				{
					float num = (float)pawn.Position.DistanceToSquared(list2[i].Position);
					if (num <= 400f)
					{
						if (closestFire == null || num < closestDistSq)
						{
							closestDistSq = num;
							closestFire = (Fire)list2[i];
						}
						firesCount++;
					}
				}
				return closestDistSq <= 100f && firesCount >= 60;
			}, 18, RegionType.Set_Passable);
			if (closestDistSq <= 100f && firesCount >= 60)
			{
				Job job = this.FleeJob(pawn, closestFire);
				if (job != null)
				{
					return job;
				}
			}
			return null;
		}
	}
}
