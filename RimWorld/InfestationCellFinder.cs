using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class InfestationCellFinder
	{
		private struct LocationCandidate
		{
			public IntVec3 cell;

			public float score;

			public LocationCandidate(IntVec3 cell, float score)
			{
				this.cell = cell;
				this.score = score;
			}
		}

		private const float MinRequiredScore = 7.5f;

		private const float MinMountainousnessScore = 0.17f;

		private const int MountainousnessScoreRadialPatternIdx = 700;

		private const int MountainousnessScoreRadialPatternSkip = 10;

		private const float MountainousnessScorePerRock = 1f;

		private const float MountainousnessScorePerThickRoof = 0.5f;

		private const float MinCellTempToSpawnHive = -17f;

		private const float MaxDistanceToColonyBuilding = 30f;

		private static List<InfestationCellFinder.LocationCandidate> locationCandidates = new List<InfestationCellFinder.LocationCandidate>();

		private static Dictionary<Region, float> regionsDistanceToUnroofed = new Dictionary<Region, float>();

		private static ByteGrid closedAreaSize;

		private static ByteGrid distToColonyBuilding;

		private static HashSet<Region> tempUnroofedRegions = new HashSet<Region>();

		private static List<IntVec3> tmpColonyBuildingsLocs = new List<IntVec3>();

		private static List<KeyValuePair<IntVec3, float>> tmpDistanceResult = new List<KeyValuePair<IntVec3, float>>();

		public static bool TryFindCell(out IntVec3 cell, Map map)
		{
			InfestationCellFinder.CalculateLocationCandidates(map);
			InfestationCellFinder.LocationCandidate locationCandidate;
			if (!InfestationCellFinder.locationCandidates.TryRandomElementByWeight((InfestationCellFinder.LocationCandidate x) => x.score, out locationCandidate))
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = locationCandidate.cell;
			return true;
		}

		private static float GetScoreAt(IntVec3 cell, Map map)
		{
			if ((float)InfestationCellFinder.distToColonyBuilding[cell] > 30f)
			{
				return 0f;
			}
			if (!cell.Standable(map))
			{
				return 0f;
			}
			if (cell.Fogged(map))
			{
				return 0f;
			}
			if (InfestationCellFinder.CellHasBlockingThings(cell, map))
			{
				return 0f;
			}
			if (!cell.Roofed(map) || !cell.GetRoof(map).isThickRoof)
			{
				return 0f;
			}
			Region region = cell.GetRegion(map);
			if (region == null)
			{
				return 0f;
			}
			if (InfestationCellFinder.closedAreaSize[cell] < 16)
			{
				return 0f;
			}
			float temperature = cell.GetTemperature(map);
			if (temperature < -17f)
			{
				return 0f;
			}
			float mountainousnessScoreAt = InfestationCellFinder.GetMountainousnessScoreAt(cell, map);
			if (mountainousnessScoreAt < 0.17f)
			{
				return 0f;
			}
			int num = InfestationCellFinder.StraightLineDistToUnroofed(cell, map);
			float num2;
			if (!InfestationCellFinder.regionsDistanceToUnroofed.TryGetValue(region, out num2))
			{
				num2 = (float)num * 1.15f;
			}
			else
			{
				num2 = Mathf.Min(num2, (float)num * 4f);
			}
			num2 = Mathf.Pow(num2, 1.55f);
			float num3 = Mathf.InverseLerp(0f, 12f, (float)num);
			float num4 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GameGlowAt(cell));
			float num5 = 1f - Mathf.Clamp(InfestationCellFinder.DistToBlocker(cell, map) / 11f, 0f, 0.6f);
			float num6 = Mathf.InverseLerp(-17f, -7f, temperature);
			float num7 = num2 * num3 * num5 * mountainousnessScoreAt * num4 * num6;
			num7 = Mathf.Pow(num7, 1.2f);
			if (num7 < 7.5f)
			{
				return 0f;
			}
			return num7;
		}

		public static void DebugDraw()
		{
			if (DebugViewSettings.drawInfestationChance)
			{
				Map visibleMap = Find.VisibleMap;
				CellRect cellRect = Find.CameraDriver.CurrentViewRect;
				cellRect.ClipInsideMap(visibleMap);
				cellRect = cellRect.ExpandedBy(1);
				InfestationCellFinder.CalculateTraversalDistancesToUnroofed(visibleMap);
				InfestationCellFinder.CalculateClosedAreaSizeGrid(visibleMap);
				InfestationCellFinder.CalculateDistanceToColonyBuildingGrid(visibleMap);
				float num = 0.001f;
				for (int i = 0; i < visibleMap.Size.z; i++)
				{
					for (int j = 0; j < visibleMap.Size.x; j++)
					{
						IntVec3 cell = new IntVec3(j, 0, i);
						float scoreAt = InfestationCellFinder.GetScoreAt(cell, visibleMap);
						if (scoreAt > num)
						{
							num = scoreAt;
						}
					}
				}
				for (int k = 0; k < visibleMap.Size.z; k++)
				{
					for (int l = 0; l < visibleMap.Size.x; l++)
					{
						IntVec3 intVec = new IntVec3(l, 0, k);
						if (cellRect.Contains(intVec))
						{
							float scoreAt2 = InfestationCellFinder.GetScoreAt(intVec, visibleMap);
							if (scoreAt2 > 0f)
							{
								float a = GenMath.LerpDouble(7.5f, num, 0f, 1f, scoreAt2);
								CellRenderer.RenderCell(intVec, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, a)));
							}
						}
					}
				}
			}
		}

		private static void CalculateLocationCandidates(Map map)
		{
			InfestationCellFinder.locationCandidates.Clear();
			InfestationCellFinder.CalculateTraversalDistancesToUnroofed(map);
			InfestationCellFinder.CalculateClosedAreaSizeGrid(map);
			InfestationCellFinder.CalculateDistanceToColonyBuildingGrid(map);
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 cell = new IntVec3(j, 0, i);
					float scoreAt = InfestationCellFinder.GetScoreAt(cell, map);
					if (scoreAt > 0f)
					{
						InfestationCellFinder.locationCandidates.Add(new InfestationCellFinder.LocationCandidate(cell, scoreAt));
					}
				}
			}
		}

		private static bool CellHasBlockingThings(IntVec3 cell, Map map)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Pawn)
				{
					return true;
				}
				if ((thingList[i].def.category == ThingCategory.Building || thingList[i].def.category == ThingCategory.Item) && GenSpawn.SpawningWipes(ThingDefOf.Hive, thingList[i].def))
				{
					return true;
				}
			}
			return false;
		}

		private static int StraightLineDistToUnroofed(IntVec3 cell, Map map)
		{
			int num = 2147483647;
			int i = 0;
			while (i < 4)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 facingCell = rot.FacingCell;
				int num2 = 0;
				int num3;
				while (true)
				{
					IntVec3 intVec = cell + facingCell * num2;
					if (!intVec.InBounds(map))
					{
						goto Block_1;
					}
					num3 = num2;
					if (InfestationCellFinder.NoRoofAroundAndWalkable(intVec, map))
					{
						break;
					}
					num2++;
				}
				IL_6F:
				if (num3 < num)
				{
					num = num3;
				}
				i++;
				continue;
				Block_1:
				num3 = 2147483647;
				goto IL_6F;
			}
			if (num == 2147483647)
			{
				return map.Size.x;
			}
			return num;
		}

		private static float DistToBlocker(IntVec3 cell, Map map)
		{
			int num = -2147483648;
			int num2 = -2147483648;
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 facingCell = rot.FacingCell;
				int num3 = 0;
				int num4;
				while (true)
				{
					IntVec3 c = cell + facingCell * num3;
					num4 = num3;
					if (!c.InBounds(map) || !c.Walkable(map))
					{
						break;
					}
					num3++;
				}
				if (num4 > num)
				{
					num2 = num;
					num = num4;
				}
				else if (num4 > num2)
				{
					num2 = num4;
				}
			}
			return (float)Mathf.Min(num, num2);
		}

		private static bool NoRoofAroundAndWalkable(IntVec3 cell, Map map)
		{
			if (!cell.Walkable(map))
			{
				return false;
			}
			if (cell.Roofed(map))
			{
				return false;
			}
			for (int i = 0; i < 4; i++)
			{
				Rot4 rot = new Rot4(i);
				IntVec3 c = rot.FacingCell + cell;
				if (c.InBounds(map) && c.Roofed(map))
				{
					return false;
				}
			}
			return true;
		}

		private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < 700; i += 10)
			{
				IntVec3 c = cell + GenRadial.RadialPattern[i];
				if (c.InBounds(map))
				{
					Building edifice = c.GetEdifice(map);
					if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
					{
						num += 1f;
					}
					else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
					{
						num += 0.5f;
					}
					num2++;
				}
			}
			return num / (float)num2;
		}

		private static void CalculateTraversalDistancesToUnroofed(Map map)
		{
			InfestationCellFinder.tempUnroofedRegions.Clear();
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					Region region = intVec.GetRegion(map);
					if (region != null && InfestationCellFinder.NoRoofAroundAndWalkable(intVec, map))
					{
						InfestationCellFinder.tempUnroofedRegions.Add(region);
					}
				}
			}
			Dijkstra<Region>.Run(InfestationCellFinder.tempUnroofedRegions, (Region x) => x.Neighbors, (Region a, Region b) => Mathf.Sqrt(a.extentsClose.CenterCell.DistanceToSquared(b.extentsClose.CenterCell)), ref InfestationCellFinder.regionsDistanceToUnroofed);
			InfestationCellFinder.tempUnroofedRegions.Clear();
		}

		private static void CalculateClosedAreaSizeGrid(Map map)
		{
			if (InfestationCellFinder.closedAreaSize == null)
			{
				InfestationCellFinder.closedAreaSize = new ByteGrid(map);
			}
			else
			{
				InfestationCellFinder.closedAreaSize.ClearAndResizeTo(map);
			}
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 intVec = new IntVec3(j, 0, i);
					if (InfestationCellFinder.closedAreaSize[j, i] == 0 && !intVec.Impassable(map))
					{
						int area = 0;
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							area++;
						});
						area = Mathf.Min(area, 255);
						map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
						{
							InfestationCellFinder.closedAreaSize[c] = (byte)area;
						});
					}
				}
			}
		}

		private static void CalculateDistanceToColonyBuildingGrid(Map map)
		{
			if (InfestationCellFinder.distToColonyBuilding == null)
			{
				InfestationCellFinder.distToColonyBuilding = new ByteGrid(map);
			}
			else if (!InfestationCellFinder.distToColonyBuilding.MapSizeMatches(map))
			{
				InfestationCellFinder.distToColonyBuilding.ClearAndResizeTo(map);
			}
			InfestationCellFinder.distToColonyBuilding.Clear(255);
			InfestationCellFinder.tmpColonyBuildingsLocs.Clear();
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				InfestationCellFinder.tmpColonyBuildingsLocs.Add(allBuildingsColonist[i].Position);
			}
			Dijkstra<IntVec3>.Run(InfestationCellFinder.tmpColonyBuildingsLocs, (IntVec3 x) => DijkstraUtility.AdjacentCellsNeighborsGetter(x, map), delegate(IntVec3 a, IntVec3 b)
			{
				if (a.x == b.x || a.z == b.z)
				{
					return 1f;
				}
				return 1.41421354f;
			}, ref InfestationCellFinder.tmpDistanceResult);
			for (int j = 0; j < InfestationCellFinder.tmpDistanceResult.Count; j++)
			{
				InfestationCellFinder.distToColonyBuilding[InfestationCellFinder.tmpDistanceResult[j].Key] = (byte)Mathf.Min(InfestationCellFinder.tmpDistanceResult[j].Value, 254.999f);
			}
		}
	}
}
