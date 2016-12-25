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
			ByteGrid byteGrid2;
			if (faction.avoidGridsBasic.TryGetValue(map, out byteGrid2))
			{
				byteGrid2.Clear(0);
			}
			else
			{
				byteGrid2 = new ByteGrid(map);
				faction.avoidGridsBasic.Add(map, byteGrid2);
			}
			AvoidGridMaker.GenerateAvoidGridInternal(byteGrid, faction, map, AvoidGridMode.Smart);
			AvoidGridMaker.GenerateAvoidGridInternal(byteGrid2, faction, map, AvoidGridMode.Basic);
		}

		internal static void Notify_CombatDangerousBuildingDespawned(Building building, Map map)
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
			List<TrapMemory> list = faction.TacticalMemory.TrapMemories();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].map == map)
				{
					AvoidGridMaker.PrintAvoidGridAroundTrapLoc(list[i], grid);
				}
			}
			if (mode == AvoidGridMode.Smart)
			{
				List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
				for (int j = 0; j < allBuildingsColonist.Count; j++)
				{
					if (allBuildingsColonist[j].def.building.ai_combatDangerous)
					{
						Building_TurretGun building_TurretGun = allBuildingsColonist[j] as Building_TurretGun;
						if (building_TurretGun != null)
						{
							AvoidGridMaker.PrintAvoidGridAroundTurret(building_TurretGun, grid);
						}
					}
				}
			}
			AvoidGridMaker.ExpandAvoidGridIntoEdifices(grid, map);
		}

		private static void PrintAvoidGridAroundTrapLoc(TrapMemory mem, ByteGrid avoidGrid)
		{
			Room room = mem.Cell.GetRoom(mem.map);
			for (int i = 0; i < AvoidGridMaker.TrapRadialCells; i++)
			{
				IntVec3 intVec = mem.Cell + GenRadial.RadialPattern[i];
				if (intVec.InBounds(mem.map) && intVec.Walkable(mem.map) && intVec.GetRoom(mem.map) == room)
				{
					float num = Mathf.Max(1f, intVec.DistanceToSquared(mem.Cell));
					int num2 = Mathf.Max(1, Mathf.RoundToInt(32f * mem.PowerPercent / num));
					AvoidGridMaker.IncrementAvoidGrid(avoidGrid, intVec, num2);
				}
			}
		}

		private static void PrintAvoidGridAroundTurret(Building_TurretGun tur, ByteGrid avoidGrid)
		{
			int num = GenRadial.NumCellsInRadius(tur.GunCompEq.PrimaryVerb.verbProps.range + 4f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = tur.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(tur.Map) && intVec.Walkable(tur.Map) && GenSight.LineOfSight(intVec, tur.Position, tur.Map, true))
				{
					AvoidGridMaker.IncrementAvoidGrid(avoidGrid, intVec, 12);
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
