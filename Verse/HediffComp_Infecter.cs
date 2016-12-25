using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class HediffComp_Infecter : HediffComp
	{
		public float infectionChanceFactor = 1f;

		private int ticksUntilInfect = -1;

		private bool alreadyCausedInfection;

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			float num = this.props.infectionChance;
			if (base.Pawn.RaceProps.Animal)
			{
				num *= 0.125f;
			}
			if (!this.alreadyCausedInfection && !this.parent.Part.def.IsSolid(this.parent.Part, base.Pawn.health.hediffSet.hediffs) && !base.Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(this.parent.Part) && !this.parent.IsOld() && Rand.Value <= num)
			{
				this.ticksUntilInfect = HealthTunings.InfectionDelayRange.RandomInRange;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.LookValue<float>(ref this.infectionChanceFactor, "infectionChanceFactor", 0f, false);
			Scribe_Values.LookValue<int>(ref this.ticksUntilInfect, "ticksUntilInfect", -1, false);
			Scribe_Values.LookValue<bool>(ref this.alreadyCausedInfection, "alreadyCausedInfection", false, false);
		}

		public override void CompPostTick()
		{
			if (!this.alreadyCausedInfection && this.ticksUntilInfect > 0)
			{
				this.ticksUntilInfect--;
				if (this.ticksUntilInfect == 0)
				{
					this.CheckMakeInfection();
				}
			}
		}

		private void CheckMakeInfection()
		{
			HediffComp_Tendable hediffComp_Tendable = this.parent.TryGetComp<HediffComp_Tendable>();
			this.ticksUntilInfect = -1;
			float num = 0f;
			if (hediffComp_Tendable != null && hediffComp_Tendable.IsTended)
			{
				num = hediffComp_Tendable.tendQuality;
			}
			num = Mathf.Clamp01(num / this.infectionChanceFactor);
			if (Rand.Value < num)
			{
				return;
			}
			if (base.Pawn.health.immunity.ChanceToGetDisease(HediffDefOf.WoundInfection, this.parent.Part) <= 0.001f)
			{
				return;
			}
			this.alreadyCausedInfection = true;
			base.Pawn.health.AddHediff(HediffDefOf.WoundInfection, this.parent.Part, null);
		}

		public override string CompDebugString()
		{
			if (this.alreadyCausedInfection)
			{
				return "already caused infection";
			}
			if (this.ticksUntilInfect <= 0)
			{
				return "no infection will appear";
			}
			return string.Concat(new object[]
			{
				"infection may appear after: ",
				this.ticksUntilInfect,
				" ticks (infection chance factor: ",
				this.infectionChanceFactor.ToString(),
				")"
			});
		}
	}
}
