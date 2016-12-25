using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_RocksFromGrid : GenStep
	{
		private class RoofThreshold
		{
			public RoofDef roofDef;

			public float minGridVal;
		}

		private const int MinRoofedCellsPerGroup = 20;

		public static ThingDef RockDefAt(IntVec3 c)
		{
			ThingDef thingDef = null;
			float num = -999999f;
			for (int i = 0; i < RockNoises.rockNoises.Count; i++)
			{
				float value = RockNoises.rockNoises[i].noise.GetValue(c);
				if (value > num)
				{
					thingDef = RockNoises.rockNoises[i].rockDef;
					num = value;
				}
			}
			if (thingDef == null)
			{
				Log.ErrorOnce("Did not get rock def to generate at " + c, 50812);
				thingDef = ThingDefOf.Sandstone;
			}
			return thingDef;
		}

		public override void Generate(Map map)
		{
			if (map.TileInfo.WaterCovered)
			{
				return;
			}
			map.regionAndRoomUpdater.Enabled = false;
			float num = 0.7f;
			List<GenStep_RocksFromGrid.RoofThreshold> list = new List<GenStep_RocksFromGrid.RoofThreshold>();
			list.Add(new GenStep_RocksFromGrid.RoofThreshold
			{
				roofDef = RoofDefOf.RoofRockThick,
				minGridVal = num * 1.14f
			});
			list.Add(new GenStep_RocksFromGrid.RoofThreshold
			{
				roofDef = RoofDefOf.RoofRockThin,
				minGridVal = num * 1.04f
			});
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			foreach (IntVec3 current in map.AllCells)
			{
				float num2 = elevation[current];
				if (num2 > num)
				{
					ThingDef def = GenStep_RocksFromGrid.RockDefAt(current);
					GenSpawn.Spawn(def, current, map);
					for (int i = 0; i < list.Count; i++)
					{
						if (num2 > list[i].minGridVal)
						{
							map.roofGrid.SetRoof(current, list[i].roofDef);
							break;
						}
					}
				}
			}
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> toRemove = new List<IntVec3>();
			foreach (IntVec3 current2 in map.AllCells)
			{
				if (!visited[current2])
				{
					if (this.IsNaturalRoofAt(current2, map))
					{
						toRemove.Clear();
						map.floodFiller.FloodFill(current2, (IntVec3 x) => this.IsNaturalRoofAt(x, map), delegate(IntVec3 x)
						{
							visited[x] = true;
							toRemove.Add(x);
						});
						if (toRemove.Count < 20)
						{
							for (int j = 0; j < toRemove.Count; j++)
							{
								map.roofGrid.SetRoof(toRemove[j], null);
							}
						}
					}
				}
			}
			GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
			float num3 = 10f;
			switch (Find.WorldGrid[map.Tile].hilliness)
			{
			case Hilliness.Flat:
				num3 = 4f;
				break;
			case Hilliness.SmallHills:
				num3 = 8f;
				break;
			case Hilliness.LargeHills:
				num3 = 11f;
				break;
			case Hilliness.Mountainous:
				num3 = 15f;
				break;
			case Hilliness.Impassable:
				num3 = 16f;
				break;
			}
			genStep_ScatterLumpsMineable.countPer10kCellsRange = new FloatRange(num3, num3);
			genStep_ScatterLumpsMineable.Generate(map);
			map.regionAndRoomUpdater.Enabled = true;
		}

		private bool IsNaturalRoofAt(IntVec3 c, Map map)
		{
			return c.Roofed(map) && c.GetRoof(map).isNatural;
		}
	}
}
