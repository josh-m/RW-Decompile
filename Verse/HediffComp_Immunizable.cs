using System;
using System.Text;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class HediffComp_Immunizable : HediffComp_SeverityPerDay
	{
		private float severityPerDayNotImmuneRandomFactor = 1f;

		private static readonly Texture2D IconImmune = ContentFinder<Texture2D>.Get("UI/Icons/Medical/IconImmune", true);

		public HediffCompProperties_Immunizable Props
		{
			get
			{
				return (HediffCompProperties_Immunizable)this.props;
			}
		}

		public override string CompLabelInBracketsExtra
		{
			get
			{
				if (this.FullyImmune)
				{
					return "DevelopedImmunityLower".Translate();
				}
				return null;
			}
		}

		public override string CompTipStringExtra
		{
			get
			{
				if (base.Def.PossibleToDevelopImmunity())
				{
					return "Immunity".Translate() + ": " + (Mathf.Floor(this.Immunity * 100f) / 100f).ToStringPercent();
				}
				return null;
			}
		}

		public float Immunity
		{
			get
			{
				return base.Pawn.health.immunity.GetImmunity(base.Def);
			}
		}

		public bool FullyImmune
		{
			get
			{
				return this.Immunity >= 1f;
			}
		}

		public override TextureAndColor CompStateIcon
		{
			get
			{
				if (this.FullyImmune)
				{
					return HediffComp_Immunizable.IconImmune;
				}
				return TextureAndColor.None;
			}
		}

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);
			this.severityPerDayNotImmuneRandomFactor = this.Props.severityPerDayNotImmuneRandomFactor.RandomInRange;
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.LookValue<float>(ref this.severityPerDayNotImmuneRandomFactor, "severityPerDayNotImmuneRandomFactor", 1f, false);
		}

		protected override float SeverityChangePerDay()
		{
			return (!this.FullyImmune) ? (this.Props.severityPerDayNotImmune * this.severityPerDayNotImmuneRandomFactor) : this.Props.severityPerDayImmune;
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.CompDebugString());
			if (this.severityPerDayNotImmuneRandomFactor != 1f)
			{
				stringBuilder.AppendLine("severityPerDayNotImmuneRandomFactor: " + this.severityPerDayNotImmuneRandomFactor.ToString("0.##"));
			}
			if (!base.Pawn.Dead)
			{
				ImmunityRecord immunityRecord = base.Pawn.health.immunity.GetImmunityRecord(base.Def);
				if (immunityRecord != null)
				{
					stringBuilder.AppendLine("immunity change per day: " + (immunityRecord.ImmunityChangePerTick(base.Pawn, true, this.parent) * 60000f).ToString("F3"));
				}
			}
			return stringBuilder.ToString();
		}
	}
}
