using System;
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
			if (Find.MapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount >= 1)
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
	}
}
