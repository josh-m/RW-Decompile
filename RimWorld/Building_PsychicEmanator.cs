using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	internal class Building_PsychicEmanator : Building_CrashedShipPart
	{
		private const int DroneLevelIncreaseInterval = 150000;

		private const int AnimalInsaneRadius = 25;

		private int ticksToInsanityPulse;

		private int ticksToIncreaseDroneLevel;

		public PsychicDroneLevel droneLevel = PsychicDroneLevel.BadLow;

		private static readonly IntRange InsanityPulseInterval = new IntRange(60000, 150000);

		protected override float PlantHarmRange
		{
			get
			{
				return Mathf.Min(3f + 40f * ((float)this.age / 60000f), 20f);
			}
		}

		public override void SpawnSetup(Map map)
		{
			base.SpawnSetup(map);
			this.ticksToInsanityPulse = Building_PsychicEmanator.InsanityPulseInterval.RandomInRange;
			this.ticksToIncreaseDroneLevel = 150000;
			if (map == Find.VisibleMap)
			{
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.LookValue<int>(ref this.ticksToInsanityPulse, "ticksToInsanityPulse", 0, false);
			Scribe_Values.LookValue<int>(ref this.ticksToIncreaseDroneLevel, "ticksToIncreaseDroneLevel", 0, false);
			Scribe_Values.LookValue<PsychicDroneLevel>(ref this.droneLevel, "droneLevel", PsychicDroneLevel.None, false);
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.GetInspectString());
			string text = "Error";
			switch (this.droneLevel)
			{
			case PsychicDroneLevel.BadLow:
				text = "PsychicDroneLevelLow".Translate();
				break;
			case PsychicDroneLevel.BadMedium:
				text = "PsychicDroneLevelMedium".Translate();
				break;
			case PsychicDroneLevel.BadHigh:
				text = "PsychicDroneLevelHigh".Translate();
				break;
			case PsychicDroneLevel.BadExtreme:
				text = "PsychicDroneLevelExtreme".Translate();
				break;
			}
			stringBuilder.AppendLine("PsychicDroneLevel".Translate(new object[]
			{
				text
			}));
			return stringBuilder.ToString();
		}

		public override void Tick()
		{
			base.Tick();
			this.ticksToInsanityPulse--;
			if (this.ticksToInsanityPulse <= 0)
			{
				this.DoAnimalInsanityPulse();
				this.ticksToInsanityPulse = Building_PsychicEmanator.InsanityPulseInterval.RandomInRange;
			}
			this.ticksToIncreaseDroneLevel--;
			if (this.ticksToIncreaseDroneLevel <= 0)
			{
				this.IncreaseDroneLevel();
				this.ticksToIncreaseDroneLevel = 150000;
			}
		}

		private void IncreaseDroneLevel()
		{
			if (this.droneLevel == PsychicDroneLevel.BadExtreme)
			{
				return;
			}
			this.droneLevel += 1;
			string text = "LetterPsychicDroneLevelIncreased".Translate();
			Find.LetterStack.ReceiveLetter("LetterLabelPsychicDroneLevelIncreased".Translate(), text, LetterType.BadNonUrgent, null);
			if (base.Map == Find.VisibleMap)
			{
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
			}
		}

		private void DoAnimalInsanityPulse()
		{
			IEnumerable<Pawn> enumerable = from p in base.Map.mapPawns.AllPawnsSpawned
			where p.RaceProps.Animal && p.Position.InHorDistOf(base.Position, 25f)
			select p;
			foreach (Pawn current in enumerable)
			{
				current.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, false, false, null);
			}
			Messages.Message("MessageAnimalInsanityPulse".Translate(), this, MessageSound.Negative);
			if (base.Map == Find.VisibleMap)
			{
				Find.CameraDriver.shaker.DoShake(4f);
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
			}
		}
	}
}
