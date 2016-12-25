using System;
using Verse;

namespace RimWorld
{
	public class StorageSettings : IExposable
	{
		public IStoreSettingsParent owner;

		public ThingFilter filter;

		[LoadAlias("priority")]
		private StoragePriority priorityInt = StoragePriority.Normal;

		private bool OwnerIsSlotGroupParent
		{
			get
			{
				return this.owner is ISlotGroupParent;
			}
		}

		private ISlotGroupParent SlotGroupParentOwner
		{
			get
			{
				return this.owner as ISlotGroupParent;
			}
		}

		public StoragePriority Priority
		{
			get
			{
				return this.priorityInt;
			}
			set
			{
				this.priorityInt = value;
				if (Current.ProgramState == ProgramState.Playing && this.OwnerIsSlotGroupParent && this.SlotGroupParentOwner.Map != null)
				{
					this.SlotGroupParentOwner.Map.slotGroupManager.Notify_GroupChangedPriority();
				}
			}
		}

		public StorageSettings()
		{
			this.filter = new ThingFilter(new Action(this.TryNotifyChanged));
		}

		public StorageSettings(IStoreSettingsParent owner) : this()
		{
			this.owner = owner;
			if (owner != null)
			{
				StorageSettings parentStoreSettings = owner.GetParentStoreSettings();
				if (parentStoreSettings != null)
				{
					this.priorityInt = parentStoreSettings.priorityInt;
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<StoragePriority>(ref this.priorityInt, "priority", StoragePriority.Unstored, false);
			Scribe_Deep.LookDeep<ThingFilter>(ref this.filter, "filter", new object[0]);
		}

		public void SetFromPreset(StorageSettingsPreset preset)
		{
			this.filter.SetFromPreset(preset);
			this.TryNotifyChanged();
		}

		public void CopyFrom(StorageSettings other)
		{
			this.Priority = other.Priority;
			this.filter.CopyAllowancesFrom(other.filter);
			this.TryNotifyChanged();
		}

		public bool AllowedToAccept(Thing t)
		{
			if (!this.filter.Allows(t))
			{
				return false;
			}
			if (this.owner != null)
			{
				StorageSettings parentStoreSettings = this.owner.GetParentStoreSettings();
				if (parentStoreSettings != null && !parentStoreSettings.AllowedToAccept(t))
				{
					return false;
				}
			}
			return true;
		}

		private void TryNotifyChanged()
		{
			if (this.owner != null && this.OwnerIsSlotGroupParent && this.SlotGroupParentOwner.GetSlotGroup() != null)
			{
				this.SlotGroupParentOwner.Map.listerHaulables.Notify_SlotGroupChanged(this.SlotGroupParentOwner.GetSlotGroup());
			}
		}
	}
}
