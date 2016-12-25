using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class StealAIDebugDrawer
	{
		private static List<Thing> tmpToSteal = new List<Thing>();

		private static BoolGrid debugDrawGrid;

		private static Lord debugDrawLord = null;

		public static void DebugDraw()
		{
			if (!DebugViewSettings.drawStealDebug)
			{
				StealAIDebugDrawer.debugDrawLord = null;
				return;
			}
			Lord lord = StealAIDebugDrawer.debugDrawLord;
			StealAIDebugDrawer.debugDrawLord = StealAIDebugDrawer.FindHostileLord();
			if (StealAIDebugDrawer.debugDrawLord == null)
			{
				return;
			}
			StealAIDebugDrawer.CheckInitDebugDrawGrid();
			float num = StealAIUtility.StartStealingMarketValueThreshold(StealAIDebugDrawer.debugDrawLord);
			if (lord != StealAIDebugDrawer.debugDrawLord)
			{
				foreach (IntVec3 current in Find.VisibleMap.AllCells)
				{
					StealAIDebugDrawer.debugDrawGrid[current] = (StealAIDebugDrawer.TotalMarketValueAround(current, Find.VisibleMap, StealAIDebugDrawer.debugDrawLord.ownedPawns.Count) > num);
				}
			}
			foreach (IntVec3 current2 in Find.VisibleMap.AllCells)
			{
				if (StealAIDebugDrawer.debugDrawGrid[current2])
				{
					CellRenderer.RenderCell(current2);
				}
			}
			StealAIDebugDrawer.tmpToSteal.Clear();
			for (int i = 0; i < StealAIDebugDrawer.debugDrawLord.ownedPawns.Count; i++)
			{
				Pawn pawn = StealAIDebugDrawer.debugDrawLord.ownedPawns[i];
				Thing thing;
				if (StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 7f, out thing, pawn, StealAIDebugDrawer.tmpToSteal))
				{
					GenDraw.DrawLineBetween(pawn.TrueCenter(), thing.TrueCenter());
					StealAIDebugDrawer.tmpToSteal.Add(thing);
				}
			}
			StealAIDebugDrawer.tmpToSteal.Clear();
		}

		public static void Notify_ThingChanged(Thing thing)
		{
			if (StealAIDebugDrawer.debugDrawLord == null)
			{
				return;
			}
			StealAIDebugDrawer.CheckInitDebugDrawGrid();
			if (thing.def.category != ThingCategory.Building && thing.def.category != ThingCategory.Item && thing.def.passability != Traversability.Impassable)
			{
				return;
			}
			if (thing.def.passability == Traversability.Impassable)
			{
				StealAIDebugDrawer.debugDrawLord = null;
			}
			else
			{
				int num = GenRadial.NumCellsInRadius(8f);
				float num2 = StealAIUtility.StartStealingMarketValueThreshold(StealAIDebugDrawer.debugDrawLord);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec = thing.Position + GenRadial.RadialPattern[i];
					if (intVec.InBounds(thing.Map))
					{
						StealAIDebugDrawer.debugDrawGrid[intVec] = (StealAIDebugDrawer.TotalMarketValueAround(intVec, Find.VisibleMap, StealAIDebugDrawer.debugDrawLord.ownedPawns.Count) > num2);
					}
				}
			}
		}

		private static float TotalMarketValueAround(IntVec3 center, Map map, int pawnsCount)
		{
			if (center.Impassable(map))
			{
				return 0f;
			}
			float num = 0f;
			StealAIDebugDrawer.tmpToSteal.Clear();
			for (int i = 0; i < pawnsCount; i++)
			{
				IntVec3 intVec = center + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map) || intVec.Impassable(map) || !GenSight.LineOfSight(center, intVec, map, false))
				{
					intVec = center;
				}
				Thing thing;
				if (StealAIUtility.TryFindBestItemToSteal(intVec, map, 7f, out thing, null, StealAIDebugDrawer.tmpToSteal))
				{
					num += StealAIUtility.GetValue(thing);
					StealAIDebugDrawer.tmpToSteal.Add(thing);
				}
			}
			StealAIDebugDrawer.tmpToSteal.Clear();
			return num;
		}

		private static Lord FindHostileLord()
		{
			Lord lord = null;
			List<Lord> lords = Find.VisibleMap.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				if (lords[i].faction.HostileTo(Faction.OfPlayer))
				{
					if (lord == null || lords[i].ownedPawns.Count > lord.ownedPawns.Count)
					{
						lord = lords[i];
					}
				}
			}
			return lord;
		}

		private static void CheckInitDebugDrawGrid()
		{
			if (StealAIDebugDrawer.debugDrawGrid == null)
			{
				StealAIDebugDrawer.debugDrawGrid = new BoolGrid(Find.VisibleMap);
			}
			else if (!StealAIDebugDrawer.debugDrawGrid.MapSizeMatches(Find.VisibleMap))
			{
				StealAIDebugDrawer.debugDrawGrid.ClearAndResizeTo(Find.VisibleMap);
			}
		}
	}
}
