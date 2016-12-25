using System;
using System.Collections.Generic;
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
			bool wokeUpVoluntarily = false;
			layDown.initAction = delegate
			{
				layDown.actor.pather.StopDead();
				JobDriver curDriver = layDown.actor.jobs.curDriver;
				curDriver.layingDown = true;
				curDriver.layingDownBed = (Building_Bed)layDown.actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
				curDriver.asleep = false;
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
						if (RoomQuery.RoomAt(actor.Position).TouchesMapEdge)
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptOutside, null);
						}
						if (building_Bed == null || building_Bed.CostListAdjusted().Count == 0)
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptOnGround, null);
						}
						if (GenTemperature.GetTemperatureForCell(actor.Position) < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
						{
							actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.SleptInCold, null);
						}
						if (GenTemperature.GetTemperatureForCell(actor.Position) > actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null))
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
						float num2 = actor.health.capacities.GetEfficiency(PawnCapacityDefOf.BloodPumping) * actor.health.capacities.GetEfficiency(PawnCapacityDefOf.Metabolism) * actor.health.capacities.GetEfficiency(PawnCapacityDefOf.Breathing);
						num = 0.7f * num + 0.3f * num * num2;
						actor.needs.rest.TickResting(num);
					}
				}
				if (building_Bed != null && gainRestAndHealth && Find.TickManager.TicksGame % building_Bed.def.building.bed_healTickInterval == 0 && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>() && actor.needs.food != null && !actor.needs.food.Starving)
				{
					BodyPartRecord part = actor.health.hediffSet.GetNaturallyHealingInjuredParts().RandomElement<BodyPartRecord>();
					List<HediffDef> healHediff = (from def in DefDatabase<HediffDef>.AllDefs
					where def.naturallyHealed
					select def).ToList<HediffDef>();
					BodyPartDamageInfo value = new BodyPartDamageInfo(part, false, healHediff);
					actor.TakeDamage(new DamageInfo(DamageDefOf.HealInjury, 1, null, new BodyPartDamageInfo?(value), null));
					if (PawnUtility.ShouldSendNotificationAbout(actor) && !actor.health.HasHediffsNeedingTendByColony(false) && !actor.health.PrefersMedicalRest && !actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						Messages.Message("MessageFullyHealed".Translate(new object[]
						{
							actor.LabelCap
						}), actor, MessageSound.Benefit);
					}
				}
				if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged())
				{
					if (curDriver.asleep)
					{
						MoteMaker.ThrowMetaIcon(actor.Position, ThingDefOf.Mote_SleepZ);
					}
					if (gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
					{
						MoteMaker.ThrowMetaIcon(actor.Position, ThingDefOf.Mote_HealingCross);
					}
				}
				if (actor.ownership != null && building_Bed != null && !building_Bed.Medical && !building_Bed.owners.Contains(actor))
				{
					if (actor.Downed)
					{
						actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, 1);
					}
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					return;
				}
				if (lookForOtherJobs && actor.IsHashIntervalTick(211))
				{
					wokeUpVoluntarily = true;
					actor.jobs.CheckForJobOverride();
					wokeUpVoluntarily = false;
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
				if (!wokeUpVoluntarily && curDriver.asleep && !actor.Dead && actor.needs.rest != null && actor.needs.rest.CurLevel < 0.75f && actor.needs.mood != null)
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
