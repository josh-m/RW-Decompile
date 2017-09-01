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
				Pawn actor = layDown.actor;
				actor.pather.StopDead();
				JobDriver curDriver = actor.jobs.curDriver;
				if (hasBed)
				{
					Building_Bed t = (Building_Bed)actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
					if (!t.OccupiedRect().Contains(actor.Position))
					{
						Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + actor);
						actor.jobs.EndCurrentJob(JobCondition.Errored, true);
						return;
					}
					curDriver.layingDown = LayingDownState.LayingInBed;
				}
				else
				{
					curDriver.layingDown = LayingDownState.LayingSurface;
				}
				curDriver.asleep = false;
				actor.mindState.awokeVoluntarily = false;
				if (actor.mindState.applyBedThoughtsTick == 0)
				{
					actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
					actor.mindState.applyBedThoughtsOnLeave = false;
				}
				if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
				{
					ThoughtUtility.RemovePositiveBedroomThoughts(actor);
				}
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
					if (canSleep && actor.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor))
					{
						curDriver.asleep = true;
					}
				}
				else if (!canSleep || actor.needs.rest.CurLevel >= RestUtility.WakeThreshold(actor))
				{
					curDriver.asleep = false;
				}
				if (curDriver.asleep && gainRestAndHealth)
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
				if (actor.mindState.applyBedThoughtsTick != 0 && actor.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
				{
					Toils_LayDown.ApplyBedThoughts(actor);
					actor.mindState.applyBedThoughtsTick += 60000;
					actor.mindState.applyBedThoughtsOnLeave = true;
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
						actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1, null);
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
				if (!actor.mindState.awokeVoluntarily && curDriver.asleep && !actor.Dead && actor.needs.rest != null && actor.needs.rest.CurLevel < RestUtility.FallAsleepMaxLevel(actor) && actor.needs.mood != null)
				{
					actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed, null);
				}
				if (actor.mindState.applyBedThoughtsOnLeave)
				{
					Toils_LayDown.ApplyBedThoughts(actor);
				}
				curDriver.layingDown = LayingDownState.NotLaying;
				curDriver.asleep = false;
			});
			return layDown;
		}

		private static void ApplyBedThoughts(Pawn actor)
		{
			if (actor.needs.mood == null)
			{
				return;
			}
			Building_Bed building_Bed = actor.CurrentBed();
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
			actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
			if (actor.GetRoom(RegionType.Set_Passable).PsychologicallyOutdoors)
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside, null);
			}
			if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround, null);
			}
			if (actor.AmbientTemperature < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold, null);
			}
			if (actor.AmbientTemperature > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null))
			{
				actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat, null);
			}
			if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners && !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
			{
				ThoughtDef thoughtDef = null;
				if (building_Bed.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.Bedroom)
				{
					thoughtDef = ThoughtDefOf.SleptInBedroom;
				}
				else if (building_Bed.GetRoom(RegionType.Set_Passable).Role == RoomRoleDefOf.Barracks)
				{
					thoughtDef = ThoughtDefOf.SleptInBarracks;
				}
				if (thoughtDef != null)
				{
					int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom(RegionType.Set_Passable).GetStat(RoomStatDefOf.Impressiveness));
					if (thoughtDef.stages[scoreStageIndex] != null)
					{
						actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex), null);
					}
				}
			}
		}
	}
}
