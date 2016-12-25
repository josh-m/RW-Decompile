using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class Pawn_InventoryTracker : IExposable, IThingContainerOwner
	{
		public Pawn pawn;

		public ThingContainer innerContainer;

		private bool unloadEverything;

		private List<Thing> itemsNotForSale = new List<Thing>();

		private static List<Thing> tmpThingList = new List<Thing>();

		public bool Spawned
		{
			get
			{
				return this.pawn.Spawned;
			}
		}

		public bool UnloadEverything
		{
			get
			{
				return this.unloadEverything;
			}
			set
			{
				if (value && this.innerContainer.Count > 0)
				{
					this.unloadEverything = true;
				}
				else
				{
					this.unloadEverything = false;
				}
			}
		}

		public Pawn_InventoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.innerContainer = new ThingContainer(this, false, LookMode.Deep);
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<Thing>(ref this.itemsNotForSale, "itemsNotForSale", LookMode.Reference, new object[0]);
			Scribe_Deep.LookDeep<ThingContainer>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_Values.LookValue<bool>(ref this.unloadEverything, "unloadEverything", false, false);
		}

		public void InventoryTrackerTick()
		{
			this.innerContainer.ThingContainerTick();
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
		}

		public ThingContainer GetInnerContainer()
		{
			return this.innerContainer;
		}

		public IntVec3 GetPosition()
		{
			return this.pawn.PositionHeld;
		}

		public Map GetMap()
		{
			return this.pawn.MapHeld;
		}
	}
}
