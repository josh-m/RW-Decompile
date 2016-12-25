using System;
using Verse;

namespace RimWorld
{
	public class PawnRecentMemory : IExposable
	{
		private Pawn pawn;

		private int lastLightTick = 999999;

		private int lastOutdoorTick = 999999;

		public int TicksSinceLastLight
		{
			get
			{
				return Find.TickManager.TicksGame - this.lastLightTick;
			}
		}

		public int TicksSinceOutdoors
		{
			get
			{
				return Find.TickManager.TicksGame - this.lastOutdoorTick;
			}
		}

		public PawnRecentMemory(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.lastLightTick, "lastLightTick", 999999, false);
			Scribe_Values.LookValue<int>(ref this.lastOutdoorTick, "lastOutdoorTick", 999999, false);
		}

		public void RecentMemoryInterval()
		{
			if (!this.pawn.Spawned)
			{
				return;
			}
			if (this.pawn.Map.glowGrid.PsychGlowAt(this.pawn.Position) != PsychGlow.Dark)
			{
				this.lastLightTick = Find.TickManager.TicksGame;
			}
			if (this.Outdoors())
			{
				this.lastOutdoorTick = Find.TickManager.TicksGame;
			}
		}

		private bool Outdoors()
		{
			Room room = this.pawn.GetRoom();
			return room != null && room.PsychologicallyOutdoors;
		}

		public void Notify_Spawned()
		{
			this.lastLightTick = Find.TickManager.TicksGame;
			if (this.Outdoors())
			{
				this.lastOutdoorTick = Find.TickManager.TicksGame;
			}
		}
	}
}
