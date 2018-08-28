using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EnsureCanHoldRoof : SymbolResolver
	{
		private static HashSet<IntVec3> roofsAboutToCollapse = new HashSet<IntVec3>();

		private static List<IntVec3> edgeRoofs = new List<IntVec3>();

		private static HashSet<IntVec3> visited = new HashSet<IntVec3>();

		public override void Resolve(ResolveParams rp)
		{
			ThingDef wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, false);
			do
			{
				this.CalculateRoofsAboutToCollapse(rp.rect);
				this.CalculateEdgeRoofs(rp.rect);
			}
			while (this.TrySpawnPillar(rp.faction, wallStuff));
		}

		private void CalculateRoofsAboutToCollapse(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Clear();
			SymbolResolver_EnsureCanHoldRoof.visited.Clear();
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (current.Roofed(map))
				{
					if (!RoofCollapseCellsFinder.ConnectsToRoofHolder(current, map, SymbolResolver_EnsureCanHoldRoof.visited))
					{
						map.floodFiller.FloodFill(current, (IntVec3 x) => x.Roofed(map), delegate(IntVec3 x)
						{
							SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Add(x);
						}, 2147483647, false, null);
					}
				}
				iterator.MoveNext();
			}
			CellRect.CellRectIterator iterator2 = rect.GetIterator();
			while (!iterator2.Done())
			{
				IntVec3 current2 = iterator2.Current;
				if (current2.Roofed(map))
				{
					if (!SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Contains(current2))
					{
						if (!RoofCollapseUtility.WithinRangeOfRoofHolder(current2, map, false))
						{
							SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Add(current2);
						}
					}
				}
				iterator2.MoveNext();
			}
		}

		private void CalculateEdgeRoofs(CellRect rect)
		{
			SymbolResolver_EnsureCanHoldRoof.edgeRoofs.Clear();
			foreach (IntVec3 current in SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse)
			{
				for (int i = 0; i < 4; i++)
				{
					IntVec3 item = current + GenAdj.CardinalDirections[i];
					if (!SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Contains(item))
					{
						SymbolResolver_EnsureCanHoldRoof.edgeRoofs.Add(current);
						break;
					}
				}
			}
		}

		private bool TrySpawnPillar(Faction faction, ThingDef wallStuff)
		{
			if (!SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Any<IntVec3>())
			{
				return false;
			}
			Map map = BaseGen.globalSettings.map;
			IntVec3 bestCell = IntVec3.Invalid;
			float bestScore = 0f;
			FloodFiller arg_8A_0 = map.floodFiller;
			IntVec3 invalid = IntVec3.Invalid;
			Predicate<IntVec3> passCheck = (IntVec3 x) => SymbolResolver_EnsureCanHoldRoof.roofsAboutToCollapse.Contains(x);
			Action<IntVec3> processor = delegate(IntVec3 x)
			{
				float pillarSpawnScore = this.GetPillarSpawnScore(x);
				if (pillarSpawnScore > 0f && (!bestCell.IsValid || pillarSpawnScore >= bestScore))
				{
					bestCell = x;
					bestScore = pillarSpawnScore;
				}
			};
			List<IntVec3> extraRoots = SymbolResolver_EnsureCanHoldRoof.edgeRoofs;
			arg_8A_0.FloodFill(invalid, passCheck, processor, 2147483647, false, extraRoots);
			if (bestCell.IsValid)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Wall, wallStuff);
				thing.SetFaction(faction, null);
				GenSpawn.Spawn(thing, bestCell, map, WipeMode.Vanish);
				return true;
			}
			return false;
		}

		private float GetPillarSpawnScore(IntVec3 c)
		{
			Map map = BaseGen.globalSettings.map;
			if (c.Impassable(map) || c.GetFirstBuilding(map) != null || c.GetFirstItem(map) != null || c.GetFirstPawn(map) != null)
			{
				return 0f;
			}
			bool flag = true;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (!c2.InBounds(map) || !c2.Walkable(map))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return 2f;
			}
			return 1f;
		}
	}
}
