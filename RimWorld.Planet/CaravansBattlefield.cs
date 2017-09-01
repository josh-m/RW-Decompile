using System;
using Verse;

namespace RimWorld.Planet
{
	public class CaravansBattlefield : MapParent
	{
		private bool wonBattle;

		public bool WonBattle
		{
			get
			{
				return this.wonBattle;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.wonBattle, "wonBattle", false, false);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
			{
				alsoRemoveWorldObject = true;
				return true;
			}
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void Tick()
		{
			base.Tick();
			if (base.HasMap)
			{
				this.CheckWonBattle();
			}
		}

		private void CheckWonBattle()
		{
			if (this.wonBattle)
			{
				return;
			}
			if (GenHostility.AnyHostileActiveThreat(base.Map))
			{
				return;
			}
			Messages.Message("MessageAmbushVictory".Translate(new object[]
			{
				MapParent.GetForceExitAndRemoveMapCountdownTimeLeftString(60000)
			}), this, MessageSound.Benefit);
			this.wonBattle = true;
			base.StartForceExitAndRemoveMapCountdown();
		}
	}
}
