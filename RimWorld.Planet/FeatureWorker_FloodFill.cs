using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class FeatureWorker_FloodFill : FeatureWorker
	{
		private List<int> roots = new List<int>();

		private HashSet<int> rootsSet = new HashSet<int>();

		private List<int> possiblyAllowed = new List<int>();

		private HashSet<int> possiblyAllowedSet = new HashSet<int>();

		private List<int> currentGroup = new List<int>();

		private List<int> currentGroupMembers = new List<int>();

		private static List<int> tmpGroup = new List<int>();

		protected virtual int MinSize
		{
			get
			{
				return this.def.minSize;
			}
		}

		protected virtual int MaxSize
		{
			get
			{
				return this.def.maxSize;
			}
		}

		protected virtual int MaxPossiblyAllowedSizeToTake
		{
			get
			{
				return this.def.maxPossiblyAllowedSizeToTake;
			}
		}

		protected virtual float MaxPossiblyAllowedSizePctOfMeToTake
		{
			get
			{
				return this.def.maxPossiblyAllowedSizePctOfMeToTake;
			}
		}

		protected abstract bool IsRoot(int tile);

		protected virtual bool IsPossiblyAllowed(int tile)
		{
			return false;
		}

		protected virtual bool IsMember(int tile)
		{
			return Find.WorldGrid[tile].feature == null;
		}

		public override void GenerateWhereAppropriate()
		{
			this.CalculateRootsAndPossiblyAllowedTiles();
			this.CalculateContiguousGroups();
		}

		private void CalculateRootsAndPossiblyAllowedTiles()
		{
			this.roots.Clear();
			this.possiblyAllowed.Clear();
			int tilesCount = Find.WorldGrid.TilesCount;
			for (int i = 0; i < tilesCount; i++)
			{
				if (this.IsRoot(i))
				{
					this.roots.Add(i);
				}
				if (this.IsPossiblyAllowed(i))
				{
					this.possiblyAllowed.Add(i);
				}
			}
			this.rootsSet.Clear();
			this.rootsSet.AddRange(this.roots);
			this.possiblyAllowedSet.Clear();
			this.possiblyAllowedSet.AddRange(this.possiblyAllowed);
		}

		private void CalculateContiguousGroups()
		{
			WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
			WorldGrid worldGrid = Find.WorldGrid;
			int tilesCount = worldGrid.TilesCount;
			int minSize = this.MinSize;
			int maxSize = this.MaxSize;
			int maxPossiblyAllowedSizeToTake = this.MaxPossiblyAllowedSizeToTake;
			float maxPossiblyAllowedSizePctOfMeToTake = this.MaxPossiblyAllowedSizePctOfMeToTake;
			FeatureWorker.ClearVisited();
			FeatureWorker.ClearGroupSizes();
			for (int i = 0; i < this.possiblyAllowed.Count; i++)
			{
				int num = this.possiblyAllowed[i];
				if (!FeatureWorker.visited[num])
				{
					if (!this.rootsSet.Contains(num))
					{
						FeatureWorker_FloodFill.tmpGroup.Clear();
						worldFloodFiller.FloodFill(num, (int x) => this.possiblyAllowedSet.Contains(x) && !this.rootsSet.Contains(x), delegate(int x)
						{
							FeatureWorker.visited[x] = true;
							FeatureWorker_FloodFill.tmpGroup.Add(x);
						}, 2147483647, null);
						for (int j = 0; j < FeatureWorker_FloodFill.tmpGroup.Count; j++)
						{
							FeatureWorker.groupSize[FeatureWorker_FloodFill.tmpGroup[j]] = FeatureWorker_FloodFill.tmpGroup.Count;
						}
					}
				}
			}
			for (int k = 0; k < this.roots.Count; k++)
			{
				int num2 = this.roots[k];
				if (!FeatureWorker.visited[num2])
				{
					int initialMembersCountClamped = 0;
					worldFloodFiller.FloodFill(num2, (int x) => (this.rootsSet.Contains(x) || this.possiblyAllowedSet.Contains(x)) && this.IsMember(x), delegate(int x)
					{
						FeatureWorker.visited[x] = true;
						initialMembersCountClamped++;
						return initialMembersCountClamped >= minSize;
					}, 2147483647, null);
					if (initialMembersCountClamped >= minSize)
					{
						int initialRootsCount = 0;
						worldFloodFiller.FloodFill(num2, (int x) => this.rootsSet.Contains(x), delegate(int x)
						{
							FeatureWorker.visited[x] = true;
							initialRootsCount++;
						}, 2147483647, null);
						if (initialRootsCount >= minSize && initialRootsCount <= maxSize)
						{
							int traversedRootsCount = 0;
							this.currentGroup.Clear();
							worldFloodFiller.FloodFill(num2, (int x) => this.rootsSet.Contains(x) || (this.possiblyAllowedSet.Contains(x) && FeatureWorker.groupSize[x] <= maxPossiblyAllowedSizeToTake && (float)FeatureWorker.groupSize[x] <= maxPossiblyAllowedSizePctOfMeToTake * (float)Mathf.Max(traversedRootsCount, initialRootsCount) && FeatureWorker.groupSize[x] < maxSize), delegate(int x)
							{
								FeatureWorker.visited[x] = true;
								if (this.rootsSet.Contains(x))
								{
									traversedRootsCount++;
								}
								this.currentGroup.Add(x);
							}, 2147483647, null);
							if (this.currentGroup.Count >= minSize && this.currentGroup.Count <= maxSize)
							{
								if (this.def.canTouchWorldEdge || !this.currentGroup.Any((int x) => worldGrid.IsOnEdge(x)))
								{
									this.currentGroupMembers.Clear();
									for (int l = 0; l < this.currentGroup.Count; l++)
									{
										if (this.IsMember(this.currentGroup[l]))
										{
											this.currentGroupMembers.Add(this.currentGroup[l]);
										}
									}
									if (this.currentGroupMembers.Count >= minSize)
									{
										if (this.currentGroup.Any((int x) => worldGrid[x].feature == null))
										{
											this.currentGroup.RemoveAll((int x) => worldGrid[x].feature != null);
										}
										base.AddFeature(this.currentGroupMembers, this.currentGroup);
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
