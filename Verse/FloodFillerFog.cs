using System;
using System.Collections.Generic;

namespace Verse
{
	public static class FloodFillerFog
	{
		private const int MaxNumTestUnfog = 500;

		private static bool testMode = false;

		private static List<IntVec3> cellsToUnfog = new List<IntVec3>(1024);

		public static FloodUnfogResult FloodUnfog(IntVec3 root)
		{
			ProfilerThreadCheck.BeginSample("FloodUnfogFrom");
			FloodUnfogResult result = default(FloodUnfogResult);
			bool[] fogGridDirect = Find.FogGrid.fogGrid;
			FogGrid fogGrid = Find.FogGrid;
			List<IntVec3> newlyUnfoggedCells = new List<IntVec3>();
			int numUnfogged = 0;
			bool expanding = false;
			Predicate<IntVec3> predicate = delegate(IntVec3 c)
			{
				if (!fogGridDirect[CellIndices.CellToIndex(c)])
				{
					return false;
				}
				Thing edifice = c.GetEdifice();
				return (edifice == null || !edifice.def.MakeFog) && (!FloodFillerFog.testMode || expanding || numUnfogged <= 500);
			};
			Action<IntVec3> processor = delegate(IntVec3 c)
			{
				fogGrid.Unfog(c);
				newlyUnfoggedCells.Add(c);
				List<Thing> thingList = c.GetThingList();
				for (int l = 0; l < thingList.Count; l++)
				{
					Pawn pawn = thingList[l] as Pawn;
					if (pawn != null)
					{
						pawn.mindState.Active = true;
						if (pawn.def.race.IsMechanoid)
						{
							result.mechanoidFound = true;
						}
					}
				}
				if (FloodFillerFog.testMode)
				{
					numUnfogged++;
					Find.DebugDrawer.FlashCell(c, (float)numUnfogged / 200f, numUnfogged.ToStringCached());
				}
			};
			FloodFiller.FloodFill(root, predicate, processor);
			expanding = true;
			for (int i = 0; i < newlyUnfoggedCells.Count; i++)
			{
				IntVec3 a = newlyUnfoggedCells[i];
				for (int j = 0; j < 8; j++)
				{
					IntVec3 intVec = a + GenAdj.AdjacentCells[j];
					if (intVec.InBounds() && fogGrid.IsFogged(intVec))
					{
						if (!predicate(intVec))
						{
							FloodFillerFog.cellsToUnfog.Add(intVec);
						}
					}
				}
			}
			for (int k = 0; k < FloodFillerFog.cellsToUnfog.Count; k++)
			{
				fogGrid.Unfog(FloodFillerFog.cellsToUnfog[k]);
				if (FloodFillerFog.testMode)
				{
					Find.DebugDrawer.FlashCell(FloodFillerFog.cellsToUnfog[k], 0.3f, "x");
				}
			}
			FloodFillerFog.cellsToUnfog.Clear();
			ProfilerThreadCheck.EndSample();
			return result;
		}

		internal static void TestFloodUnfog(IntVec3 root)
		{
			Find.FogGrid.SetAllFogged();
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				Find.MapDrawer.MapMeshDirty(current, MapMeshFlag.FogOfWar);
			}
			FloodFillerFog.testMode = true;
			FloodFillerFog.FloodUnfog(root);
			FloodFillerFog.testMode = false;
		}

		internal static void TestRefogMap()
		{
			Find.FogGrid.SetAllFogged();
			foreach (IntVec3 current in Find.Map.AllCells)
			{
				Find.MapDrawer.MapMeshDirty(current, MapMeshFlag.FogOfWar);
			}
			FloodFillerFog.FloodUnfog(Find.MapPawns.FreeColonistsSpawned.RandomElement<Pawn>().Position);
		}
	}
}
