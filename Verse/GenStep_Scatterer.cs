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

		public float minSpacing = 10f;

		public bool spotMustBeStandable;

		public int minDistToPlayerStart;

		public int minEdgeDist;

		public int extraNoBuildEdgeDist;

		public bool buildableAreaOnly;

		public List<ScattererValidator> validators = new List<ScattererValidator>();

		public bool warnOnFail = true;

		[Unsaved]
		protected HashSet<IntVec3> usedSpots = new HashSet<IntVec3>();

		public override void Generate()
		{
			int num = this.CalculateFinalCount();
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec;
				if (!this.TryFindScatterCell(out intVec))
				{
					return;
				}
				this.ScatterAt(intVec, 1);
				this.usedSpots.Add(intVec);
			}
		}

		protected virtual bool TryFindScatterCell(out IntVec3 result)
		{
			if (this.nearPlayerStart)
			{
				result = CellFinder.RandomClosewalkCellNear(MapGenerator.PlayerStartSpot, 20);
				return true;
			}
			if (CellFinderLoose.TryFindRandomNotEdgeCellWith(5, new Predicate<IntVec3>(this.CanScatterAt), out result))
			{
				return true;
			}
			if (this.warnOnFail)
			{
				Log.Warning("Scatterer " + this.ToString() + " could not find cell to generate at.");
			}
			return false;
		}

		protected abstract void ScatterAt(IntVec3 loc, int count = 1);

		protected virtual bool CanScatterAt(IntVec3 loc)
		{
			if (this.buildableAreaOnly && loc.InNoBuildEdgeArea())
			{
				return false;
			}
			if (this.extraNoBuildEdgeDist > 0 && loc.CloseToEdge(this.extraNoBuildEdgeDist + 10))
			{
				return false;
			}
			if (this.minEdgeDist > 0 && loc.CloseToEdge(this.minEdgeDist))
			{
				return false;
			}
			foreach (IntVec3 current in this.usedSpots)
			{
				if ((current - loc).LengthHorizontal < this.minSpacing)
				{
					bool result = false;
					return result;
				}
			}
			if ((Find.Map.Center - loc).LengthHorizontalSquared < (float)(this.minDistToPlayerStart * this.minDistToPlayerStart))
			{
				return false;
			}
			if (this.spotMustBeStandable && !loc.Standable())
			{
				return false;
			}
			if (this.validators != null)
			{
				for (int i = 0; i < this.validators.Count; i++)
				{
					if (!this.validators[i].Allows(loc))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected int CalculateFinalCount()
		{
			if (this.count < 0)
			{
				return GenStep_Scatterer.CountFromPer10kCells(this.countPer10kCellsRange.RandomInRange, -1);
			}
			return this.count;
		}

		public static int CountFromPer10kCells(float countPer10kCells, int mapSize = -1)
		{
			if (mapSize < 0)
			{
				mapSize = Find.Map.Size.x;
			}
			int num = Mathf.RoundToInt(10000f / countPer10kCells);
			return Mathf.RoundToInt((float)(mapSize * mapSize) / (float)num);
		}

		public void DebugForceScatterAt(IntVec3 loc)
		{
			this.ScatterAt(loc, 1);
		}
	}
}
