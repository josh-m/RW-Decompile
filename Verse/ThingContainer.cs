using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public sealed class ThingContainer : IEnumerable, IEnumerable<Thing>, IExposable
	{
		public IThingContainerOwner owner;

		private int maxStacks = 99999;

		private LookMode contentsLookMode = LookMode.Deep;

		private List<Thing> innerList = new List<Thing>();

		public Thing this[int index]
		{
			get
			{
				return this.innerList[index];
			}
			set
			{
				this.innerList[index] = value;
			}
		}

		public int Count
		{
			get
			{
				return this.innerList.Count;
			}
		}

		public int TotalStackCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.innerList.Count; i++)
				{
					num += this.innerList[i].stackCount;
				}
				return num;
			}
		}

		public int AvailableStackSpace
		{
			get
			{
				if (this.maxStacks == 1 && this.innerList.Count == 1)
				{
					return this.innerList[0].def.stackLimit - this.innerList[0].stackCount;
				}
				return 99999;
			}
		}

		public string ContentsString
		{
			get
			{
				if (this.Count == 0)
				{
					return "NothingLower".Translate();
				}
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < this.innerList.Count; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(this.innerList[i].Label);
				}
				return stringBuilder.ToString();
			}
		}

		public ThingContainer()
		{
			this.maxStacks = 99999;
		}

		public ThingContainer(IThingContainerOwner owner)
		{
			this.owner = owner;
		}

		public ThingContainer(IThingContainerOwner owner, bool oneStackOnly, LookMode contentsLookMode = LookMode.Deep) : this(owner)
		{
			this.maxStacks = ((!oneStackOnly) ? 99999 : 1);
			this.contentsLookMode = contentsLookMode;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<Thing> GetEnumerator()
		{
			return this.innerList.GetEnumerator();
		}

		public void ExposeData()
		{
			Scribe_Values.LookValue<int>(ref this.maxStacks, "maxStacks", 99999, false);
			Scribe_Values.LookValue<LookMode>(ref this.contentsLookMode, "contentsLookMode", LookMode.Deep, false);
			Scribe_Collections.LookList<Thing>(ref this.innerList, true, "innerList", this.contentsLookMode, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = this.innerList.Count - 1; i >= 0; i--)
				{
					if (this.innerList[i] == null)
					{
						Log.Error("ThingContainer contained null.");
						this.innerList.RemoveAt(i);
					}
					else
					{
						this.innerList[i].holdingContainer = this;
					}
				}
			}
		}

		public void ThingContainerTick()
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				if (this.innerList[i].def.tickerType == TickerType.Normal)
				{
					this.innerList[i].Tick();
				}
			}
		}

		public void ThingContainerTickRare()
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				if (this.innerList[i].def.tickerType == TickerType.Rare)
				{
					this.innerList[i].TickRare();
				}
			}
		}

		public void Clear()
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				this.Remove(this.innerList[i]);
			}
		}

		public void ClearAndDestroyContents(DestroyMode mode = DestroyMode.Vanish)
		{
			while (this.innerList.Any<Thing>())
			{
				for (int i = this.innerList.Count - 1; i >= 0; i--)
				{
					Thing thing = this.innerList[i];
					thing.Destroy(mode);
					this.Remove(thing);
				}
			}
		}

		public void ClearAndDestroyContentsOrPassToWorld(DestroyMode mode = DestroyMode.Vanish)
		{
			while (this.innerList.Any<Thing>())
			{
				for (int i = this.innerList.Count - 1; i >= 0; i--)
				{
					Thing thing = this.innerList[i];
					thing.DestroyOrPassToWorld(mode);
					this.Remove(thing);
				}
			}
		}

		public bool CanAcceptAnyOf(Thing item)
		{
			return this.innerList.Count < this.maxStacks;
		}

		public int TryAdd(Thing item, int count)
		{
			if (item == null)
			{
				Log.Warning("Tried to add null item to ThingContainer.");
				return 0;
			}
			if (this.Contains(item))
			{
				Log.Warning("Tried to add " + item + " to ThingContainer but this item is already here.");
				return 0;
			}
			int num = Mathf.Min(count, this.AvailableStackSpace);
			if (num <= 0)
			{
				return 0;
			}
			Thing item2 = item.SplitOff(num);
			if (!this.TryAdd(item2, true))
			{
				return 0;
			}
			return num;
		}

		public bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
		{
			if (item == null)
			{
				Log.Warning("Tried to add null item to ThingContainer.");
				return false;
			}
			if (this.Contains(item))
			{
				Log.Warning("Tried to add " + item + " to ThingContainer but this item is already here.");
				return false;
			}
			if (item.stackCount > this.AvailableStackSpace)
			{
				return this.TryAdd(item, this.AvailableStackSpace) > 0;
			}
			SlotGroupUtility.Notify_TakingThing(item);
			if (canMergeWithExistingStacks && item.def.stackLimit > 1)
			{
				for (int i = 0; i < this.innerList.Count; i++)
				{
					if (this.innerList[i].def == item.def)
					{
						int num = item.stackCount;
						if (num > this.AvailableStackSpace)
						{
							num = this.AvailableStackSpace;
						}
						Thing other = item.SplitOff(num);
						if (!this.innerList[i].TryAbsorbStack(other, false))
						{
							Log.Error("ThingContainer did TryAbsorbStack " + item + " but could not absorb stack.");
						}
					}
					if (item.Destroyed)
					{
						return true;
					}
				}
			}
			if (this.innerList.Count >= this.maxStacks)
			{
				return false;
			}
			if (item.Spawned)
			{
				item.DeSpawn();
			}
			if (item.HasAttachment(ThingDefOf.Fire))
			{
				item.GetAttachment(ThingDefOf.Fire).Destroy(DestroyMode.Vanish);
			}
			item.holdingContainer = this;
			this.innerList.Add(item);
			return true;
		}

		public void TryAddMany(IEnumerable<Thing> things)
		{
			List<Thing> list = things as List<Thing>;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.TryAdd(list[i], true);
				}
			}
			else
			{
				foreach (Thing current in things)
				{
					this.TryAdd(current, true);
				}
			}
		}

		public bool Contains(Thing item)
		{
			return this.innerList.Contains(item);
		}

		public void Remove(Thing item)
		{
			if (!this.Contains(item))
			{
				return;
			}
			if (item.holdingContainer == this)
			{
				item.holdingContainer = null;
			}
			this.innerList.Remove(item);
			Pawn_InventoryTracker pawn_InventoryTracker = this.owner as Pawn_InventoryTracker;
			if (pawn_InventoryTracker != null)
			{
				pawn_InventoryTracker.Notify_ItemRemoved(item);
			}
		}

		public void RemoveAll(Predicate<Thing> match)
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				if (match(this.innerList[i]))
				{
					this.Remove(this.innerList[i]);
				}
			}
		}

		public void TransferToContainer(Thing item, ThingContainer otherContainer, int stackCount)
		{
			Thing thing;
			this.TransferToContainer(item, otherContainer, stackCount, out thing);
		}

		public void TransferToContainer(Thing item, ThingContainer otherContainer, int stackCount, out Thing resultingTransferredItem)
		{
			Thing thing = item.SplitOff(stackCount);
			if (this.Contains(thing))
			{
				this.Remove(thing);
			}
			otherContainer.TryAdd(thing, true);
			resultingTransferredItem = thing;
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].designationManager.RemoveAllDesignationsOn(thing, false);
			}
			Thing thing2 = otherContainer.owner as Thing;
			if (thing2 != null && thing2.Spawned)
			{
				item.def.soundPickup.PlayOneShot(new TargetInfo(thing2.Position, thing2.Map, false));
			}
		}

		public Thing Get(Thing thing, int count)
		{
			if (count > thing.stackCount)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to get ",
					count,
					" of ",
					thing,
					" while only having ",
					thing.stackCount
				}));
				count = thing.stackCount;
			}
			if (count == thing.stackCount)
			{
				this.Remove(thing);
				return thing;
			}
			Thing thing2 = thing.SplitOff(count);
			thing2.holdingContainer = null;
			return thing2;
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (thing.stackCount < count)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to drop ",
					count,
					" of ",
					thing,
					" while only having ",
					thing.stackCount
				}));
				count = thing.stackCount;
			}
			if (count == thing.stackCount)
			{
				if (GenDrop.TryDropSpawn(thing, dropLoc, map, mode, out resultingThing, placedAction))
				{
					this.Remove(thing);
					return true;
				}
				return false;
			}
			else
			{
				Thing thing2 = thing.SplitOff(count);
				if (GenDrop.TryDropSpawn(thing2, dropLoc, map, mode, out resultingThing, placedAction))
				{
					return true;
				}
				thing.stackCount += thing2.stackCount;
				return false;
			}
		}

		public bool TryDrop(Thing thing, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
		{
			if (this.owner.GetMap() == null)
			{
				Log.Error("Cannot drop " + thing + " without a dropLoc and with an owner whose map is null.");
				lastResultingThing = null;
				return false;
			}
			return this.TryDrop(thing, this.owner.GetPosition(), this.owner.GetMap(), mode, out lastResultingThing, placedAction);
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
		{
			if (!this.innerList.Contains(thing))
			{
				Log.Error(string.Concat(new object[]
				{
					this.owner,
					" container tried to drop  ",
					thing,
					" which it didn't contain."
				}));
				lastResultingThing = null;
				return false;
			}
			if (GenDrop.TryDropSpawn(thing, dropLoc, map, mode, out lastResultingThing, placedAction))
			{
				this.Remove(thing);
				return true;
			}
			return false;
		}

		public bool TryDropAll(IntVec3 dropLoc, Map map, ThingPlaceMode mode)
		{
			bool result = true;
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				Thing thing;
				if (!this.TryDrop(this.innerList[i], dropLoc, map, mode, out thing, null))
				{
					result = false;
				}
			}
			return result;
		}

		public bool Contains(ThingDef def)
		{
			return this.Contains(def, 1);
		}

		public bool Contains(ThingDef def, int minCount)
		{
			if (minCount <= 0)
			{
				return true;
			}
			int num = 0;
			for (int i = 0; i < this.innerList.Count; i++)
			{
				if (this.innerList[i].def == def)
				{
					num += this.innerList[i].stackCount;
				}
				if (num >= minCount)
				{
					return true;
				}
			}
			return false;
		}

		public int TotalStackCountOfDef(ThingDef def)
		{
			int num = 0;
			for (int i = 0; i < this.innerList.Count; i++)
			{
				if (this.innerList[i].def == def)
				{
					num += this.innerList[i].stackCount;
				}
			}
			return num;
		}

		public void Notify_ContainedItemDestroyed(Thing t)
		{
			if (!(this.owner is Corpse))
			{
				this.Remove(t);
			}
		}
	}
}
