using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_GetRest : ThinkNode_JobGiver
	{
		private RestCategory minCategory;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetRest jobGiver_GetRest = (JobGiver_GetRest)base.DeepCopy(resolve);
			jobGiver_GetRest.minCategory = this.minCategory;
			return jobGiver_GetRest;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null)
			{
				return 0f;
			}
			if (rest.CurCategory < this.minCategory)
			{
				return 0f;
			}
			if (Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
			{
				return 0f;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
			{
				return 0f;
			}
			TimeAssignmentDef timeAssignmentDef;
			if (pawn.RaceProps.Humanlike)
			{
				timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
			}
			else
			{
				int num = GenLocalDate.HourOfDay(pawn);
				if (num < 7 || num > 21)
				{
					timeAssignmentDef = TimeAssignmentDefOf.Sleep;
				}
				else
				{
					timeAssignmentDef = TimeAssignmentDefOf.Anything;
				}
			}
			float curLevel = rest.CurLevel;
			if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
			{
				if (curLevel < 0.3f)
				{
					return 8f;
				}
				return 0f;
			}
			else
			{
				if (timeAssignmentDef == TimeAssignmentDefOf.Work)
				{
					return 0f;
				}
				if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
				{
					if (curLevel < 0.3f)
					{
						return 8f;
					}
					return 0f;
				}
				else
				{
					if (timeAssignmentDef != TimeAssignmentDefOf.Sleep)
					{
						throw new NotImplementedException();
					}
					if (curLevel < RestUtility.FallAsleepMaxLevel(pawn))
					{
						return 8f;
					}
					return 0f;
				}
			}
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null || rest.CurCategory < this.minCategory)
			{
				return null;
			}
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return null;
			}
			Lord lord = pawn.GetLord();
			Building_Bed building_Bed;
			if (lord != null && lord.CurLordToil != null && !lord.CurLordToil.AllowRestingInBed)
			{
				building_Bed = null;
			}
			else
			{
				building_Bed = RestUtility.FindBedFor(pawn);
			}
			if (building_Bed != null)
			{
				return new Job(JobDefOf.LayDown, building_Bed);
			}
			return new Job(JobDefOf.LayDown, this.FindGroundSleepSpotFor(pawn));
		}

		private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
		{
			Map map = pawn.Map;
			for (int i = 0; i < 2; i++)
			{
				int radius = (i != 0) ? 12 : 4;
				IntVec3 result;
				if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, map, radius, out result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
				{
					return result;
				}
			}
			return CellFinder.RandomClosewalkCellNearNotForbidden(pawn.Position, map, 4, pawn);
		}
	}
}
