using System;
using UnityEngine;

namespace Verse
{
	public sealed class SnowGrid : IExposable
	{
		public const float MaxDepth = 1f;

		private float[] depthGrid;

		private double totalDepth;

		internal float[] DepthGridDirect_Unsafe
		{
			get
			{
				return this.depthGrid;
			}
		}

		public float TotalDepth
		{
			get
			{
				return (float)this.totalDepth;
			}
		}

		public SnowGrid()
		{
			if (this.depthGrid == null)
			{
				this.depthGrid = new float[CellIndices.NumGridCells];
			}
		}

		public void ExposeData()
		{
			string compressedString = null;
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				compressedString = GridSaveUtility.CompressedStringForShortGrid((IntVec3 c) => SnowGrid.SnowFloatToShort(this.GetDepth(c)));
			}
			Scribe_Values.LookValue<string>(ref compressedString, "depthGrid", null, false);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.totalDepth = 0.0;
				foreach (GridSaveUtility.LoadedGridShort current in GridSaveUtility.LoadedUShortGrid(compressedString))
				{
					ushort val = current.val;
					this.depthGrid[CellIndices.CellToIndex(current.cell)] = SnowGrid.SnowShortToFloat(val);
					this.totalDepth += (double)val;
				}
			}
		}

		private static ushort SnowFloatToShort(float depth)
		{
			depth = Mathf.Clamp(depth, 0f, 1f);
			depth *= 65535f;
			return (ushort)Mathf.RoundToInt(depth);
		}

		private static float SnowShortToFloat(ushort depth)
		{
			return (float)depth / 65535f;
		}

		private static bool CanHaveSnow(int ind)
		{
			Building building = Find.EdificeGrid[ind];
			if (building != null && !SnowGrid.CanCoexistWithSnow(building.def))
			{
				return false;
			}
			TerrainDef terrainDef = Find.TerrainGrid.TerrainAt(ind);
			return terrainDef == null || terrainDef.holdSnow;
		}

		public static bool CanCoexistWithSnow(ThingDef def)
		{
			return def.category != ThingCategory.Building || def.Fillage != FillCategory.Full;
		}

		public void AddDepth(IntVec3 c, float depthToAdd)
		{
			int num = CellIndices.CellToIndex(c);
			float num2 = this.depthGrid[num];
			if (num2 <= 0f && depthToAdd < 0f)
			{
				return;
			}
			if (num2 >= 0.999f && depthToAdd > 1f)
			{
				return;
			}
			if (!SnowGrid.CanHaveSnow(num))
			{
				this.depthGrid[num] = 0f;
				return;
			}
			float num3 = num2 + depthToAdd;
			num3 = Mathf.Clamp(num3, 0f, 1f);
			float num4 = num3 - num2;
			this.totalDepth += (double)num4;
			if (Mathf.Abs(num4) > 0.0001f)
			{
				this.depthGrid[num] = num3;
				this.CheckVisualOrPathCostChange(c, num2, num3);
			}
		}

		public void SetDepth(IntVec3 c, float newDepth)
		{
			int num = CellIndices.CellToIndex(c);
			if (!SnowGrid.CanHaveSnow(num))
			{
				this.depthGrid[num] = 0f;
				return;
			}
			newDepth = Mathf.Clamp(newDepth, 0f, 1f);
			float num2 = this.depthGrid[num];
			this.depthGrid[num] = newDepth;
			float num3 = newDepth - num2;
			this.totalDepth += (double)num3;
			this.CheckVisualOrPathCostChange(c, num2, newDepth);
		}

		private void CheckVisualOrPathCostChange(IntVec3 c, float oldDepth, float newDepth)
		{
			if (!Mathf.Approximately(oldDepth, newDepth))
			{
				if (Mathf.Abs(oldDepth - newDepth) > 0.15f || Rand.Value < 0.0125f)
				{
					Find.MapDrawer.MapMeshDirty(c, MapMeshFlag.Snow, true, false);
					Find.MapDrawer.MapMeshDirty(c, MapMeshFlag.Things, true, false);
				}
				else if (newDepth == 0f)
				{
					Find.MapDrawer.MapMeshDirty(c, MapMeshFlag.Snow, true, false);
				}
				if (SnowUtility.GetSnowCategory(oldDepth) != SnowUtility.GetSnowCategory(newDepth))
				{
					Find.PathGrid.RecalculatePerceivedPathCostAt(c);
				}
			}
		}

		public float GetDepth(IntVec3 c)
		{
			if (!c.InBounds())
			{
				return 0f;
			}
			return this.depthGrid[CellIndices.CellToIndex(c)];
		}

		public SnowCategory GetCategory(IntVec3 c)
		{
			return SnowUtility.GetSnowCategory(this.GetDepth(c));
		}
	}
}
