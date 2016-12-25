using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public sealed class SlotGroupManager
	{
		private List<SlotGroup> allGroups = new List<SlotGroup>();

		private SlotGroup[,,] groupGrid;

		public IEnumerable<SlotGroup> AllGroups
		{
			get
			{
				return this.allGroups;
			}
		}

		public List<SlotGroup> AllGroupsListForReading
		{
			get
			{
				return this.allGroups;
			}
		}

		public List<SlotGroup> AllGroupsListInPriorityOrder
		{
			get
			{
				return this.allGroups;
			}
		}

		public IEnumerable<IntVec3> AllSlots
		{
			get
			{
				for (int i = 0; i < this.allGroups.Count; i++)
				{
					List<IntVec3> cellsList = this.allGroups[i].CellsList;
					int j = 0;
					while (j < cellsList.Count)
					{
						yield return cellsList[j];
						i++;
					}
				}
			}
		}

		public SlotGroupManager()
		{
			this.groupGrid = new SlotGroup[Find.Map.Size.x, Find.Map.Size.y, Find.Map.Size.z];
		}

		public void AddGroup(SlotGroup newGroup)
		{
			if (this.allGroups.Contains(newGroup))
			{
				Log.Error("Double-added slot group. SlotGroup parent is " + newGroup.parent);
				return;
			}
			if ((from g in this.allGroups
			where g.parent == newGroup.parent
			select g).Any<SlotGroup>())
			{
				Log.Error("Added SlotGroup with a parent matching an existing one. Parent is " + newGroup.parent);
				return;
			}
			this.allGroups.Add(newGroup);
			this.allGroups.InsertionSort(new Comparison<SlotGroup>(SlotGroupManager.CompareSlotGroupPrioritiesDescending));
			List<IntVec3> cellsList = newGroup.CellsList;
			for (int i = 0; i < cellsList.Count; i++)
			{
				this.SetCellFor(cellsList[i], newGroup);
			}
			ListerHaulables.Notify_SlotGroupChanged(newGroup);
		}

		public void RemoveGroup(SlotGroup oldGroup)
		{
			if (!this.allGroups.Contains(oldGroup))
			{
				Log.Error("Removing SlotGroup that isn't registered.");
				return;
			}
			this.allGroups.Remove(oldGroup);
			List<IntVec3> cellsList = oldGroup.CellsList;
			for (int i = 0; i < cellsList.Count; i++)
			{
				IntVec3 intVec = cellsList[i];
				this.groupGrid[intVec.x, intVec.y, intVec.z] = null;
			}
			ListerHaulables.Notify_SlotGroupChanged(oldGroup);
		}

		public void Notify_GroupChangedPriority()
		{
			this.allGroups.InsertionSort(new Comparison<SlotGroup>(SlotGroupManager.CompareSlotGroupPrioritiesDescending));
		}

		public SlotGroup SlotGroupAt(IntVec3 loc)
		{
			return this.groupGrid[loc.x, loc.y, loc.z];
		}

		public void SetCellFor(IntVec3 c, SlotGroup group)
		{
			if (this.SlotGroupAt(c) != null)
			{
				Log.Error(string.Concat(new object[]
				{
					group,
					" overwriting slot group square ",
					c,
					" of ",
					this.SlotGroupAt(c)
				}));
			}
			this.groupGrid[c.x, c.y, c.z] = group;
		}

		public void ClearCellFor(IntVec3 c, SlotGroup group)
		{
			if (this.SlotGroupAt(c) != group)
			{
				Log.Error(string.Concat(new object[]
				{
					group,
					" clearing group grid square ",
					c,
					" containing ",
					this.SlotGroupAt(c)
				}));
			}
			this.groupGrid[c.x, c.y, c.z] = null;
		}

		private static int CompareSlotGroupPrioritiesDescending(SlotGroup a, SlotGroup b)
		{
			return ((int)b.Settings.Priority).CompareTo((int)a.Settings.Priority);
		}
	}
}
