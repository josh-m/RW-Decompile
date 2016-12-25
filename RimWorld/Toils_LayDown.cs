using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_LayDown
	{
		private const int TicksBetweenSleepZs = 100;

		private const float GroundRestEffectiveness = 0.8f;

		private const int GetUpOrStartJobWhileInBedCheckInterval = 211;

		public static Toil LayDown(TargetIndex bedOrRestSpotIndex, bool hasBed, bool lookForOtherJobs, bool canSleep = true, bool gainRestAndHealth = true)
		{
			Toil layDown = new Toil();
			layDown.initAction = delegate
			{
				layDown.actor.pather.StopDead();
				JobDriver curDriver = layDown.actor.jobs.curDriver;
				curDriver.layingDown = true;
				curDriver.layingDownBed = (Building_Bed)layDown.actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
				curDriver.asleep = false;
				layDown.actor.mindState.awokeVoluntarily = false;
			};
			layDown.tickAction = delegate
			{
				Pawn actor = layDown.actor;
				Job curJob = actor.CurJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Building_Bed building_Bed = (Building_Bed)curJob.GetTarget(bedOrRestSpotIndex).Thing;
				actor.GainComfortFromCellIfPossible();
				if (!curDriver.asleep)
				{
					if (canSleep && actor.needs.rest.CurLevel < 0.75f)
					{
						curDriver.asleep = true;
					}
				}
				else if (!canSleep || actor.needs.rest.CurLevel >= 1f)
				{
					curDriver.asleep = false;
				}
				if (curDriver.asleep)
				{
					if (Find.TickManager.TicksGame % 750 == 0 && actor.needs.mood != null)
					{
						if (RoomQuery.RoomAt(actor).TouchesMapEdge)
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptOutside, null);
						}
						if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptOnGround, null);
						}
						if (GenTemperature.GetTemperatureForCell(actor.Position, actor.Map) < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptInCold, null);
						}
						if (GenTemperature.GetTemperatureForCell(actor.Position, actor.Map) > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null))
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptInHeat, null);
						}
					}
					if (gainRestAndHealth)
					{
						float num;
						if (building_Bed != null && building_Bed.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
						{
							num = building_Bed.GetStatValue(StatDefOf.BedRestEffectiveness, true);
						}
						else
						{
							num = 0.8f;
						}
						float num2 = RestUtility.PawnHealthRestEffectivenessFactor(actor);
						num = 0.7f * num + 0.3f * num * num2;
						actor.needs.rest.TickResting(num);
					}
				}
				if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
				{
					if (curDriver.asleep)
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_SleepZ);
					}
					if (gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOf.Mote_HealingCross);
					}
				}
				if (actor.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.owners.Contains(actor))
				{
					if (actor.Downed)
					{
						actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				if (lookForOtherJobs && actor.IsHashIntervalTick(211))
				{
					actor.mindState.awokeVoluntarily = true;
					actor.jobs.CheckForJobOverride();
					actor.mindState.awokeVoluntarily = false;
					return;
				}
			};
			layDown.defaultCompleteMode = ToilCompleteMode.Never;
			if (hasBed)
			{
				layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
			}
			layDown.AddFinishAction(delegate
			{
				Pawn actor = layDown.actor;
				JobDriver curDriver = actor.jobs.curDriver;
				if (!actor.mindState.awokeVoluntarily && curDriver.asleep && !actor.Dead && actor.needs.rest != null && actor.needs.rest.CurLevel < 0.75f && actor.needs.mood != null)
				{
					actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleepDisturbed, null);
				}
				curDriver.layingDown = false;
				curDriver.layingDownBed = null;
				curDriver.asleep = false;
			});
			return layDown;
		}
	}
}
