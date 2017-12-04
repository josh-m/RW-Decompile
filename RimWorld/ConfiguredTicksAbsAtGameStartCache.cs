using System;
using Verse;

namespace RimWorld
{
	public class ConfiguredTicksAbsAtGameStartCache
	{
		private int cachedTicks = -1;

		private int cachedForStartingTile = -1;

		private Season cachedForStartingSeason;

		public bool TryGetCachedValue(GameInitData initData, out int ticksAbs)
		{
			if (initData.startingTile == this.cachedForStartingTile && initData.startingSeason == this.cachedForStartingSeason)
			{
				ticksAbs = this.cachedTicks;
				return true;
			}
			ticksAbs = -1;
			return false;
		}

		public void Cache(int ticksAbs, GameInitData initData)
		{
			this.cachedTicks = ticksAbs;
			this.cachedForStartingTile = initData.startingTile;
			this.cachedForStartingSeason = initData.startingSeason;
		}
	}
}
