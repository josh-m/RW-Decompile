using RimWorld;
using System;
using System.Text;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class HediffComp_TendDuration : HediffComp_SeverityPerDay
	{
		public int tendTick = -999999;

		public float tendQuality;

		private int tendedCount;

		private const float TendQualityRandomVariance = 0.25f;

		private static readonly Color UntendedColor;

		private static readonly Texture2D TendedIcon_Need_General;

		private static readonly Texture2D TendedIcon_Well_General;

		private static readonly Texture2D TendedIcon_Well_Injury;

		public HediffCompProperties_TendDuration TProps
		{
			get
			{
				return (HediffCompProperties_TendDuration)this.props;
			}
		}

		public override bool CompShouldRemove
		{
			get
			{
				return base.CompShouldRemove || (this.TProps.disappearsAtTendedCount >= 0 && this.tendedCount >= this.TProps.disappearsAtTendedCount);
			}
		}

		public bool IsTended
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					return false;
				}
				if (this.TProps.tendDuration > 0)
				{
					return Find.TickManager.TicksGame <= this.tendTick + this.TProps.tendDuration;
				}
				return this.tendTick > 0;
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				if (this.parent.IsOld())
				{
					return null;
				}
				StringBuilder stringBuilder = new StringBuilder();
				if (!this.IsTended)
				{
					if (!base.Pawn.Dead && this.parent.TendableNow)
					{
						stringBuilder.AppendLine("NeedsTendingNow".Translate());
					}
				}
				else
				{
					string text;
					if (this.parent.Part != null && this.parent.Part.def.IsSolid(this.parent.Part, base.Pawn.health.hediffSet.hediffs))
					{
						text = this.TProps.labelSolidTendedWell;
					}
					else if (this.parent.Part != null && this.parent.Part.depth == BodyPartDepth.Inside)
					{
						text = this.TProps.labelTendedWellInner;
					}
					else
					{
						text = this.TProps.labelTendedWell;
					}
					if (text != null)
					{
						stringBuilder.AppendLine(string.Concat(new string[]
						{
							text.CapitalizeFirst(),
							" (",
							"Quality".Translate().ToLower(),
							" ",
							this.tendQuality.ToStringPercent("F0"),
							")"
						}));
					}
					if (!base.Pawn.Dead && this.TProps.tendDuration > 0)
					{
						int numTicks = this.tendTick + this.TProps.tendDuration - Find.TickManager.TicksGame;
						string text2 = numTicks.ToStringTicksToPeriod(true, false, true);
						if ("NextTendIn".CanTranslate())
						{
							text2 = "NextTendIn".Translate(new object[]
							{
								text2
							});
						}
						else
						{
							text2 = "NextTreatmentIn".Translate(new object[]
							{
								text2
							});
						}
						stringBuilder.AppendLine(text2);
					}
				}
				return stringBuilder.ToString().TrimEndNewlines();
			}
		}

		public override TextureAndColor CompStateIcon
		{
			get
			{
				if (this.parent is Hediff_Injury)
				{
					if (this.IsTended && !this.parent.IsOld())
					{
						Color color = Color.Lerp(HediffComp_TendDuration.UntendedColor, Color.white, Mathf.Clamp01(this.tendQuality));
						return new TextureAndColor(HediffComp_TendDuration.TendedIcon_Well_Injury, color);
					}
				}
				else if (!(this.parent is Hediff_MissingPart))
				{
					if (!this.parent.FullyImmune())
					{
						if (this.IsTended)
						{
							Color color2 = Color.Lerp(HediffComp_TendDuration.UntendedColor, Color.white, Mathf.Clamp01(this.tendQuality));
							return new TextureAndColor(HediffComp_TendDuration.TendedIcon_Well_General, color2);
						}
						return HediffComp_TendDuration.TendedIcon_Need_General;
					}
				}
				return TextureAndColor.None;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look<int>(ref this.tendTick, "tendTick", -999999, false);
			Scribe_Values.Look<float>(ref this.tendQuality, "tendQuality", 0f, false);
			Scribe_Values.Look<int>(ref this.tendedCount, "tendedCount", 0, false);
		}

		protected override float SeverityChangePerDay()
		{
			if (this.IsTended)
			{
				return this.TProps.severityPerDayTended * this.tendQuality;
			}
			return 0f;
		}

		public override void CompTended(float quality, int batchPosition = 0)
		{
			this.tendQuality = Mathf.Clamp01(quality + Rand.Range(-0.25f, 0.25f));
			this.tendTick = Find.TickManager.TicksGame;
			this.tendedCount++;
			if (batchPosition == 0 && base.Pawn.Spawned)
			{
				string text = string.Concat(new string[]
				{
					"TextMote_Tended".Translate(new object[]
					{
						this.parent.Label
					}).CapitalizeFirst(),
					"\n",
					"Quality".Translate(),
					" ",
					this.tendQuality.ToStringPercent()
				});
				MoteMaker.ThrowText(base.Pawn.DrawPos, base.Pawn.Map, text, Color.white, 3.65f);
			}
			base.Pawn.health.Notify_HediffChanged(this.parent);
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.IsTended)
			{
				stringBuilder.AppendLine("tendQuality: " + this.tendQuality.ToStringPercent());
				if (this.TProps.tendDuration > 0)
				{
					int num = Find.TickManager.TicksGame - this.tendTick;
					stringBuilder.AppendLine("ticks since tend: " + num);
					stringBuilder.AppendLine("tend duration passed: " + ((float)num / (float)this.TProps.tendDuration).ToStringPercent());
					stringBuilder.AppendLine("severity change per day: " + (this.TProps.severityPerDayTended * this.tendQuality).ToString());
				}
			}
			else
			{
				stringBuilder.AppendLine("untended");
			}
			if (this.TProps.disappearsAtTendedCount >= 0)
			{
				stringBuilder.AppendLine(string.Concat(new object[]
				{
					"tended count: ",
					this.tendedCount,
					" / ",
					this.TProps.disappearsAtTendedCount
				}));
			}
			return stringBuilder.ToString().Trim();
		}

		static HediffComp_TendDuration()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ColorInt colorInt = new ColorInt(116, 101, 72);
			HediffComp_TendDuration.UntendedColor = colorInt.ToColor;
			HediffComp_TendDuration.TendedIcon_Need_General = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedNeed", true);
			HediffComp_TendDuration.TendedIcon_Well_General = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedWell", true);
			HediffComp_TendDuration.TendedIcon_Well_Injury = ContentFinder<Texture2D>.Get("UI/Icons/Medical/BandageWell", true);
		}
	}
}
