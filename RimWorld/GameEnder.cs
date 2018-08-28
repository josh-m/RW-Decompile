using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class GameEnder : IExposable
	{
		public bool gameEnding;

		private int ticksToGameOver = -1;

		private const int GameEndCountdownDuration = 400;

		public void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.gameEnding, "gameEnding", false, false);
			Scribe_Values.Look<int>(ref this.ticksToGameOver, "ticksToGameOver", -1, false);
		}

		public void CheckOrUpdateGameOver()
		{
			if (Find.TickManager.TicksGame < 300)
			{
				return;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount >= 1)
				{
					this.gameEnding = false;
					return;
				}
			}
			for (int j = 0; j < maps.Count; j++)
			{
				List<Pawn> allPawnsSpawned = maps[j].mapPawns.AllPawnsSpawned;
				for (int k = 0; k < allPawnsSpawned.Count; k++)
				{
					if (allPawnsSpawned[k].carryTracker != null)
					{
						Pawn pawn = allPawnsSpawned[k].carryTracker.CarriedThing as Pawn;
						if (pawn != null && pawn.IsFreeColonist)
						{
							this.gameEnding = false;
							return;
						}
					}
				}
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int l = 0; l < caravans.Count; l++)
			{
				if (this.IsPlayerControlledWithFreeColonist(caravans[l]))
				{
					this.gameEnding = false;
					return;
				}
			}
			List<TravelingTransportPods> travelingTransportPods = Find.WorldObjects.TravelingTransportPods;
			for (int m = 0; m < travelingTransportPods.Count; m++)
			{
				if (travelingTransportPods[m].PodsHaveAnyFreeColonist)
				{
					this.gameEnding = false;
					return;
				}
			}
			if (this.gameEnding)
			{
				return;
			}
			this.gameEnding = true;
			this.ticksToGameOver = 400;
		}

		public void GameEndTick()
		{
			if (this.gameEnding)
			{
				this.ticksToGameOver--;
				if (this.ticksToGameOver == 0)
				{
					GenGameEnd.EndGameDialogMessage("GameOverEveryoneDead".Translate(), true);
				}
			}
		}

		private bool IsPlayerControlledWithFreeColonist(Caravan caravan)
		{
			if (!caravan.IsPlayerControlled)
			{
				return false;
			}
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn = pawnsListForReading[i];
				if (pawn.IsColonist && pawn.HostFaction == null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
