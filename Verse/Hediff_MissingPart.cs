using System;
using System.Text;

namespace Verse
{
	public class Hediff_MissingPart : HediffWithComps
	{
		public HediffDef lastInjury;

		private bool isFreshInt;

		public override float SummaryHealthPercentImpact
		{
			get
			{
				if (!this.IsFresh)
				{
					return 0f;
				}
				return (float)base.Part.def.hitPoints / (75f * this.pawn.HealthScale);
			}
		}

		public override bool ShouldRemove
		{
			get
			{
				return false;
			}
		}

		public override string LabelBase
		{
			get
			{
				if (this.lastInjury != null && this.lastInjury.injuryProps.useRemovedLabel)
				{
					return "RemovedBodyPart".Translate();
				}
				if (this.lastInjury == null || base.Part.depth == BodyPartDepth.Inside)
				{
					bool solid = base.Part.def.IsSolid(base.Part, this.pawn.health.hediffSet.hediffs);
					return HealthUtility.GetGeneralDestroyedPartLabel(base.Part, this.IsFresh, solid);
				}
				if (base.Part.def.useDestroyedOutLabel && !this.lastInjury.injuryProps.destroyedOutLabel.NullOrEmpty())
				{
					return this.lastInjury.injuryProps.destroyedOutLabel;
				}
				return this.lastInjury.injuryProps.destroyedLabel;
			}
		}

		public override string LabelInBrackets
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.LabelInBrackets);
				if (this.IsFresh)
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append("FreshMissingBodyPart".Translate());
				}
				return stringBuilder.ToString();
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
				if (!this.IsFresh)
				{
					return 0f;
				}
				if (this.ParentIsMissing)
				{
					return 0f;
				}
				return base.Part.def.GetMaxHealth(this.pawn) * this.def.injuryProps.bleedRate * base.Part.def.bleedingRateMultiplier;
			}
		}

		public override float PainOffset
		{
			get
			{
				if (this.pawn.Dead)
				{
					return 0f;
				}
				if (this.causesNoPain)
				{
					return 0f;
				}
				if (!this.IsFresh)
				{
					return 0f;
				}
				if (this.ParentIsMissing)
				{
					return 0f;
				}
				return base.Part.def.GetMaxHealth(this.pawn) * this.def.injuryProps.painPerSeverity;
			}
		}

		private bool ParentIsMissing
		{
			get
			{
				for (int i = 0; i < this.pawn.health.hediffSet.hediffs.Count; i++)
				{
					Hediff_MissingPart hediff_MissingPart = this.pawn.health.hediffSet.hediffs[i] as Hediff_MissingPart;
					if (hediff_MissingPart != null && hediff_MissingPart.Part == base.Part.parent)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool IsFresh
		{
			get
			{
				return Current.ProgramState != ProgramState.Entry && this.isFreshInt && base.Part.depth != BodyPartDepth.Inside && !this.TicksAfterMissingBodyPartNoLongerFreshPassed && !base.Part.def.IsSolid(base.Part, this.pawn.health.hediffSet.hediffs) && !this.ParentIsMissing;
			}
			set
			{
				this.isFreshInt = value;
			}
		}

		private bool TicksAfterMissingBodyPartNoLongerFreshPassed
		{
			get
			{
				return this.ageTicks >= 90000;
			}
		}

		public override void Tick()
		{
			bool ticksAfterMissingBodyPartNoLongerFreshPassed = this.TicksAfterMissingBodyPartNoLongerFreshPassed;
			base.Tick();
			bool ticksAfterMissingBodyPartNoLongerFreshPassed2 = this.TicksAfterMissingBodyPartNoLongerFreshPassed;
			if (ticksAfterMissingBodyPartNoLongerFreshPassed != ticksAfterMissingBodyPartNoLongerFreshPassed2)
			{
				this.pawn.health.Notify_HediffChanged(this);
			}
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			this.pawn.health.RestorePart(base.Part, this, false);
			for (int i = 0; i < base.Part.parts.Count; i++)
			{
				Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(this.def, this.pawn, null);
				hediff_MissingPart.IsFresh = false;
				hediff_MissingPart.lastInjury = this.lastInjury;
				hediff_MissingPart.Part = base.Part.parts[i];
				this.pawn.health.hediffSet.AddDirect(hediff_MissingPart, null);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.LookDef<HediffDef>(ref this.lastInjury, "lastInjury");
			Scribe_Values.LookValue<bool>(ref this.isFreshInt, "isFresh", false, false);
		}
	}
}
