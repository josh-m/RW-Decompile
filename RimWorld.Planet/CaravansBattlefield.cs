using System;
using System.Collections.Generic;
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
			Scribe_Values.LookValue<bool>(ref this.wonBattle, "wonBattle", false, false);
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			if (!base.Map.mapPawns.AnyColonistTameAnimalOrPrisonerOfColony)
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
			List<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				if (!PawnUtility.ThreatDisabledOrFleeing(allPawnsSpawned[i]) && allPawnsSpawned[i].HostileTo(Faction.OfPlayer))
				{
					return;
				}
			}
			Messages.Message("MessageAmbushVictory".Translate(new object[]
			{
				24
			}), new GlobalTargetInfo(base.Tile), MessageSound.Benefit);
			this.wonBattle = true;
			base.StartForceExitAndRemoveMapCountdown();
		}
	}
}
