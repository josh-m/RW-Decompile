using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FloorFill : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			TerrainGrid terrainGrid = BaseGen.globalSettings.map.terrainGrid;
			TerrainDef newTerr = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef();
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				terrainGrid.SetTerrain(iterator.Current, newTerr);
				iterator.MoveNext();
			}
		}
	}
}
