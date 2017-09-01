using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Lovin : JobDriver
	{
		private const int TicksBetweenHeartMotes = 100;

		private int ticksLeft;

		private TargetIndex PartnerInd = TargetIndex.A;

		private TargetIndex BedInd = TargetIndex.B;

		private static readonly SimpleCurve LovinIntervalHoursFromAgeCurve = new SimpleCurve
		{
			{
				new CurvePoint(16f, 1.5f),
				true
			},
			{
				new CurvePoint(22f, 1.5f),
				true
			},
			{
				new CurvePoint(30f, 4f),
				true
			},
			{
				new CurvePoint(50f, 12f),
				true
			},
			{
				new CurvePoint(75f, 36f),
				true
			}
		};

		private Pawn Partner
		{
			get
			{
				return (Pawn)((Thing)base.CurJob.GetTarget(this.PartnerInd));
			}
		}

		private Building_Bed Bed
		{
			get
			{
				return (Building_Bed)((Thing)base.CurJob.GetTarget(this.BedInd));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(this.BedInd);
			this.FailOnDespawnedOrNull(this.PartnerInd);
			this.FailOn(() => !this.<>f__this.Partner.health.capacities.CanBeAwake);
			this.KeepLyingDown(this.BedInd);
			yield return Toils_Reserve.Reserve(this.PartnerInd, 1, -1, null);
			yield return Toils_Reserve.Reserve(this.BedInd, this.Bed.SleepingSlotsCount, 0, null);
			yield return Toils_Bed.ClaimBedIfNonMedical(this.BedInd, TargetIndex.None);
			yield return Toils_Bed.GotoBed(this.BedInd);
			yield return new Toil
			{
				initAction = delegate
				{
					if (this.<>f__this.Partner.CurJob == null || this.<>f__this.Partner.CurJob.def != JobDefOf.Lovin)
					{
						Job newJob = new Job(JobDefOf.Lovin, this.<>f__this.pawn, this.<>f__this.Bed);
						this.<>f__this.Partner.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null);
						this.<>f__this.ticksLeft = (int)(2500f * Mathf.Clamp(Rand.Range(0.1f, 1.1f), 0.1f, 2f));
					}
					else
					{
						this.<>f__this.ticksLeft = 9999999;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			Toil doLovin = Toils_LayDown.LayDown(this.BedInd, true, false, false, false);
			doLovin.FailOn(() => this.<>f__this.Partner.CurJob == null || this.<>f__this.Partner.CurJob.def != JobDefOf.Lovin);
			doLovin.AddPreTickAction(delegate
			{
				this.<>f__this.ticksLeft--;
				if (this.<>f__this.ticksLeft <= 0)
				{
					this.<>f__this.ReadyForNextToil();
				}
				else if (this.<>f__this.pawn.IsHashIntervalTick(100))
				{
					MoteMaker.ThrowMetaIcon(this.<>f__this.pawn.Position, this.<>f__this.pawn.Map, ThingDefOf.Mote_Heart);
				}
			});
			doLovin.AddFinishAction(delegate
			{
				Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.GotSomeLovin);
				this.<>f__this.pawn.needs.mood.thoughts.memories.TryGainMemory(newThought, this.<>f__this.Partner);
				this.<>f__this.pawn.mindState.canLovinTick = Find.TickManager.TicksGame + this.<>f__this.GenerateRandomMinTicksToNextLovin(this.<>f__this.pawn);
			});
			doLovin.socialMode = RandomSocialMode.Off;
			yield return doLovin;
		}

		private int GenerateRandomMinTicksToNextLovin(Pawn pawn)
		{
			if (DebugSettings.alwaysDoLovin)
			{
				return 100;
			}
			float num = JobDriver_Lovin.LovinIntervalHoursFromAgeCurve.Evaluate(pawn.ageTracker.AgeBiologicalYearsFloat);
			num = Rand.Gaussian(num, 0.3f);
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			return (int)(num * 2500f);
		}
	}
}
