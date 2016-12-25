using System;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Building_SteamGeyser : Building
	{
		private IntermittentSteamSprayer steamSprayer;

		public Building harvester;

		private Sustainer spraySustainer;

		private int spraySustainerStartTick = -999;

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			this.steamSprayer = new IntermittentSteamSprayer(this);
			this.steamSprayer.startSprayCallback = new Action(this.StartSpray);
			this.steamSprayer.endSprayCallback = new Action(this.EndSpray);
		}

		private void StartSpray()
		{
			SnowUtility.AddSnowRadial(this.OccupiedRect().RandomCell, 4f, -0.06f);
			this.spraySustainer = SoundDefOf.GeyserSpray.TrySpawnSustainer(base.Position);
			this.spraySustainerStartTick = Find.TickManager.TicksGame;
		}

		private void EndSpray()
		{
			if (this.spraySustainer != null)
			{
				this.spraySustainer.End();
				this.spraySustainer = null;
			}
		}

		public override void Tick()
		{
			if (this.harvester == null)
			{
				this.steamSprayer.SteamSprayerTick();
			}
			if (this.spraySustainer != null && Find.TickManager.TicksGame > this.spraySustainerStartTick + 1000)
			{
				Log.Message("Geyser spray sustainer still playing after 1000 ticks. Force-ending.");
				this.spraySustainer.End();
				this.spraySustainer = null;
			}
		}
	}
}
