using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public sealed class TerrainGrid : IExposable
	{
		private Map map;

		public TerrainDef[] topGrid;

		private TerrainDef[] underGrid;

		public TerrainGrid(Map map)
		{
			this.map = map;
			this.ResetGrids();
		}

		public void ResetGrids()
		{
			this.topGrid = new TerrainDef[this.map.cellIndices.NumGridCells];
			this.underGrid = new TerrainDef[this.map.cellIndices.NumGridCells];
		}

		public TerrainDef TerrainAt(int ind)
		{
			return this.topGrid[ind];
		}

		public TerrainDef TerrainAt(IntVec3 c)
		{
			return this.topGrid[this.map.cellIndices.CellToIndex(c)];
		}

		public void SetTerrain(IntVec3 c, TerrainDef newTerr)
		{
			if (newTerr == null)
			{
				Log.Error("Tried to set terrain at " + c + " to null.");
				return;
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Designation designation = this.map.designationManager.DesignationAt(c, DesignationDefOf.SmoothFloor);
				if (designation != null)
				{
					designation.Delete();
				}
			}
			int num = this.map.cellIndices.CellToIndex(c);
			if (newTerr.layerable)
			{
				if (this.underGrid[num] == null)
				{
					if (this.topGrid[num].passability != Traversability.Impassable)
					{
						this.underGrid[num] = this.topGrid[num];
					}
					else
					{
						this.underGrid[num] = TerrainDefOf.Sand;
					}
				}
			}
			else
			{
				this.underGrid[num] = null;
			}
			this.topGrid[num] = newTerr;
			this.DoTerrainChangedEffects(c);
		}

		public void RemoveTopLayer(IntVec3 c, bool doLeavings = true)
		{
			int num = this.map.cellIndices.CellToIndex(c);
			if (doLeavings)
			{
				GenLeaving.DoLeavingsFor(this.topGrid[num], c, this.map);
			}
			if (this.underGrid[num] != null)
			{
				this.topGrid[num] = this.underGrid[num];
			}
			this.underGrid[num] = null;
			this.DoTerrainChangedEffects(c);
		}

		private void DoTerrainChangedEffects(IntVec3 c)
		{
			this.map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Terrain, true, false);
			Plant plant = c.GetPlant(this.map);
			if (plant != null && this.map.fertilityGrid.FertilityAt(c) < plant.def.plant.fertilityMin)
			{
				plant.Destroy(DestroyMode.Vanish);
			}
			this.map.pathGrid.RecalculatePerceivedPathCostAt(c);
			Room room = RoomQuery.RoomAt(c, this.map);
			if (room != null)
			{
				room.Notify_TerrainChanged();
			}
		}

		public void ExposeData()
		{
			this.ExposeTerrainGrid(this.topGrid, "topGrid");
			this.ExposeTerrainGrid(this.underGrid, "underGrid");
		}

		private void ExposeTerrainGrid(TerrainDef[] grid, string label)
		{
			string compressedString = string.Empty;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				compressedString = GridSaveUtility.CompressedStringForShortGrid(delegate(IntVec3 c)
				{
					TerrainDef terrainDef2 = grid[this.map.cellIndices.CellToIndex(c)];
					return (terrainDef2 == null) ? 0 : terrainDef2.shortHash;
				}, this.map);
			}
			Scribe_Values.LookValue<string>(ref compressedString, label, null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Dictionary<ushort, TerrainDef> dictionary = new Dictionary<ushort, TerrainDef>();
				foreach (TerrainDef current in DefDatabase<TerrainDef>.AllDefs)
				{
					dictionary.Add(current.shortHash, current);
				}
				foreach (GridSaveUtility.LoadedGridShort current2 in GridSaveUtility.LoadedUShortGrid(compressedString, this.map))
				{
					TerrainDef terrainDef = null;
					try
					{
						if (current2.val == 0)
						{
							terrainDef = null;
						}
						else
						{
							terrainDef = dictionary[current2.val];
						}
					}
					catch (KeyNotFoundException)
					{
						Log.Error(string.Concat(new object[]
						{
							"Did not find terrain def with short hash ",
							current2.val,
							" for cell ",
							current2.cell,
							"."
						}));
						terrainDef = TerrainDefOf.Sand;
						dictionary.Add(current2.val, terrainDef);
					}
					grid[this.map.cellIndices.CellToIndex(current2.cell)] = terrainDef;
				}
			}
		}

		public string DebugStringAt(IntVec3 c)
		{
			if (c.InBounds(this.map))
			{
				TerrainDef terrain = c.GetTerrain(this.map);
				TerrainDef terrainDef = this.underGrid[this.map.cellIndices.CellToIndex(c)];
				return "top: " + ((terrain == null) ? "null" : terrain.defName) + ", under=" + ((terrainDef == null) ? "null" : terrainDef.defName);
			}
			return "out of bounds";
		}
	}
}
