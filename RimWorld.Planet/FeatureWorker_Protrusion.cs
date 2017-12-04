using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public abstract class FeatureWorker_Protrusion : FeatureWorker
	{
		private List<int> roots = new List<int>();

		private HashSet<int> rootsSet = new HashSet<int>();

		private List<int> rootsWithoutSmallPassages = new List<int>();

		private HashSet<int> rootsWithoutSmallPassagesSet = new HashSet<int>();

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

		protected virtual int MaxPassageWidth
		{
			get
			{
				return this.def.maxPassageWidth;
			}
		}

		protected virtual float MaxPctOfWholeArea
		{
			get
			{
				return this.def.maxPctOfWholeArea;
			}
		}

		protected abstract bool IsRoot(int tile);

		protected virtual bool IsMember(int tile)
		{
			return Find.WorldGrid[tile].feature == null;
		}

		public override void GenerateWhereAppropriate()
		{
			this.CalculateRoots();
			this.CalculateRootsWithoutSmallPassages();
			this.CalculateContiguousGroups();
		}

		private void CalculateRoots()
		{
			this.roots.Clear();
			int tilesCount = Find.WorldGrid.TilesCount;
			for (int i = 0; i < tilesCount; i++)
			{
				if (this.IsRoot(i))
				{
					this.roots.Add(i);
				}
			}
			this.rootsSet.Clear();
			this.rootsSet.AddRange(this.roots);
		}

		private void CalculateRootsWithoutSmallPassages()
		{
			this.rootsWithoutSmallPassages.Clear();
			this.rootsWithoutSmallPassages.AddRange(this.roots);
			GenPlanetMorphology.Open(this.rootsWithoutSmallPassages, this.MaxPassageWidth);
			this.rootsWithoutSmallPassagesSet.Clear();
			this.rootsWithoutSmallPassagesSet.AddRange(this.rootsWithoutSmallPassages);
		}

		private void CalculateContiguousGroups()
		{
			WorldGrid worldGrid = Find.WorldGrid;
			WorldFloodFiller worldFloodFiller = Find.WorldFloodFiller;
			int minSize = this.MinSize;
			int maxSize = this.MaxSize;
			float maxPctOfWholeArea = this.MaxPctOfWholeArea;
			int maxPassageWidth = this.MaxPassageWidth;
			FeatureWorker.ClearVisited();
			FeatureWorker.ClearGroupSizes();
			for (int i = 0; i < this.roots.Count; i++)
			{
				int num = this.roots[i];
				if (!FeatureWorker.visited[num])
				{
					FeatureWorker_Protrusion.tmpGroup.Clear();
					worldFloodFiller.FloodFill(num, (int x) => this.rootsSet.Contains(x), delegate(int x)
					{
						FeatureWorker.visited[x] = true;
						FeatureWorker_Protrusion.tmpGroup.Add(x);
					}, 2147483647, null);
					for (int j = 0; j < FeatureWorker_Protrusion.tmpGroup.Count; j++)
					{
						FeatureWorker.groupSize[FeatureWorker_Protrusion.tmpGroup[j]] = FeatureWorker_Protrusion.tmpGroup.Count;
					}
				}
			}
			FeatureWorker.ClearVisited();
			for (int k = 0; k < this.rootsWithoutSmallPassages.Count; k++)
			{
				int num2 = this.rootsWithoutSmallPassages[k];
				if (!FeatureWorker.visited[num2])
				{
					this.currentGroup.Clear();
					worldFloodFiller.FloodFill(num2, (int x) => this.rootsWithoutSmallPassagesSet.Contains(x), delegate(int x)
					{
						FeatureWorker.visited[x] = true;
						this.currentGroup.Add(x);
					}, 2147483647, null);
					if (this.currentGroup.Count >= minSize)
					{
						GenPlanetMorphology.Dilate(this.currentGroup, maxPassageWidth * 2, (int x) => this.rootsSet.Contains(x));
						if (this.currentGroup.Count <= maxSize)
						{
							float num3 = (float)this.currentGroup.Count / (float)FeatureWorker.groupSize[num2];
							if (num3 <= maxPctOfWholeArea)
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
