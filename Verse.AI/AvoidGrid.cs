using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public class AvoidGrid
	{
		public Map map;

		private ByteGrid grid;

		private bool gridDirty = true;

		public ByteGrid Grid
		{
			get
			{
				if (this.gridDirty)
				{
					this.Regenerate();
				}
				return this.grid;
			}
		}

		public AvoidGrid(Map map)
		{
			this.map = map;
			this.grid = new ByteGrid(map);
		}

		public void Regenerate()
		{
			this.gridDirty = false;
			this.grid.Clear(0);
			List<Building> allBuildingsColonist = this.map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def.building.ai_combatDangerous)
				{
					Building_TurretGun building_TurretGun = allBuildingsColonist[i] as Building_TurretGun;
					if (building_TurretGun != null)
					{
						this.PrintAvoidGridAroundTurret(building_TurretGun);
					}
				}
			}
			this.ExpandAvoidGridIntoEdifices();
		}

		public void Notify_BuildingSpawned(Building building)
		{
			if (building.def.building.ai_combatDangerous || !building.CanBeSeenOver())
			{
				this.gridDirty = true;
			}
		}

		public void Notify_BuildingDespawned(Building building)
		{
			if (building.def.building.ai_combatDangerous || !building.CanBeSeenOver())
			{
				this.gridDirty = true;
			}
		}

		public void DebugDrawOnMap()
		{
			if (DebugViewSettings.drawAvoidGrid && Find.CurrentMap == this.map)
			{
				this.Grid.DebugDraw();
			}
		}

		private void PrintAvoidGridAroundTurret(Building_TurretGun tur)
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
					this.IncrementAvoidGrid(intVec, 45);
				}
			}
		}

		private void IncrementAvoidGrid(IntVec3 c, int num)
		{
			byte b = this.grid[c];
			b = (byte)Mathf.Min(255, (int)b + num);
			this.grid[c] = b;
		}

		private void ExpandAvoidGridIntoEdifices()
		{
			int numGridCells = this.map.cellIndices.NumGridCells;
			for (int i = 0; i < numGridCells; i++)
			{
				if (this.grid[i] != 0)
				{
					if (this.map.edificeGrid[i] == null)
					{
						for (int j = 0; j < 8; j++)
						{
							IntVec3 c = this.map.cellIndices.IndexToCell(i) + GenAdj.AdjacentCells[j];
							if (c.InBounds(this.map))
							{
								if (c.GetEdifice(this.map) != null)
								{
									this.grid[c] = (byte)Mathf.Min(255, Mathf.Max((int)this.grid[c], (int)this.grid[i]));
								}
							}
						}
					}
				}
			}
		}
	}
}
