using RimWorld.BaseGen;
using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GenStep_EscapeShip : GenStep_Scatterer
	{
		private static readonly IntRange EscapeShipSizeWidth = new IntRange(20, 28);

		private static readonly IntRange EscapeShipSizeHeight = new IntRange(34, 42);

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			if (!c.Standable(map))
			{
				return false;
			}
			if (c.Roofed(map))
			{
				return false;
			}
			if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
			{
				return false;
			}
			CellRect cellRect = new CellRect(c.x - GenStep_EscapeShip.EscapeShipSizeWidth.min / 2, c.z - GenStep_EscapeShip.EscapeShipSizeHeight.min / 2, GenStep_EscapeShip.EscapeShipSizeWidth.min, GenStep_EscapeShip.EscapeShipSizeHeight.min);
			if (!cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
			{
				return false;
			}
			foreach (IntVec3 current in cellRect)
			{
				TerrainDef terrainDef = map.terrainGrid.TerrainAt(current);
				if (!terrainDef.affordances.Contains(TerrainAffordance.Heavy) && (terrainDef.driesTo == null || !terrainDef.driesTo.affordances.Contains(TerrainAffordance.Heavy)))
				{
					return false;
				}
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 c, Map map, int stackCount = 1)
		{
			int randomInRange = GenStep_EscapeShip.EscapeShipSizeWidth.RandomInRange;
			int randomInRange2 = GenStep_EscapeShip.EscapeShipSizeHeight.RandomInRange;
			CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
			rect.ClipInsideMap(map);
			foreach (IntVec3 current in rect)
			{
				if (!map.terrainGrid.TerrainAt(current).affordances.Contains(TerrainAffordance.Heavy))
				{
					CompTerrainPumpDry.AffectCell(map, current);
					for (int i = 0; i < 8; i++)
					{
						Vector2 vector = UnityEngine.Random.insideUnitCircle * 3f;
						IntVec3 c2 = IntVec3.FromVector3(current.ToVector3Shifted() + new Vector3(vector.x, 0f, vector.y));
						if (c2.InBounds(map))
						{
							CompTerrainPumpDry.AffectCell(map, c2);
						}
					}
				}
			}
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			BaseGen.globalSettings.map = map;
			BaseGen.symbolStack.Push("ship_core", resolveParams);
			BaseGen.Generate();
		}
	}
}
