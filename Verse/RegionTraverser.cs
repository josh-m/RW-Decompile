using System;
using System.Collections.Generic;

namespace Verse
{
	public static class RegionTraverser
	{
		private class BFSWorker
		{
			private Queue<Region> open = new Queue<Region>();

			private int numRegionsProcessed;

			private uint closedIndex = 1u;

			private int closedArrayPos;

			public BFSWorker(int closedArrayPos)
			{
				this.closedArrayPos = closedArrayPos;
			}

			private void QueueNewOpenRegion(Region region)
			{
				if (region.closedIndex[this.closedArrayPos] == this.closedIndex)
				{
					throw new InvalidOperationException("Region is already closed; you can't open it. Region: " + region.ToString());
				}
				this.open.Enqueue(region);
				region.closedIndex[this.closedArrayPos] = this.closedIndex;
			}

			private void FinalizeSearch()
			{
			}

			public void BreadthFirstTraverseWork(Region root, RegionEntryPredicate entryCondition, RegionProcessor regionProcessor, int maxRegions)
			{
				ProfilerThreadCheck.BeginSample("BreadthFirstTraversal");
				this.closedIndex += 1u;
				this.open.Clear();
				this.numRegionsProcessed = 0;
				this.QueueNewOpenRegion(root);
				while (this.open.Count > 0)
				{
					Region region = this.open.Dequeue();
					if (DebugViewSettings.drawRegionTraversal)
					{
						region.Debug_Notify_Traversed();
					}
					ProfilerThreadCheck.BeginSample("regionProcessor");
					if (regionProcessor != null && regionProcessor(region))
					{
						this.FinalizeSearch();
						ProfilerThreadCheck.EndSample();
						ProfilerThreadCheck.EndSample();
						return;
					}
					ProfilerThreadCheck.EndSample();
					this.numRegionsProcessed++;
					if (this.numRegionsProcessed >= maxRegions)
					{
						this.FinalizeSearch();
						ProfilerThreadCheck.EndSample();
						return;
					}
					for (int i = 0; i < region.links.Count; i++)
					{
						RegionLink regionLink = region.links[i];
						for (int j = 0; j < 2; j++)
						{
							Region region2 = regionLink.regions[j];
							if (region2 != null && region2.closedIndex[this.closedArrayPos] != this.closedIndex && (entryCondition == null || entryCondition(region, region2)))
							{
								this.QueueNewOpenRegion(region2);
							}
						}
					}
				}
				this.FinalizeSearch();
				ProfilerThreadCheck.EndSample();
			}
		}

		private static Queue<RegionTraverser.BFSWorker> freeWorkers;

		public static int NumWorkers;

		static RegionTraverser()
		{
			RegionTraverser.freeWorkers = new Queue<RegionTraverser.BFSWorker>();
			RegionTraverser.NumWorkers = 8;
			for (int i = 0; i < RegionTraverser.NumWorkers; i++)
			{
				RegionTraverser.freeWorkers.Enqueue(new RegionTraverser.BFSWorker(i));
			}
		}

		public static Room FloodAndSetRooms(Region root, Map map, Room existingRoom)
		{
			Room floodingRoom;
			if (existingRoom == null)
			{
				floodingRoom = Room.MakeNew(map);
			}
			else
			{
				floodingRoom = existingRoom;
			}
			root.Room = floodingRoom;
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.portal == null && r.Room != floodingRoom;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				r.Room = floodingRoom;
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(root, entryCondition, regionProcessor, 999999);
			return floodingRoom;
		}

		public static void FloodAndSetNewRegionIndex(Region root, int newRegionGroupIndex)
		{
			root.newRegionGroupIndex = newRegionGroupIndex;
			if (root.portal != null)
			{
				return;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.portal == null && r.newRegionGroupIndex < 0;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				r.newRegionGroupIndex = newRegionGroupIndex;
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(root, entryCondition, regionProcessor, 999999);
		}

		public static bool WithinRegions(this IntVec3 A, IntVec3 B, Map map, int regionLookCount, TraverseParms traverseParams)
		{
			if (traverseParams.mode == TraverseMode.PassAnything)
			{
				throw new ArgumentException("traverseParams");
			}
			Region validRegionAt = map.regionGrid.GetValidRegionAt(A);
			if (validRegionAt == null)
			{
				return false;
			}
			Region regB = map.regionGrid.GetValidRegionAt(B);
			if (regB == null)
			{
				return false;
			}
			if (validRegionAt == regB)
			{
				return true;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, false);
			bool found = false;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r == regB)
				{
					found = true;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, regionProcessor, regionLookCount);
			return found;
		}

		public static void MarkRegionsBFS(Region root, RegionEntryPredicate entryCondition, int maxRegions, int inRadiusMark)
		{
			RegionTraverser.BreadthFirstTraverse(root, entryCondition, delegate(Region r)
			{
				r.mark = inRadiusMark;
				return false;
			}, maxRegions);
		}

		public static void BreadthFirstTraverse(IntVec3 start, Map map, RegionEntryPredicate entryCondition, RegionProcessor regionProcessor, int maxRegions = 999999)
		{
			Region region = start.GetRegion(map);
			if (region == null)
			{
				return;
			}
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, maxRegions);
		}

		public static void BreadthFirstTraverse(Region root, RegionEntryPredicate entryCondition, RegionProcessor regionProcessor, int maxRegions = 999999)
		{
			if (RegionTraverser.freeWorkers.Count == 0)
			{
				Log.Error("No free workers for breadth-first traversal. Either BFS recurred deeper than " + RegionTraverser.NumWorkers + ", or a bug has put this system in an inconsistent state. Resetting.");
				return;
			}
			if (root == null)
			{
				Log.Error("BreadthFirstTraverse with null root region.");
				return;
			}
			RegionTraverser.BFSWorker bFSWorker = RegionTraverser.freeWorkers.Dequeue();
			try
			{
				bFSWorker.BreadthFirstTraverseWork(root, entryCondition, regionProcessor, maxRegions);
			}
			catch (Exception ex)
			{
				Log.Error("Exception in BreadthFirstTraverse: " + ex.ToString());
			}
			finally
			{
				RegionTraverser.freeWorkers.Enqueue(bFSWorker);
			}
		}
	}
}
