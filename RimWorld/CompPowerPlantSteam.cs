using System;
using Verse;

namespace RimWorld
{
	public class CompPowerPlantSteam : CompPowerPlant
	{
		private IntermittentSteamSprayer steamSprayer;

		private Building_SteamGeyser geyser;

		public override void PostSpawnSetup()
		{
			base.PostSpawnSetup();
			this.steamSprayer = new IntermittentSteamSprayer(this.parent);
		}

		public override void CompTick()
		{
			base.CompTick();
			if (this.geyser == null)
			{
				this.geyser = (Building_SteamGeyser)Find.ThingGrid.ThingAt(this.parent.Position, ThingDefOf.SteamGeyser);
			}
			if (this.geyser != null)
			{
				this.geyser.harvester = (Building)this.parent;
				this.steamSprayer.SteamSprayerTick();
			}
		}

		public override void PostDeSpawn()
		{
			base.PostDeSpawn();
			if (this.geyser != null)
			{
				this.geyser.harvester = null;
			}
		}
	}
}
