using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI.Group;

namespace Verse.AI
{
	public static class AvoidGridMaker
	{
		private static readonly int TrapRadialCells = GenRadial.NumCellsInRadius(2.9f);

		public static void RegenerateAllAvoidGridsFor(Faction faction)
		{
			if (!faction.def.canUseAvoidGrid)
			{
				return;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				AvoidGridMaker.RegenerateAvoidGridsFor(faction, maps[i]);
			}
		}

		public static void RegenerateAvoidGridsFor(Faction faction, Map map)
		{
			if (!faction.def.canUseAvoidGrid)
			{
				return;
			}
			ByteGrid byteGrid;
			if (faction.avoidGridsSmart.TryGetValue(map, out byteGrid))
			{
				byteGrid.Clear(0);
			}
			else
			{
				byteGrid = new ByteGrid(map);
				faction.avoidGridsSmart.Add(map, byteGrid);
			}
			AvoidGridMaker.GenerateAvoidGridInternal(byteGrid, faction, map, AvoidGridMode.Smart);
		}

		public static void Notify_CombatDangerousBuildingDespawned(Building building, Map map)
		{
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				if (current.HostileTo(Faction.OfPlayer) && map.mapPawns.SpawnedPawnsInFaction(current).Count > 0)
				{
					AvoidGridMaker.RegenerateAvoidGridsFor(current, map);
				}
			}
		}

		private static void GenerateAvoidGridInternal(ByteGrid grid, Faction faction, Map map, AvoidGridMode mode)
		{
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			if (mode == AvoidGridMode.Smart)
			{
				for (int i = 0; i < allBuildingsColonist.Count; i++)
				{
					if (Rand.Chance(0.5f) && allBuildingsColonist[i].def.building.isTrap)
					{
						AvoidGridMaker.PrintAvoidGridAroundTrapLoc(allBuildingsColonist[i], grid);
					}
					if (allBuildingsColonist[i].def.building.ai_combatDangerous)
					{
						Building_TurretGun building_TurretGun = allBuildingsColonist[i] as Building_TurretGun;
						if (building_TurretGun != null)
						{
							AvoidGridMaker.PrintAvoidGridAroundTurret(building_TurretGun, grid);
						}
					}
				}
			}
			AvoidGridMaker.ExpandAvoidGridIntoEdifices(grid, map);
		}

		private static void PrintAvoidGridAroundTrapLoc(Building b, ByteGrid avoidGrid)
		{
			Room room = b.Position.GetRoom(b.Map, RegionType.Set_Passable);
			for (int i = 0; i < AvoidGridMaker.TrapRadialCells; i++)
			{
				IntVec3 intVec = b.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(b.Map) && intVec.Walkable(b.Map) && intVec.GetRoom(b.Map, RegionType.Set_Passable) == room)
				{
					float num = (float)Mathf.Max(1, intVec.DistanceToSquared(b.Position));
					int num2 = Mathf.Max(1, Mathf.RoundToInt(80f / num));
					AvoidGridMaker.IncrementAvoidGrid(avoidGrid, intVec, num2);
				}
			}
		}

		private static void PrintAvoidGridAroundTurret(Building_TurretGun tur, ByteGrid avoidGrid)
		{
			float range = tur.GunCompEq.PrimaryVerb.verbProps.range;
			float num = tur.GunCompEq.PrimaryVerb.verbProps.EffectiveMinRange(true);
			int num2 = GenRadial.NumCellsInRadius(range + 4f);
			int num3 = (num >= 1f) ? GenRadial.NumCellsInRadius(num) : 0;
			for (int i = num3; i < num2; i++)
			{
				IntVec3 intVec = tur.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(tur.Map) && intVec.Walkable(tur.Map) && GenSight.LineOfSight(intVec, tur.Position, tur.Map, true, null, 0, 0))
				{
					AvoidGridMaker.IncrementAvoidGrid(avoidGrid, intVec, 45);
				}
			}
		}

		private static void IncrementAvoidGrid(ByteGrid avoidGrid, IntVec3 c, int num)
		{
			byte b = avoidGrid[c];
			b = (byte)Mathf.Min(255, (int)b + num);
			avoidGrid[c] = b;
		}

		private static void ExpandAvoidGridIntoEdifices(ByteGrid avoidGrid, Map map)
		{
			int numGridCells = map.cellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				if (avoidGrid[i] != 0)
				{
					if (map.edificeGrid[i] == null)
					{
						for (int j = 0; j < 8; j++)
						{
							IntVec3 c = map.cellIndices.IndexToCell(i) + GenAdj.AdjacentCells[j];
							if (c.InBounds(map))
							{
								if (c.GetEdifice(map) != null)
								{
									avoidGrid[c] = (byte)Mathf.Min(255, Mathf.Max((int)avoidGrid[c], (int)avoidGrid[i]));
								}
							}
						}
					}
				}
			}
		}
	}
}
