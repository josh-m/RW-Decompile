using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Zone_Stockpile : Zone, IStoreSettingsParent, ISlotGroupParent
	{
		public StorageSettings settings;

		public SlotGroup slotGroup;

		private static readonly ITab StorageTab = new ITab_Storage();

		public bool StorageTabVisible
		{
			get
			{
				return true;
			}
		}

		protected override Color NextZoneColor
		{
			get
			{
				return ZoneColorUtility.NextStorageZoneColor();
			}
		}

		public Zone_Stockpile()
		{
		}

		public Zone_Stockpile(StorageSettingsPreset preset) : base(preset.PresetName())
		{
			this.settings = new StorageSettings(this);
			this.settings.SetFromPreset(preset);
			this.slotGroup = new SlotGroup(this);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.LookDeep<StorageSettings>(ref this.settings, "settings", new object[]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.slotGroup = new SlotGroup(this);
			}
		}

		public override void AddCell(IntVec3 sq)
		{
			base.AddCell(sq);
			if (this.slotGroup != null)
			{
				this.slotGroup.Notify_AddedCell(sq);
			}
		}

		public override void RemoveCell(IntVec3 sq)
		{
			base.RemoveCell(sq);
			this.slotGroup.Notify_LostCell(sq);
		}

		public override void Deregister()
		{
			base.Deregister();
			this.slotGroup.Notify_ParentDestroying();
		}

		[DebuggerHidden]
		public override IEnumerable<ITab> GetInspectionTabs()
		{
			yield return Zone_Stockpile.StorageTab;
		}

		[DebuggerHidden]
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			foreach (Gizmo g2 in StorageSettingsClipboard.CopyPasteGizmosFor(this.settings))
			{
				yield return g2;
			}
		}

		public SlotGroup GetSlotGroup()
		{
			return this.slotGroup;
		}

		[DebuggerHidden]
		public IEnumerable<IntVec3> AllSlotCells()
		{
			for (int i = 0; i < this.cells.Count; i++)
			{
				yield return this.cells[i];
			}
		}

		public List<IntVec3> AllSlotCellsList()
		{
			return this.cells;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return null;
		}

		public StorageSettings GetStoreSettings()
		{
			return this.settings;
		}

		public string SlotYielderLabel()
		{
			return this.label;
		}

		public void Notify_ReceivedThing(Thing newItem)
		{
			if (newItem.def.storedConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(newItem.def.storedConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
		}

		public void Notify_LostThing(Thing newItem)
		{
		}
	}
}
