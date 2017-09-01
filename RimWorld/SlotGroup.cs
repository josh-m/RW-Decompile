using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class SlotGroup
	{
		public ISlotGroupParent parent;

		private Map Map
		{
			get
			{
				return this.parent.Map;
			}
		}

		public StorageSettings Settings
		{
			get
			{
				return this.parent.GetStoreSettings();
			}
		}

		public IEnumerable<Thing> HeldThings
		{
			get
			{
				List<IntVec3> cellsList = this.CellsList;
				for (int i = 0; i < cellsList.Count; i++)
				{
					List<Thing> thingList = this.Map.thingGrid.ThingsListAt(cellsList[i]);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].def.EverStoreable)
						{
							yield return thingList[j];
						}
					}
				}
			}
		}

		public List<IntVec3> CellsList
		{
			get
			{
				return this.parent.AllSlotCellsList();
			}
		}

		public SlotGroup(ISlotGroupParent parent)
		{
			this.parent = parent;
			this.Map.slotGroupManager.AddGroup(this);
		}

		[DebuggerHidden]
		public IEnumerator<IntVec3> GetEnumerator()
		{
			for (int i = 0; i < this.CellsList.Count; i++)
			{
				yield return this.CellsList[i];
			}
		}

		public void Notify_AddedCell(IntVec3 c)
		{
			this.Map.slotGroupManager.SetCellFor(c, this);
			this.Map.listerHaulables.RecalcAllInCell(c);
		}

		public void Notify_LostCell(IntVec3 c)
		{
			this.Map.slotGroupManager.ClearCellFor(c, this);
			this.Map.listerHaulables.RecalcAllInCell(c);
		}

		public void Notify_ParentDestroying()
		{
			this.Map.slotGroupManager.RemoveGroup(this);
		}

		public override string ToString()
		{
			if (this.parent != null)
			{
				return this.parent.ToString();
			}
			return "NullParent";
		}
	}
}
