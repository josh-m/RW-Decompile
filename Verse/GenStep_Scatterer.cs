using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class GenStep_Scatterer : GenStep
	{
		private const int ScatterNearPlayerRadius = 20;

		public int count = -1;

		public FloatRange countPer10kCellsRange = FloatRange.Zero;

		public bool nearPlayerStart;

		public bool nearMapCenter;

		public float minSpacing = 10f;

		public bool spotMustBeStandable;

		public int minDistToPlayerStart;

		public int minEdgeDist;

		public int extraNoBuildEdgeDist;

		public List<ScattererValidator> validators = new List<ScattererValidator>();

		public bool allowOnWater = true;

		public bool warnOnFail = true;

		[Unsaved]
		protected List<IntVec3> usedSpots = new List<IntVec3>();

		public override void Generate(Map map)
		{
			if (!this.allowOnWater && map.TileInfo.WaterCovered)
			{
				return;
			}
			int num = this.CalculateFinalCount(map);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec;
				if (!this.TryFindScatterCell(map, out intVec))
				{
					return;
				}
				this.ScatterAt(intVec, map, 1);
				this.usedSpots.Add(intVec);
			}
			this.usedSpots.Clear();
		}

		protected virtual bool TryFindScatterCell(Map map, out IntVec3 result)
		{
			if (this.nearMapCenter)
			{
				if (RCellFinder.TryFindRandomCellNearWith(map.Center, (IntVec3 x) => this.CanScatterAt(x, map), map, out result, 3))
				{
					return true;
				}
			}
			else
			{
				if (this.nearPlayerStart)
				{
					result = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, map, 20);
					return true;
				}
				if (CellFinderLoose.TryFindRandomNotEdgeCellWith(5, (IntVec3 x) => this.CanScatterAt(x, map), map, out result))
				{
					return true;
				}
			}
			if (this.warnOnFail)
			{
				Log.Warning("Scatterer " + this.ToString() + " could not find cell to generate at.");
			}
			return false;
		}

		protected abstract void ScatterAt(IntVec3 loc, Map map, int count = 1);

		protected virtual bool CanScatterAt(IntVec3 loc, Map map)
		{
			if (this.extraNoBuildEdgeDist > 0 && loc.CloseToEdge(map, this.extraNoBuildEdgeDist + 10))
			{
				return false;
			}
			if (this.minEdgeDist > 0 && loc.CloseToEdge(map, this.minEdgeDist))
			{
				return false;
			}
			if (this.NearUsedSpot(loc, this.minSpacing))
			{
				return false;
			}
			if ((map.Center - loc).LengthHorizontalSquared < (float)(this.minDistToPlayerStart * this.minDistToPlayerStart))
			{
				return false;
			}
			if (this.spotMustBeStandable && !loc.Standable(map))
			{
				return false;
			}
			if (this.validators != null)
			{
				for (int i = 0; i < this.validators.Count; i++)
				{
					if (!this.validators[i].Allows(loc, map))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected bool NearUsedSpot(IntVec3 c, float dist)
		{
			for (int i = 0; i < this.usedSpots.Count; i++)
			{
				if ((this.usedSpots[i] - c).LengthHorizontalSquared <= dist * dist)
				{
					return true;
				}
			}
			return false;
		}

		protected int CalculateFinalCount(Map map)
		{
			if (this.count < 0)
			{
				return GenStep_Scatterer.CountFromPer10kCells(this.countPer10kCellsRange.RandomInRange, map, -1);
			}
			return this.count;
		}

		public static int CountFromPer10kCells(float countPer10kCells, Map map, int mapSize = -1)
		{
			if (mapSize < 0)
			{
				mapSize = map.Size.x;
			}
			int num = Mathf.RoundToInt(10000f / countPer10kCells);
			return Mathf.RoundToInt((float)(mapSize * mapSize) / (float)num);
		}

		public void DebugForceScatterAt(IntVec3 loc)
		{
			this.ScatterAt(loc, Find.VisibleMap, 1);
		}
	}
}
