using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class HediffComp_Infecter : HediffComp
	{
		private float infectionChanceFactor = 1f;

		private int ticksUntilInfect = -1;

		private bool alreadyCausedInfection;

		public HediffCompProperties_Infecter Props
		{
			get
			{
				return (HediffCompProperties_Infecter)this.props;
			}
		}

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			float num = this.Props.infectionChance;
			if (base.Pawn.RaceProps.Animal)
			{
				num *= 0.15f;
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

		public override void CompTended(float quality, int batchPosition = 0)
		{
			HediffComp_Infecter hediffComp_Infecter = this.parent.TryGetComp<HediffComp_Infecter>();
			if (hediffComp_Infecter != null && base.Pawn.Spawned)
			{
				Room room = base.Pawn.GetRoom();
				if (room != null)
				{
					hediffComp_Infecter.infectionChanceFactor = room.GetStat(RoomStatDefOf.InfectionChanceFactor);
				}
			}
		}

		private void CheckMakeInfection()
		{
			this.ticksUntilInfect = -1;
			HediffComp_TendDuration hediffComp_TendDuration = this.parent.TryGetComp<HediffComp_TendDuration>();
			if (hediffComp_TendDuration != null && hediffComp_TendDuration.IsTended)
			{
				if (Rand.Value < this.infectionChanceFactor)
				{
					return;
				}
				float num = Mathf.Clamp(hediffComp_TendDuration.tendQuality + 0.3f, 0f, 0.97f);
				if (Rand.Value < num)
				{
					return;
				}
			}
			if (base.Pawn.health.immunity.DiseaseContractChanceFactor(HediffDefOf.WoundInfection, this.parent.Part) <= 0.001f)
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
