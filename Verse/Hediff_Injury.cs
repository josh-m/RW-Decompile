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
					if (this.IsTendedWell())
					{
						num = Gen.HashCombineInt(num, 135738120);
					}
				}
				return num;
			}
		}

		public override string LabelBase
		{
			get
			{
				HediffComp_GetsOld hediffComp_GetsOld = this.TryGetComp<HediffComp_GetsOld>();
				if (hediffComp_GetsOld == null || !hediffComp_GetsOld.IsOld)
				{
					return this.def.LabelCap;
				}
				if (base.Part.def.IsDelicate && !hediffComp_GetsOld.props.instantlyOldLabel.NullOrEmpty())
				{
					return hediffComp_GetsOld.props.instantlyOldLabel.CapitalizeFirst();
				}
				if (!hediffComp_GetsOld.props.oldLabel.NullOrEmpty())
				{
					return hediffComp_GetsOld.props.oldLabel.CapitalizeFirst();
				}
				return this.def.LabelCap;
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

		public override string DamageLabel
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
				if (this.pawn.health.Dead || this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(base.Part) || this.causesNoPain)
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
				if (this.pawn.health.Dead)
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
				if (this.TicksAfterNoLongerBleedingPassed)
				{
					return 0f;
				}
				return this.Severity * this.def.injuryProps.bleeding;
			}
		}

		public override float MaxBleeding
		{
			get
			{
				return base.Part.def.GetMaxHealth(this.pawn);
			}
		}

		private int TicksAfterNoLongerBleeding
		{
			get
			{
				int num = 80000;
				float t = Mathf.Clamp(Mathf.InverseLerp(1f, 30f, this.Severity), 0f, 1f);
				return num + Mathf.RoundToInt(Mathf.Lerp(0f, 80000f, t));
			}
		}

		private bool TicksAfterNoLongerBleedingPassed
		{
			get
			{
				return this.ageTicks >= this.TicksAfterNoLongerBleeding;
			}
		}

		public override void Tick()
		{
			bool ticksAfterNoLongerBleedingPassed = this.TicksAfterNoLongerBleedingPassed;
			base.Tick();
			bool ticksAfterNoLongerBleedingPassed2 = this.TicksAfterNoLongerBleedingPassed;
			if (ticksAfterNoLongerBleedingPassed != ticksAfterNoLongerBleedingPassed2)
			{
				this.pawn.health.Notify_HediffChanged(this);
			}
		}

		public override void DirectHeal(float amount)
		{
			if (amount <= 0f)
			{
				return;
			}
			if (this.FullyHealableOnlyByTend() && this.Severity - amount <= 2f)
			{
				amount = this.Severity - 2f;
			}
			if (amount <= 0f)
			{
				return;
			}
			this.Severity -= amount;
		}

		public override bool TryMergeWith(Hediff other)
		{
			Hediff_Injury hediff_Injury = other as Hediff_Injury;
			return hediff_Injury != null && hediff_Injury.def == this.def && hediff_Injury.Part == base.Part && !hediff_Injury.IsTended() && !hediff_Injury.IsOld() && !this.IsTended() && !this.IsOld() && this.def.injuryProps.canMerge && base.TryMergeWith(other);
		}
	}
}
