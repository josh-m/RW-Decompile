using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class Need_Rest : Need
	{
		private int lastRestTick = -999;

		private float lastRestEffectiveness = 1f;

		private int ticksAtZero;

		private const float FullSleepHours = 10.5f;

		public const float BaseRestGainPerTick = 3.8095237E-05f;

		private const float BaseRestFallPerTick = 1.58333332E-05f;

		public const float ThreshTired = 0.28f;

		public const float ThreshVeryTired = 0.14f;

		public const float DefaultFallAsleepMaxLevel = 0.75f;

		public const float DefaultNaturalWakeThreshold = 1f;

		public const float CanWakeThreshold = 0.2f;

		private const float BaseInvoluntarySleepMTBDays = 0.25f;

		public RestCategory CurCategory
		{
			get
			{
				if (this.CurLevel < 0.01f)
				{
					return RestCategory.Exhausted;
				}
				if (this.CurLevel < 0.14f)
				{
					return RestCategory.VeryTired;
				}
				if (this.CurLevel < 0.28f)
				{
					return RestCategory.Tired;
				}
				return RestCategory.Rested;
			}
		}

		public float RestFallPerTick
		{
			get
			{
				switch (this.CurCategory)
				{
				case RestCategory.Rested:
					return 1.58333332E-05f * this.RestFallFactor;
				case RestCategory.Tired:
					return 1.58333332E-05f * this.RestFallFactor * 0.7f;
				case RestCategory.VeryTired:
					return 1.58333332E-05f * this.RestFallFactor * 0.3f;
				case RestCategory.Exhausted:
					return 1.58333332E-05f * this.RestFallFactor * 0.6f;
				default:
					return 999f;
				}
			}
		}

		private float RestFallFactor
		{
			get
			{
				return this.pawn.health.hediffSet.RestFallFactor;
			}
		}

		public override int GUIChangeArrow
		{
			get
			{
				if (this.Resting)
				{
					return 1;
				}
				return -1;
			}
		}

		public int TicksAtZero
		{
			get
			{
				return this.ticksAtZero;
			}
		}

		private bool Resting
		{
			get
			{
				return Find.TickManager.TicksGame < this.lastRestTick + 2;
			}
		}

		public Need_Rest(Pawn pawn) : base(pawn)
		{
			this.threshPercents = new List<float>();
			this.threshPercents.Add(0.28f);
			this.threshPercents.Add(0.14f);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.ticksAtZero, "ticksAtZero", 0, false);
		}

		public override void SetInitialLevel()
		{
			this.CurLevel = Rand.Range(0.9f, 1f);
		}

		public override void NeedInterval()
		{
			if (!base.IsFrozen)
			{
				if (this.Resting)
				{
					this.CurLevel += 0.00571428565f * this.lastRestEffectiveness;
				}
				else
				{
					this.CurLevel -= this.RestFallPerTick * 150f;
				}
			}
			if (this.CurLevel < 0.0001f)
			{
				this.ticksAtZero += 150;
			}
			else
			{
				this.ticksAtZero = 0;
			}
			if (this.ticksAtZero > 1000 && this.pawn.Spawned)
			{
				float mtb;
				if (this.ticksAtZero < 15000)
				{
					mtb = 0.25f;
				}
				else if (this.ticksAtZero < 30000)
				{
					mtb = 0.125f;
				}
				else if (this.ticksAtZero < 45000)
				{
					mtb = 0.0833333358f;
				}
				else
				{
					mtb = 0.0625f;
				}
				if (Rand.MTBEventOccurs(mtb, 60000f, 150f) && (this.pawn.CurJob == null || this.pawn.CurJob.def != JobDefOf.LayDown))
				{
					this.pawn.jobs.StartJob(new Job(JobDefOf.LayDown, this.pawn.Position), JobCondition.InterruptForced, null, false, true, null, new JobTag?(JobTag.SatisfyingNeeds), false);
					if (this.pawn.InMentalState)
					{
						this.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
					}
					if (PawnUtility.ShouldSendNotificationAbout(this.pawn))
					{
						Messages.Message("MessageInvoluntarySleep".Translate(new object[]
						{
							this.pawn.LabelShort
						}), this.pawn, MessageTypeDefOf.NegativeEvent);
					}
					TaleRecorder.RecordTale(TaleDefOf.Exhausted, new object[]
					{
						this.pawn
					});
				}
			}
		}

		public void TickResting(float restEffectiveness)
		{
			this.lastRestTick = Find.TickManager.TicksGame;
			this.lastRestEffectiveness = restEffectiveness;
		}
	}
}
