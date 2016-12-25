using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathPool
	{
		private List<WorldPath> paths = new List<WorldPath>(64);

		private static readonly WorldPath notFoundPathInt;

		public static WorldPath NotFoundPath
		{
			get
			{
				return WorldPathPool.notFoundPathInt;
			}
		}

		static WorldPathPool()
		{
			WorldPathPool.notFoundPathInt = WorldPath.NewNotFound();
		}

		public WorldPath GetEmptyWorldPath()
		{
			for (int i = 0; i < this.paths.Count; i++)
			{
				if (!this.paths[i].inUse)
				{
					this.paths[i].inUse = true;
					return this.paths[i];
				}
			}
			if (this.paths.Count > Find.WorldObjects.CaravansCount + 2)
			{
				Log.ErrorOnce("WorldPathPool leak: more paths than caravans. Force-recovering.", 664788);
				this.paths.Clear();
			}
			WorldPath worldPath = new WorldPath();
			this.paths.Add(worldPath);
			worldPath.inUse = true;
			return worldPath;
		}
	}
}
