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

		public ThingContainer(IThingContainerOwner owner, bool oneStackOnly) : this(owner)
		{
			this.maxStacks = ((!oneStackOnly) ? 99999 : 1);
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
			Scribe_Collections.LookList<Thing>(ref this.innerList, "innerList", LookMode.Deep, new object[0]);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < this.innerList.Count; i++)
				{
					this.innerList[i].holder = this;
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
			for (int i = 0; i < this.innerList.Count; i++)
			{
				if (this.innerList[i].holder == this)
				{
					this.innerList[i].holder = null;
				}
			}
			this.innerList.Clear();
		}

		public void ClearAndDestroyContents(DestroyMode mode = DestroyMode.Vanish)
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				this.innerList[i].Destroy(mode);
			}
			this.Clear();
		}

		public bool CanAcceptAnyOf(Thing item)
		{
			return this.innerList.Count < this.maxStacks;
		}

		public bool TryAdd(Thing item, int count)
		{
			int count2 = Mathf.Min(count, this.AvailableStackSpace);
			Thing item2 = item.SplitOff(count2);
			return this.TryAdd(item2);
		}

		public bool TryAdd(Thing item)
		{
			if (item.stackCount > this.AvailableStackSpace)
			{
				Log.Error(string.Concat(new object[]
				{
					"Add item with stackCount=",
					item.stackCount,
					" with only ",
					this.AvailableStackSpace,
					" in container. Splitting and adding..."
				}));
				return this.TryAdd(item, this.AvailableStackSpace);
			}
			SlotGroupUtility.Notify_TakingThing(item);
			if (item.def.stackLimit > 1)
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
			item.holder = this;
			this.innerList.Add(item);
			return true;
		}

		public bool Contains(Thing item)
		{
			return this.innerList.Contains(item);
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

		public void Remove(Thing item)
		{
			if (item.holder == this)
			{
				item.holder = null;
			}
			this.innerList.Remove(item);
		}

		public void RemoveAll(Predicate<Thing> match)
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				if (match(this.innerList[i]))
				{
					if (this.innerList[i].holder == this)
					{
						this.innerList[i].holder = null;
					}
					this.innerList.RemoveAt(i);
				}
			}
		}

		public void TransferToContainer(Thing item, ThingContainer otherContainer, int stackCount)
		{
			Thing thing = item.SplitOff(stackCount);
			if (this.Contains(thing))
			{
				this.Remove(thing);
			}
			otherContainer.TryAdd(thing);
			Find.DesignationManager.RemoveAllDesignationsOn(thing, false);
			Thing thing2 = otherContainer.owner as Thing;
			if (thing2 != null)
			{
				item.def.soundPickup.PlayOneShot(thing2.Position);
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
			thing2.holder = null;
			return thing2;
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, ThingPlaceMode mode, int count, out Thing resultingThing, Action<Thing, int> placedAction = null)
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
				if (GenDrop.TryDropSpawn(thing, dropLoc, mode, out resultingThing, placedAction))
				{
					this.Remove(thing);
					return true;
				}
				return false;
			}
			else
			{
				Thing thing2 = thing.SplitOff(count);
				if (GenDrop.TryDropSpawn(thing2, dropLoc, mode, out resultingThing, placedAction))
				{
					return true;
				}
				thing.stackCount += thing2.stackCount;
				return false;
			}
		}

		public bool TryDrop(Thing thing, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
		{
			Thing thing2 = this.owner as Thing;
			if (thing2 == null)
			{
				Log.Error(string.Concat(new object[]
				{
					"Cannot drop ",
					thing,
					" without a dropLoc and with non-Thing owner ",
					this.owner,
					". Attempting recovery..."
				}));
				Pawn_CarryTracker pawn_CarryTracker = this.owner as Pawn_CarryTracker;
				if (pawn_CarryTracker == null)
				{
					thing.Destroy(DestroyMode.Vanish);
					lastResultingThing = null;
					return false;
				}
				thing2 = pawn_CarryTracker.pawn;
			}
			return this.TryDrop(thing, thing2.Position, mode, out lastResultingThing, placedAction);
		}

		public bool TryDrop(Thing thing, IntVec3 dropLoc, ThingPlaceMode mode, out Thing lastResultingThing, Action<Thing, int> placedAction = null)
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
			if (GenDrop.TryDropSpawn(thing, dropLoc, mode, out lastResultingThing, placedAction))
			{
				this.Remove(thing);
				return true;
			}
			return false;
		}

		public bool TryDropAll(IntVec3 dropLoc, ThingPlaceMode mode)
		{
			for (int i = this.innerList.Count - 1; i >= 0; i--)
			{
				Thing thing;
				if (!this.TryDrop(this.innerList[i], dropLoc, mode, out thing, null))
				{
					return false;
				}
			}
			return true;
		}

		public bool Contains(ThingDef def)
		{
			return this.Contains(def, 1);
		}

		public bool Contains(ThingDef def, int minCount)
		{
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

		public int NumContained(ThingDef def)
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
			this.Remove(t);
		}
	}
}
