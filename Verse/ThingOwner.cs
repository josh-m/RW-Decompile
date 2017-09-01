using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class ThingOwner<T> : ThingOwner, IEnumerable, IList<T>, ICollection<T>, IEnumerable<T> where T : Thing
	{
		private List<T> innerList = new List<T>();

		T IList<T>.this[int index]
		{
			get
			{
				return this.innerList[index];
			}
			set
			{
				throw new InvalidOperationException("ThingOwner doesn't allow setting individual elements.");
			}
		}

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public List<T> InnerListForReading
		{
			get
			{
				return this.innerList;
			}
		}

		public new T this[int index]
		{
			get
			{
				return this.innerList[index];
			}
		}

		public override int Count
		{
			get
			{
				return this.innerList.Count;
			}
		}

		public ThingOwner()
		{
		}

		public ThingOwner(IThingHolder owner) : base(owner)
		{
		}

		public ThingOwner(IThingHolder owner, bool oneStackOnly, LookMode contentsLookMode = LookMode.Deep) : base(owner, oneStackOnly, contentsLookMode)
		{
		}

		int IList<T>.IndexOf(T item)
		{
			return this.innerList.IndexOf(item);
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new InvalidOperationException("ThingOwner doesn't allow inserting individual elements at any position.");
		}

		void ICollection<T>.Add(T item)
		{
			this.TryAdd(item, true);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			this.innerList.CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.Contains(T item)
		{
			return this.innerList.Contains(item);
		}

		bool ICollection<T>.Remove(T item)
		{
			return this.Remove(item);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.innerList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.innerList.GetEnumerator();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look<T>(ref this.innerList, true, "innerList", this.contentsLookMode, new object[0]);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.innerList.RemoveAll((T x) => x == null);
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				for (int i = 0; i < this.innerList.Count; i++)
				{
					if (this.innerList[i] != null)
					{
						this.innerList[i].holdingOwner = this;
					}
				}
			}
		}

		public List<T>.Enumerator GetEnumerator()
		{
			return this.innerList.GetEnumerator();
		}

		public override bool CanAcceptAnyOf(Thing item, bool canMergeWithExistingStacks = true)
		{
			return item is T && base.CanAcceptAnyOf(item, canMergeWithExistingStacks);
		}

		public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
		{
			if (item == null)
			{
				Log.Warning("Tried to add null item to ThingOwner.");
				return 0;
			}
			if (this.Contains(item))
			{
				Log.Warning("Tried to add " + item + " to ThingOwner but this item is already here.");
				return 0;
			}
			if (count <= 0)
			{
				return 0;
			}
			if (!this.CanAcceptAnyOf(item, canMergeWithExistingStacks))
			{
				return 0;
			}
			int stackCount = item.stackCount;
			int num = Mathf.Min(stackCount, count);
			Thing thing = item.SplitOff(num);
			if (this.Contains(thing))
			{
				this.Remove(thing);
			}
			if (this.TryAdd((T)((object)thing), canMergeWithExistingStacks))
			{
				return num;
			}
			if (thing != item)
			{
				int result = stackCount - item.stackCount - thing.stackCount;
				item.TryAbsorbStack(thing, false);
				return result;
			}
			this.TryAdd(thing, false);
			return stackCount - item.stackCount;
		}

		public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
		{
			if (item == null)
			{
				Log.Warning("Tried to add null item to ThingOwner.");
				return false;
			}
			T t = item as T;
			if (t == null)
			{
				return false;
			}
			if (this.Contains(item))
			{
				Log.Warning("Tried to add " + item.ToStringSafe<Thing>() + " to ThingOwner but this item is already here.");
				return false;
			}
			if (item.holdingOwner != null)
			{
				Log.Warning(string.Concat(new string[]
				{
					"Tried to add ",
					item.ToStringSafe<Thing>(),
					" to ThingOwner but this thing is already in another container. owner=",
					this.owner.ToStringSafe<IThingHolder>(),
					", other container owner=",
					item.holdingOwner.Owner.ToStringSafe<IThingHolder>()
				}));
				return false;
			}
			if (!this.CanAcceptAnyOf(item, canMergeWithExistingStacks))
			{
				return false;
			}
			if (canMergeWithExistingStacks)
			{
				for (int i = 0; i < this.innerList.Count; i++)
				{
					T t2 = this.innerList[i];
					if (t2.CanStackWith(item))
					{
						int num = Mathf.Min(item.stackCount, t2.def.stackLimit - t2.stackCount);
						if (num > 0)
						{
							Thing other = item.SplitOff(num);
							int stackCount = t2.stackCount;
							t2.TryAbsorbStack(other, true);
							if (t2.stackCount > stackCount)
							{
								base.NotifyAddedAndMergedWith(t2, t2.stackCount - stackCount);
							}
							if (item.Destroyed || item.stackCount == 0)
							{
								return true;
							}
						}
					}
				}
			}
			if (this.Count >= this.maxStacks)
			{
				return false;
			}
			item.holdingOwner = this;
			this.innerList.Add(t);
			base.NotifyAdded(t);
			return true;
		}

		public new void TryAddRange(IEnumerable<T> things, bool canMergeWithExistingStacks = true)
		{
			if (things == this)
			{
				return;
			}
			ThingOwner thingOwner = things as ThingOwner;
			if (thingOwner != null)
			{
				thingOwner.TryTransferAllToContainer(this, canMergeWithExistingStacks);
			}
			else
			{
				IList<T> list = things as IList<T>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						this.TryAdd(list[i], canMergeWithExistingStacks);
					}
				}
				else
				{
					foreach (T current in things)
					{
						this.TryAdd(current, canMergeWithExistingStacks);
					}
				}
			}
		}

		public override int IndexOf(Thing item)
		{
			T t = item as T;
			if (t == null)
			{
				return -1;
			}
			return this.innerList.IndexOf(t);
		}

		public override bool Remove(Thing item)
		{
			if (!this.Contains(item))
			{
				return false;
			}
			if (item.holdingOwner == this)
			{
				item.holdingOwner = null;
			}
			int index = this.innerList.LastIndexOf((T)((object)item));
			this.innerList.RemoveAt(index);
			base.NotifyRemoved(item);
			return true;
		}

		public new int RemoveAll(Predicate<T> predicate)
		{
			int num = 0;
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				if (predicate(this.innerList[i]))
				{
					this.Remove(this.innerList[i]);
					num++;
				}
			}
			return num;
		}

		protected override Thing GetAt(int index)
		{
			return this.innerList[index];
		}

		public bool TryTransferToContainer(Thing item, ThingOwner otherContainer, int stackCount, out T resultingTransferredItem, bool canMergeWithExistingStacks = true)
		{
			Thing thing;
			bool result = base.TryTransferToContainer(item, otherContainer, stackCount, out thing, canMergeWithExistingStacks);
			resultingTransferredItem = (T)((object)thing);
			return result;
		}

		public new T Take(Thing thing, int count)
		{
			return (T)((object)base.Take(thing, count));
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count, out T resultingThing, Action<T, int> placedAction = null)
		{
			Action<Thing, int> placedAction2 = null;
			if (placedAction != null)
			{
				placedAction2 = delegate(Thing t, int c)
				{
					placedAction((T)((object)t), c);
				};
			}
			Thing thing2;
			bool result = base.TryDrop(thing, dropLoc, map, mode, count, out thing2, placedAction2);
			resultingThing = (T)((object)thing2);
			return result;
		}

		public bool TryDrop(Thing thing, ThingPlaceMode mode, out T lastResultingThing, Action<T, int> placedAction = null)
		{
			Action<Thing, int> placedAction2 = null;
			if (placedAction != null)
			{
				placedAction2 = delegate(Thing t, int c)
				{
					placedAction((T)((object)t), c);
				};
			}
			Thing thing2;
			bool result = base.TryDrop(thing, mode, out thing2, placedAction2);
			lastResultingThing = (T)((object)thing2);
			return result;
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, out T lastResultingThing, Action<T, int> placedAction = null)
		{
			Action<Thing, int> placedAction2 = null;
			if (placedAction != null)
			{
				placedAction2 = delegate(Thing t, int c)
				{
					placedAction((T)((object)t), c);
				};
			}
			Thing thing2;
			bool result = base.TryDrop(thing, dropLoc, map, mode, out thing2, placedAction2);
			lastResultingThing = (T)((object)thing2);
			return result;
		}
	}
	public abstract class ThingOwner : IEnumerable<Thing>, IEnumerable, ICollection<Thing>, IList<Thing>, IExposable
	{
		protected IThingHolder owner;

		protected int maxStacks = 999999;

		protected LookMode contentsLookMode = LookMode.Deep;

		Thing IList<Thing>.this[int index]
		{
			get
			{
				return this.GetAt(index);
			}
			set
			{
				throw new InvalidOperationException("ThingOwner doesn't allow setting individual elements.");
			}
		}

		bool ICollection<Thing>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public IThingHolder Owner
		{
			get
			{
				return this.owner;
			}
		}

		public abstract int Count
		{
			get;
		}

		public Thing this[int index]
		{
			get
			{
				return this.GetAt(index);
			}
		}

		public bool Any
		{
			get
			{
				return this.Count > 0;
			}
		}

		public int TotalStackCount
		{
			get
			{
				int num = 0;
				int count = this.Count;
				for (int i = 0; i < count; i++)
				{
					num += this.GetAt(i).stackCount;
				}
				return num;
			}
		}

		public string ContentsString
		{
			get
			{
				int count = this.Count;
				if (count == 0)
				{
					return "NothingLower".Translate();
				}
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < count; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(this.GetAt(i).Label);
				}
				return stringBuilder.ToString();
			}
		}

		public ThingOwner()
		{
		}

		public ThingOwner(IThingHolder owner)
		{
			this.owner = owner;
		}

		public ThingOwner(IThingHolder owner, bool oneStackOnly, LookMode contentsLookMode = LookMode.Deep) : this(owner)
		{
			this.maxStacks = ((!oneStackOnly) ? 999999 : 1);
			this.contentsLookMode = contentsLookMode;
		}

		void IList<Thing>.Insert(int index, Thing item)
		{
			throw new InvalidOperationException("ThingOwner doesn't allow inserting individual elements at any position.");
		}

		void ICollection<Thing>.Add(Thing item)
		{
			this.TryAdd(item, true);
		}

		void ICollection<Thing>.CopyTo(Thing[] array, int arrayIndex)
		{
			for (int i = 0; i < this.Count; i++)
			{
				array[i + arrayIndex] = this.GetAt(i);
			}
		}

		[DebuggerHidden]
		IEnumerator<Thing> IEnumerable<Thing>.GetEnumerator()
		{
			for (int i = 0; i < this.Count; i++)
			{
				yield return this.GetAt(i);
			}
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < this.Count; i++)
			{
				yield return this.GetAt(i);
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.maxStacks, "maxStacks", 999999, false);
			Scribe_Values.Look<LookMode>(ref this.contentsLookMode, "contentsLookMode", LookMode.Deep, false);
		}

		public void ThingOwnerTick(bool removeIfDestroyed = true)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				Thing at = this.GetAt(i);
				if (at.def.tickerType == TickerType.Normal)
				{
					at.Tick();
					if (at.Destroyed && removeIfDestroyed)
					{
						this.Remove(at);
					}
				}
			}
		}

		public void ThingOwnerTickRare(bool removeIfDestroyed = true)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				Thing at = this.GetAt(i);
				if (at.def.tickerType == TickerType.Rare)
				{
					at.TickRare();
					if (at.Destroyed && removeIfDestroyed)
					{
						this.Remove(at);
					}
				}
			}
		}

		public void ThingOwnerTickLong(bool removeIfDestroyed = true)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				Thing at = this.GetAt(i);
				if (at.def.tickerType == TickerType.Long)
				{
					at.TickRare();
					if (at.Destroyed && removeIfDestroyed)
					{
						this.Remove(at);
					}
				}
			}
		}

		public void Clear()
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				this.Remove(this.GetAt(i));
			}
		}

		public void ClearAndDestroyContents(DestroyMode mode = DestroyMode.Vanish)
		{
			while (this.Any)
			{
				for (int i = this.Count - 1; i >= 0; i--)
				{
					Thing at = this.GetAt(i);
					at.Destroy(mode);
					this.Remove(at);
				}
			}
		}

		public void ClearAndDestroyContentsOrPassToWorld(DestroyMode mode = DestroyMode.Vanish)
		{
			while (this.Any)
			{
				for (int i = this.Count - 1; i >= 0; i--)
				{
					Thing at = this.GetAt(i);
					at.DestroyOrPassToWorld(mode);
					this.Remove(at);
				}
			}
		}

		public virtual bool CanAcceptAnyOf(Thing item, bool canMergeWithExistingStacks = true)
		{
			if (item == null || item.stackCount <= 0)
			{
				return false;
			}
			int count = this.Count;
			if (count >= this.maxStacks)
			{
				if (canMergeWithExistingStacks)
				{
					for (int i = 0; i < count; i++)
					{
						Thing at = this.GetAt(i);
						if (at.stackCount < at.def.stackLimit && at.CanStackWith(item))
						{
							return true;
						}
					}
				}
				return false;
			}
			return true;
		}

		public abstract int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true);

		public abstract bool TryAdd(Thing item, bool canMergeWithExistingStacks = true);

		public abstract int IndexOf(Thing item);

		public abstract bool Remove(Thing item);

		protected abstract Thing GetAt(int index);

		public bool Contains(Thing item)
		{
			return item != null && item.holdingOwner == this;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= this.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			this.Remove(this.GetAt(index));
		}

		public void TryAddRange(IEnumerable<Thing> things, bool canMergeWithExistingStacks = true)
		{
			if (things == this)
			{
				return;
			}
			ThingOwner thingOwner = things as ThingOwner;
			if (thingOwner != null)
			{
				thingOwner.TryTransferAllToContainer(this, canMergeWithExistingStacks);
			}
			else
			{
				IList<Thing> list = things as IList<Thing>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						this.TryAdd(list[i], canMergeWithExistingStacks);
					}
				}
				else
				{
					foreach (Thing current in things)
					{
						this.TryAdd(current, canMergeWithExistingStacks);
					}
				}
			}
		}

		public int RemoveAll(Predicate<Thing> predicate)
		{
			int num = 0;
			for (int i = this.Count - 1; i >= 0; i--)
			{
				if (predicate(this.GetAt(i)))
				{
					this.Remove(this.GetAt(i));
					num++;
				}
			}
			return num;
		}

		public bool TryTransferToContainer(Thing item, ThingOwner otherContainer, bool canMergeWithExistingStacks = true)
		{
			return this.TryTransferToContainer(item, otherContainer, item.stackCount, canMergeWithExistingStacks);
		}

		public bool TryTransferToContainer(Thing item, ThingOwner otherContainer, int count, bool canMergeWithExistingStacks = true)
		{
			Thing thing;
			return this.TryTransferToContainer(item, otherContainer, count, out thing, canMergeWithExistingStacks);
		}

		public bool TryTransferToContainer(Thing item, ThingOwner otherContainer, int count, out Thing resultingTransferredItem, bool canMergeWithExistingStacks = true)
		{
			if (!this.Contains(item))
			{
				Log.Error("Can't transfer item " + item + " because it's not here.");
				resultingTransferredItem = null;
				return false;
			}
			if (otherContainer == this)
			{
				resultingTransferredItem = item;
				return true;
			}
			if (!otherContainer.CanAcceptAnyOf(item, canMergeWithExistingStacks))
			{
				resultingTransferredItem = null;
				return false;
			}
			if (count <= 0)
			{
				resultingTransferredItem = null;
				return false;
			}
			Thing thing = item.SplitOff(count);
			if (this.Contains(thing))
			{
				this.Remove(thing);
			}
			bool flag = otherContainer.TryAdd(thing, canMergeWithExistingStacks);
			if (flag)
			{
				resultingTransferredItem = thing;
			}
			else
			{
				if (!otherContainer.Contains(thing) && thing.stackCount > 0)
				{
					if (item != thing)
					{
						item.TryAbsorbStack(thing, false);
					}
					else
					{
						this.TryAdd(thing, false);
					}
				}
				resultingTransferredItem = null;
			}
			return flag;
		}

		public void TryTransferAllToContainer(ThingOwner other, bool canMergeWithExistingStacks = true)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				this.TryTransferToContainer(this.GetAt(i), other, canMergeWithExistingStacks);
			}
		}

		public Thing Take(Thing thing, int count)
		{
			if (!this.Contains(thing))
			{
				Log.Error("Tried to take " + thing.ToStringSafe<Thing>() + " but it's not here.");
				return null;
			}
			if (count > thing.stackCount)
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to get ",
					count,
					" of ",
					thing.ToStringSafe<Thing>(),
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
			thing2.holdingOwner = null;
			return thing2;
		}

		public void Take(Thing thing)
		{
			this.Take(thing, thing.stackCount);
		}

		public bool TryDrop(Thing thing, ThingPlaceMode mode, int count, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
		{
			Map rootMap = ThingOwnerUtility.GetRootMap(this.owner);
			IntVec3 rootPosition = ThingOwnerUtility.GetRootPosition(this.owner);
			if (rootMap == null || !rootPosition.IsValid)
			{
				Log.Error("Cannot drop " + thing + " without a dropLoc and with an owner whose map is null.");
				lastResultingThing = null;
				return false;
			}
			return this.TryDrop(thing, rootPosition, rootMap, mode, count, out lastResultingThing, placedAction);
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (!this.Contains(thing))
			{
				Log.Error("Tried to drop " + thing.ToStringSafe<Thing>() + " but it's not here.");
				resultingThing = null;
				return false;
			}
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
				thing.TryAbsorbStack(thing2, false);
				return false;
			}
		}

		public bool TryDrop(Thing thing, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
		{
			Map rootMap = ThingOwnerUtility.GetRootMap(this.owner);
			IntVec3 rootPosition = ThingOwnerUtility.GetRootPosition(this.owner);
			if (rootMap == null || !rootPosition.IsValid)
			{
				Log.Error("Cannot drop " + thing + " without a dropLoc and with an owner whose map is null.");
				lastResultingThing = null;
				return false;
			}
			return this.TryDrop(thing, rootPosition, rootMap, mode, out lastResultingThing, placedAction);
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
		{
			if (!this.Contains(thing))
			{
				Log.Error(this.owner.ToStringSafe<IThingHolder>() + " container tried to drop  " + thing.ToStringSafe<Thing>() + " which it didn't contain.");
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
			for (int i = this.Count - 1; i >= 0; i--)
			{
				Thing thing;
				if (!this.TryDrop(this.GetAt(i), dropLoc, map, mode, out thing, null))
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
			int count = this.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.GetAt(i).def == def)
				{
					num += this.GetAt(i).stackCount;
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
			int count = this.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.GetAt(i).def == def)
				{
					num += this.GetAt(i).stackCount;
				}
			}
			return num;
		}

		public void Notify_ContainedItemDestroyed(Thing t)
		{
			if (ThingOwnerUtility.ShouldAutoRemoveDestroyedThings(this.owner))
			{
				this.Remove(t);
			}
		}

		protected void NotifyAdded(Thing item)
		{
			if (ThingOwnerUtility.ShouldAutoExtinguishInnerThings(this.owner) && item.HasAttachment(ThingDefOf.Fire))
			{
				item.GetAttachment(ThingDefOf.Fire).Destroy(DestroyMode.Vanish);
			}
			if (ThingOwnerUtility.ShouldRemoveDesignationsOnAddedThings(this.owner))
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					maps[i].designationManager.RemoveAllDesignationsOn(item, false);
				}
			}
			CompTransporter compTransporter = this.owner as CompTransporter;
			if (compTransporter != null)
			{
				compTransporter.Notify_ThingAdded(item);
			}
			Caravan caravan = this.owner as Caravan;
			if (caravan != null)
			{
				caravan.Notify_PawnAdded((Pawn)item);
			}
			Pawn_ApparelTracker pawn_ApparelTracker = this.owner as Pawn_ApparelTracker;
			if (pawn_ApparelTracker != null)
			{
				pawn_ApparelTracker.Notify_ApparelAdded((Apparel)item);
			}
			Pawn_EquipmentTracker pawn_EquipmentTracker = this.owner as Pawn_EquipmentTracker;
			if (pawn_EquipmentTracker != null)
			{
				pawn_EquipmentTracker.Notify_EquipmentAdded((ThingWithComps)item);
			}
			this.NotifyColonistBarIfColonistCorpse(item);
		}

		protected void NotifyAddedAndMergedWith(Thing item, int mergedCount)
		{
			CompTransporter compTransporter = this.owner as CompTransporter;
			if (compTransporter != null)
			{
				compTransporter.Notify_ThingAddedAndMergedWith(item, mergedCount);
			}
		}

		protected void NotifyRemoved(Thing item)
		{
			Pawn_InventoryTracker pawn_InventoryTracker = this.owner as Pawn_InventoryTracker;
			if (pawn_InventoryTracker != null)
			{
				pawn_InventoryTracker.Notify_ItemRemoved(item);
			}
			Pawn_ApparelTracker pawn_ApparelTracker = this.owner as Pawn_ApparelTracker;
			if (pawn_ApparelTracker != null)
			{
				pawn_ApparelTracker.Notify_ApparelRemoved((Apparel)item);
			}
			Pawn_EquipmentTracker pawn_EquipmentTracker = this.owner as Pawn_EquipmentTracker;
			if (pawn_EquipmentTracker != null)
			{
				pawn_EquipmentTracker.Notify_EquipmentRemoved((ThingWithComps)item);
			}
			Caravan caravan = this.owner as Caravan;
			if (caravan != null)
			{
				caravan.Notify_PawnRemoved((Pawn)item);
			}
			this.NotifyColonistBarIfColonistCorpse(item);
		}

		private void NotifyColonistBarIfColonistCorpse(Thing thing)
		{
			Corpse corpse = thing as Corpse;
			if (corpse != null && !corpse.Bugged && corpse.InnerPawn.Faction != null && corpse.InnerPawn.Faction.IsPlayer && Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}
	}
}
