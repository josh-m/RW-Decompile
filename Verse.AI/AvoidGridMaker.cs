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

		public static void RegenerateAvoidGridsFor(Faction faction)
		{
			if (!faction.def.canUseAvoidGrid)
			{
				return;
			}
			if (faction.avoidGridSmart == null)
			{
				faction.avoidGridSmart = new ByteGrid();
			}
			else
			{
				faction.avoidGridSmart.Clear();
			}
			if (faction.avoidGridBasic == null)
			{
				faction.avoidGridBasic = new ByteGrid();
			}
			else
			{
				faction.avoidGridBasic.Clear();
			}
			AvoidGridMaker.GenerateAvoidGridInternal(faction.avoidGridSmart, faction, AvoidGridMode.Smart);
			AvoidGridMaker.GenerateAvoidGridInternal(faction.avoidGridBasic, faction, AvoidGridMode.Basic);
		}

		internal static void Notify_CombatDangerousBuildingDespawned(Building building)
		{
			foreach (Faction current in Find.FactionManager.AllFactions)
			{
				if (current.HostileTo(Faction.OfPlayer) && Find.MapPawns.SpawnedPawnsInFaction(current).Count > 0)
				{
					AvoidGridMaker.RegenerateAvoidGridsFor(current);
				}
			}
		}

		private static void GenerateAvoidGridInternal(ByteGrid grid, Faction faction, AvoidGridMode mode)
		{
			List<TrapMemory> list = faction.TacticalMemory.TrapMemories();
			for (int i = 0; i < list.Count; i++)
			{
				AvoidGridMaker.PrintAvoidGridAroundTrapLoc(list[i], grid);
			}
			if (mode == AvoidGridMode.Smart)
			{
				List<Building> allBuildingsColonist = Find.ListerBuildings.allBuildingsColonist;
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
			AvoidGridMaker.ExpandAvoidGridIntoEdifices(grid);
		}

		private static void PrintAvoidGridAroundTrapLoc(TrapMemory mem, ByteGrid avoidGrid)
		{
			Room room = mem.Cell.GetRoom();
			for (int i = 0; i < AvoidGridMaker.TrapRadialCells; i++)
			{
				IntVec3 intVec = mem.Cell + GenRadial.RadialPattern[i];
				if (intVec.InBounds() && intVec.Walkable() && intVec.GetRoom() == room)
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
				if (intVec.InBounds() && intVec.Walkable() && GenSight.LineOfSight(intVec, tur.Position, true))
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

		private static void ExpandAvoidGridIntoEdifices(ByteGrid avoidGrid)
		{
			int numGridCells = CellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				if (avoidGrid[i] != 0)
				{
					if (Find.EdificeGrid[i] == null)
					{
						for (int j = 0; j < 8; j++)
						{
							IntVec3 c = CellIndices.IndexToCell(i) + GenAdj.AdjacentCells[j];
							if (c.InBounds())
							{
								if (c.GetEdifice() != null)
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
