using System;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class Hediff_Injury : HediffWithComps
	{
		private static readonly Color OldInjuryColor = new Color(0.72f, 0.72f, 0.72f);

		public override int UIGroupKey
		{
			get
			{
				int num = base.UIGroupKey;
				if (this.IsTended())
				{
					num = Gen.HashCombineInt(num, 152235495);
				}
				return num;
			}
		}

		public override string LabelBase
		{
			get
			{
				HediffComp_GetsOld hediffComp_GetsOld = this.TryGetComp<HediffComp_GetsOld>();
				if (hediffComp_GetsOld != null && hediffComp_GetsOld.IsOld)
				{
					if (base.Part.def.IsDelicate && !hediffComp_GetsOld.Props.instantlyOldLabel.NullOrEmpty())
					{
						return hediffComp_GetsOld.Props.instantlyOldLabel;
					}
					if (!hediffComp_GetsOld.Props.oldLabel.NullOrEmpty())
					{
						return hediffComp_GetsOld.Props.oldLabel;
					}
				}
				return base.LabelBase;
			}
		}

		public override string LabelInBrackets
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.LabelInBrackets);
				if (this.sourceHediffDef != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(this.sourceHediffDef.label);
				}
				else if (this.source != null)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(this.source.label);
					if (this.sourceBodyPartGroup != null)
					{
						stringBuilder.Append(" - ");
						stringBuilder.Append(this.sourceBodyPartGroup.label);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public override Color LabelColor
		{
			get
			{
				if (this.IsOld())
				{
					return Hediff_Injury.OldInjuryColor;
				}
				return Color.white;
			}
		}

		public override string SeverityLabel
		{
			get
			{
				if (this.Severity == 0f)
				{
					return null;
				}
				return this.Severity.ToString("0.##");
			}
		}

		public override float SummaryHealthPercentImpact
		{
			get
			{
				if (this.IsOld())
				{
					return 0f;
				}
				return this.Severity / (75f * this.pawn.HealthScale);
			}
		}

		public override float PainOffset
		{
			get
			{
				if (this.pawn.Dead || this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part) || this.causesNoPain)
				{
					return 0f;
				}
				HediffComp_GetsOld hediffComp_GetsOld = this.TryGetComp<HediffComp_GetsOld>();
				if (hediffComp_GetsOld != null && hediffComp_GetsOld.IsOld)
				{
					return this.Severity * this.def.injuryProps.averagePainPerSeverityOld * hediffComp_GetsOld.painFactor;
				}
				return this.Severity * this.def.injuryProps.painPerSeverity;
			}
		}

		public override float BleedRate
		{
			get
			{
				if (this.pawn.Dead)
				{
					return 0f;
				}
				if (this.BleedingStoppedDueToAge)
				{
					return 0f;
				}
				if (base.Part.def.IsSolid(base.Part, this.pawn.health.hediffSet.hediffs) || this.IsTended() || this.IsOld())
				{
					return 0f;
				}
				if (this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part))
				{
					return 0f;
				}
				float num = this.Severity * this.def.injuryProps.bleedRate;
				if (base.Part != null)
				{
					num *= base.Part.def.bleedingRateMultiplier;
				}
				return num;
			}
		}

		private int AgeTicksToStopBleeding
		{
			get
			{
				int num = 90000;
				float t = Mathf.Clamp(Mathf.InverseLerp(1f, 30f, this.Severity), 0f, 1f);
				return num + Mathf.RoundToInt(Mathf.Lerp(0f, 90000f, t));
			}
		}

		private bool BleedingStoppedDueToAge
		{
			get
			{
				return this.ageTicks >= this.AgeTicksToStopBleeding;
			}
		}

		public override void Tick()
		{
			bool bleedingStoppedDueToAge = this.BleedingStoppedDueToAge;
			base.Tick();
			bool bleedingStoppedDueToAge2 = this.BleedingStoppedDueToAge;
			if (bleedingStoppedDueToAge != bleedingStoppedDueToAge2)
			{
				this.pawn.health.Notify_HediffChanged(this);
			}
		}

		public override void Heal(float amount)
		{
			this.Severity -= amount;
			if (this.comps != null)
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					this.comps[i].CompPostInjuryHeal(amount);
				}
			}
			this.pawn.health.Notify_HediffChanged(this);
		}

		public override bool TryMergeWith(Hediff other)
		{
			Hediff_Injury hediff_Injury = other as Hediff_Injury;
			return hediff_Injury != null && hediff_Injury.def == this.def && hediff_Injury.Part == base.Part && !hediff_Injury.IsTended() && !hediff_Injury.IsOld() && !this.IsTended() && !this.IsOld() && this.def.injuryProps.canMerge && base.TryMergeWith(other);
		}
	}
}
