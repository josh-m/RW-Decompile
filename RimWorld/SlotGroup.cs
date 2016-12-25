using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class SlotGroup
	{
		public ISlotGroupParent parent;

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
				for (int i = 0; i < this.CellsList.Count; i++)
				{
					List<Thing> thingList = Find.ThingGrid.ThingsListAt(this.CellsList[i]);
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
			Find.SlotGroupManager.AddGroup(this);
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
			Find.SlotGroupManager.SetCellFor(c, this);
			ListerHaulables.RecalcAllInCell(c);
		}

		public void Notify_LostCell(IntVec3 c)
		{
			Find.SlotGroupManager.ClearCellFor(c, this);
			ListerHaulables.RecalcAllInCell(c);
		}

		public void Notify_ParentDestroying()
		{
			Find.SlotGroupManager.RemoveGroup(this);
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
