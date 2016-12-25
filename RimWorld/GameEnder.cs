using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class GameEnder : IExposable
	{
		private const int GameEndCountdownDuration = 400;

		public bool gameEnding;

		private int ticksToGameOver = -1;

		public void ExposeData()
		{
			Scribe_Values.LookValue<bool>(ref this.gameEnding, "gameEnding", false, false);
			Scribe_Values.LookValue<int>(ref this.ticksToGameOver, "ticksToGameOver", -1, false);
		}

		public void CheckGameOver()
		{
			if (Find.TickManager.TicksGame < 300)
			{
				return;
			}
			if (this.gameEnding)
			{
				return;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount >= 1)
				{
					return;
				}
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int j = 0; j < caravans.Count; j++)
			{
				if (this.IsPlayerControlledWithFreeColonist(caravans[j]))
				{
					return;
				}
			}
			List<TravelingTransportPods> travelingTransportPods = Find.WorldObjects.TravelingTransportPods;
			for (int k = 0; k < travelingTransportPods.Count; k++)
			{
				if (travelingTransportPods[k].PodsHaveAnyFreeColonist)
				{
					return;
				}
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
