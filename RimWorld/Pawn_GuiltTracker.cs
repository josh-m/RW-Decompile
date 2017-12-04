using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_GuiltTracker : IExposable
	{
		public int lastGuiltyTick = -99999;

		private const int GuiltyDuration = 60000;

		public bool IsGuilty
		{
			get
			{
				return this.TicksUntilInnocent > 0;
			}
		}

		public int TicksUntilInnocent
		{
			get
			{
				return Mathf.Max(0, this.lastGuiltyTick + 60000 - Find.TickManager.TicksGame);
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.lastGuiltyTick, "lastGuiltyTick", -99999, false);
		}

		public void Notify_Guilty()
		{
			this.lastGuiltyTick = Find.TickManager.TicksGame;
		}
	}
}
