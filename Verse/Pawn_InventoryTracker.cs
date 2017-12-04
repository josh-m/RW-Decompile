using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class Pawn_InventoryTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		public ThingOwner<Thing> innerContainer;

		private bool unloadEverything;

		private List<Thing> itemsNotForSale = new List<Thing>();

		private static List<ThingCount> tmpDrugsToKeep = new List<ThingCount>();

		private static List<Thing> tmpThingList = new List<Thing>();

		public bool UnloadEverything
		{
			get
			{
				return this.unloadEverything && this.HasAnyUnloadableThing;
			}
			set
			{
				if (value && this.HasAnyUnloadableThing)
				{
					this.unloadEverything = true;
				}
				else
				{
					this.unloadEverything = false;
				}
			}
		}

		private bool HasAnyUnloadableThing
		{
			get
			{
				return this.FirstUnloadableThing != default(ThingStackPart);
			}
		}

		public ThingStackPart FirstUnloadableThing
		{
			get
			{
				if (this.innerContainer.Count == 0)
				{
					return default(ThingStackPart);
				}
				if (this.pawn.drugs != null && this.pawn.drugs.CurrentPolicy != null)
				{
					DrugPolicy currentPolicy = this.pawn.drugs.CurrentPolicy;
					Pawn_InventoryTracker.tmpDrugsToKeep.Clear();
					for (int i = 0; i < currentPolicy.Count; i++)
					{
						if (currentPolicy[i].takeToInventory > 0)
						{
							Pawn_InventoryTracker.tmpDrugsToKeep.Add(new ThingCount(currentPolicy[i].drug, currentPolicy[i].takeToInventory));
						}
					}
					for (int j = 0; j < this.innerContainer.Count; j++)
					{
						if (!this.innerContainer[j].def.IsDrug)
						{
							return new ThingStackPart(this.innerContainer[j], this.innerContainer[j].stackCount);
						}
						int num = -1;
						for (int k = 0; k < Pawn_InventoryTracker.tmpDrugsToKeep.Count; k++)
						{
							if (this.innerContainer[j].def == Pawn_InventoryTracker.tmpDrugsToKeep[k].ThingDef)
							{
								num = k;
								break;
							}
						}
						if (num < 0)
						{
							return new ThingStackPart(this.innerContainer[j], this.innerContainer[j].stackCount);
						}
						if (this.innerContainer[j].stackCount > Pawn_InventoryTracker.tmpDrugsToKeep[num].Count)
						{
							return new ThingStackPart(this.innerContainer[j], this.innerContainer[j].stackCount - Pawn_InventoryTracker.tmpDrugsToKeep[num].Count);
						}
						Pawn_InventoryTracker.tmpDrugsToKeep[num] = new ThingCount(Pawn_InventoryTracker.tmpDrugsToKeep[num].ThingDef, Pawn_InventoryTracker.tmpDrugsToKeep[num].Count - this.innerContainer[j].stackCount);
					}
					return default(ThingStackPart);
				}
				return new ThingStackPart(this.innerContainer[0], this.innerContainer[0].stackCount);
			}
		}

		public IThingHolder ParentHolder
		{
			get
			{
				return this.pawn;
			}
		}

		public Pawn_InventoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<Thing>(ref this.itemsNotForSale, "itemsNotForSale", LookMode.Reference, new object[0]);
			Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Values.Look<bool>(ref this.unloadEverything, "unloadEverything", false, false);
		}

		public void InventoryTrackerTick()
		{
			this.innerContainer.ThingOwnerTick(true);
			if (this.unloadEverything && !this.HasAnyUnloadableThing)
			{
				this.unloadEverything = false;
			}
		}

		public void InventoryTrackerTickRare()
		{
			this.innerContainer.ThingOwnerTickRare(true);
		}

		public void DropAllNearPawn(IntVec3 pos, bool forbid = false, bool unforbid = false)
		{
			if (this.pawn.MapHeld == null)
			{
				Log.Error("Tried to drop all inventory near pawn but the pawn is unspawned. pawn=" + this.pawn);
				return;
			}
			Pawn_InventoryTracker.tmpThingList.Clear();
			Pawn_InventoryTracker.tmpThingList.AddRange(this.innerContainer);
			for (int i = 0; i < Pawn_InventoryTracker.tmpThingList.Count; i++)
			{
				Thing thing;
				this.innerContainer.TryDrop(Pawn_InventoryTracker.tmpThingList[i], pos, this.pawn.MapHeld, ThingPlaceMode.Near, out thing, delegate(Thing t, int unused)
				{
					if (forbid)
					{
						t.SetForbiddenIfOutsideHomeArea();
					}
					if (unforbid)
					{
						t.SetForbidden(false, false);
					}
					if (t.def.IsPleasureDrug)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.DrugBurning, OpportunityType.Important);
					}
				});
			}
		}

		public void DestroyAll(DestroyMode mode = DestroyMode.Vanish)
		{
			this.innerContainer.ClearAndDestroyContents(mode);
		}

		public bool Contains(Thing item)
		{
			return this.innerContainer.Contains(item);
		}

		public bool NotForSale(Thing item)
		{
			return this.itemsNotForSale.Contains(item);
		}

		public void TryAddItemNotForSale(Thing item)
		{
			if (this.innerContainer.TryAdd(item, false))
			{
				this.itemsNotForSale.Add(item);
			}
		}

		public void Notify_ItemRemoved(Thing item)
		{
			this.itemsNotForSale.Remove(item);
			if (this.unloadEverything && !this.HasAnyUnloadableThing)
			{
				this.unloadEverything = false;
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}
	}
}
