using System;
using System.Collections.Generic;

namespace Verse.AI
{
	public static class PawnPathPool
	{
		private static List<PawnPath> paths;

		private static readonly PawnPath NotFoundPathInt;

		public static PawnPath NotFoundPath
		{
			get
			{
				return PawnPathPool.NotFoundPathInt;
			}
		}

		static PawnPathPool()
		{
			PawnPathPool.paths = new List<PawnPath>(64);
			PawnPathPool.NotFoundPathInt = PawnPath.NewNotFound();
		}

		public static void Reinit()
		{
			PawnPathPool.paths.Clear();
		}

		public static PawnPath GetEmptyPawnPath()
		{
			for (int i = 0; i < PawnPathPool.paths.Count; i++)
			{
				if (!PawnPathPool.paths[i].inUse)
				{
					PawnPathPool.paths[i].inUse = true;
					return PawnPathPool.paths[i];
				}
			}
			if (PawnPathPool.paths.Count > Find.MapPawns.AllPawnsSpawnedCount)
			{
				Log.ErrorOnce("PawnPathPool leak: more paths than spawned pawns. Force-recovering.", 664788);
				PawnPathPool.paths.Clear();
			}
			PawnPath pawnPath = new PawnPath();
			PawnPathPool.paths.Add(pawnPath);
			pawnPath.inUse = true;
			return pawnPath;
		}
	}
}
