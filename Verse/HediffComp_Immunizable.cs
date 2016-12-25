using System;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class HediffComp_Immunizable : HediffComp_SeverityPerDay
	{
		private float severityPerDayNotImmuneRandomFactor = 1f;

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

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);
			this.severityPerDayNotImmuneRandomFactor = this.props.severityPerDayNotImmuneRandomFactor.RandomInRange;
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.LookValue<float>(ref this.severityPerDayNotImmuneRandomFactor, "severityPerDayNotImmuneRandomFactor", 1f, false);
		}

		protected override float SeverityChangePerDay()
		{
			return (!this.FullyImmune) ? (this.props.severityPerDayNotImmune * this.severityPerDayNotImmuneRandomFactor) : this.props.severityPerDayImmune;
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
