using RimWorld;
using System;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class HediffComp_Tendable : HediffComp_SeverityPerDay
	{
		public const float TendedWellThreshold = 1f;

		private const float TendQualityVariance = 0.4f;

		public int tendTick = -999999;

		public float tendQuality;

		private int tendedCount;

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (!this.IsTended && !this.parent.FullyImmune() && !(this.parent is Hediff_Injury) && !(this.parent is Hediff_MissingPart))
				{
					return "UntreatedLower".Translate();
				}
				return null;
			}
		}

		public override bool CompShouldRemove
		{
			get
			{
				return base.CompShouldRemove || (this.props.disappearsAtTendedCount >= 0 && this.tendedCount >= this.props.disappearsAtTendedCount);
			}
		}

		public bool IsTended
		{
			get
			{
				if (Current.ProgramState != ProgramState.MapPlaying)
				{
					return false;
				}
				if (this.props.tendDuration > 0)
				{
					return Find.TickManager.TicksGame <= this.tendTick + this.props.tendDuration;
				}
				return this.tendTick > 0;
			}
		}

		public bool IsTendedWell
		{
			get
			{
				return this.IsTended && this.tendQuality >= 1f;
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (!this.parent.IsOld() && this.parent is Hediff_Injury && ((!this.IsTended && !base.Def.naturallyHealed) || this.parent.NotNaturallyHealingBecauseNeedsTending()))
				{
					stringBuilder.AppendLine("InjuryRequiresTreatment".Translate());
				}
				string text = null;
				if (this.IsTended && !this.parent.IsOld())
				{
					if (this.parent.Part != null && this.parent.Part.def.IsSolid(this.parent.Part, base.Pawn.health.hediffSet.hediffs))
					{
						if (this.parent.IsTendedWell())
						{
							text = this.props.labelSolidTendedWell;
						}
						else
						{
							text = this.props.labelSolidTended;
						}
					}
					else if (this.parent.Part != null && this.parent.Part.depth == BodyPartDepth.Inside)
					{
						if (this.parent.IsTendedWell())
						{
							text = this.props.labelTendedWellInner;
						}
						else
						{
							text = this.props.labelTendedInner;
						}
					}
					else if (this.parent.IsTendedWell())
					{
						text = this.props.labelTendedWell;
					}
					else
					{
						text = this.props.labelTended;
					}
				}
				if (text != null)
				{
					stringBuilder.AppendLine(text.CapitalizeFirst());
				}
				if (this.IsTended && this.props.tendDuration > 0)
				{
					int numTicks = this.tendTick + this.props.tendDuration - Find.TickManager.TicksGame;
					stringBuilder.AppendLine("NextTreatmentIn".Translate(new object[]
					{
						numTicks.ToStringTicksToPeriod(true)
					}));
				}
				return stringBuilder.ToString().TrimEndNewlines();
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.tendTick, "tendTick", -999999, false);
			Scribe_Values.LookValue<float>(ref this.tendQuality, "tendQuality", 0f, false);
			Scribe_Values.LookValue<int>(ref this.tendedCount, "tendedCount", 0, false);
		}

		protected override float SeverityChangePerDay()
		{
			if (this.IsTended)
			{
				return this.props.severityPerDayTendedOffset * this.tendQuality;
			}
			return 0f;
		}

		public override void CompTended(float quality, int batchPosition = 0)
		{
			float num = Mathf.Clamp01(quality + Rand.Range(-0.4f, 0.4f));
			this.tendQuality = num;
			this.tendTick = Find.TickManager.TicksGame;
			this.tendedCount++;
			HediffComp_Infecter hediffComp_Infecter = this.parent.TryGetComp<HediffComp_Infecter>();
			if (hediffComp_Infecter != null && base.Pawn.Spawned)
			{
				Room room = base.Pawn.GetRoom();
				if (room != null)
				{
					hediffComp_Infecter.infectionChanceFactor = room.GetStat(RoomStatDefOf.InfectionChanceFactor);
				}
			}
			if (batchPosition == 0 && base.Pawn.Spawned)
			{
				string text = this.parent.LabelCap + "\n" + "TreatQuality".Translate(new object[]
				{
					quality.ToStringPercent()
				});
				Color color = Color.white;
				if (this.parent is Hediff_Injury)
				{
					if (num < 1f)
					{
						text = text + "\n" + "PoorResult".Translate();
						ColorInt colorInt = new ColorInt(255, 230, 215);
						color = colorInt.ToColor;
					}
					else
					{
						text = text + "\n" + "GoodResult".Translate();
					}
				}
				MoteMaker.ThrowText(this.parent.pawn.DrawPos, text, color, 3.65f);
			}
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.IsTended)
			{
				stringBuilder.AppendLine("tendQuality: " + this.tendQuality.ToStringPercent());
				if (this.props.tendDuration > 0)
				{
					int num = Find.TickManager.TicksGame - this.tendTick;
					stringBuilder.AppendLine("ticks since tend: " + num);
					stringBuilder.AppendLine("tend duration passed: " + ((float)num / (float)this.props.tendDuration).ToStringPercent());
				}
			}
			else
			{
				stringBuilder.AppendLine("untended");
			}
			if (this.props.disappearsAtTendedCount >= 0)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"tended count: ",
					this.tendedCount,
					" / ",
					this.props.disappearsAtTendedCount
				}));
			}
			return stringBuilder.ToString().Trim();
		}
	}
}
