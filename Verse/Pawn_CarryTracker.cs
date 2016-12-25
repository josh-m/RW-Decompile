using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Pawn_CarryTracker : IExposable, IThingContainerOwner
	{
		public Pawn pawn;

		public ThingContainer container;

		public Thing CarriedThing
		{
			get
			{
				if (this.container.Count == 0)
				{
					return null;
				}
				return this.container[0];
			}
		}

		public bool Full
		{
			get
			{
				return this.CarriedThing != null && this.AvailableStackSpace(this.CarriedThing.def) <= 0;
			}
		}

		public bool Spawned
		{
			get
			{
				return this.pawn.Spawned;
			}
		}

		public Pawn_CarryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			this.container = new ThingContainer(this, true);
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<ThingContainer>(ref this.container, "container", new object[]
			{
				this
			});
		}

		public ThingContainer GetContainer()
		{
			return this.container;
		}

		public IntVec3 GetPosition()
		{
			return this.pawn.Position;
		}

		public int AvailableStackSpace(ThingDef td)
		{
			int b = Mathf.RoundToInt(this.pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / td.VolumePerUnit);
			int num = Mathf.Min(td.stackLimit, b);
			if (this.CarriedThing != null)
			{
				num -= this.CarriedThing.stackCount;
			}
			return num;
		}

		public void NotifyBuildingsItemLost(Thing item)
		{
			if (item.StoringBuilding() != null)
			{
				Building_Storage building_Storage = item.StoringBuilding() as Building_Storage;
				if (building_Storage != null)
				{
					building_Storage.Notify_LostThing(item);
				}
			}
		}

		public bool TryStartCarry(Thing item)
		{
			if (this.pawn.Dead || this.pawn.Downed)
			{
				Log.Error("Dead/downed pawn " + this.pawn + " tried to start carry item.");
				return false;
			}
			this.NotifyBuildingsItemLost(item);
			if (this.container.TryAdd(item))
			{
				item.def.soundPickup.PlayOneShot(item.Position);
				return true;
			}
			return false;
		}

		public bool TryStartCarry(Thing item, int count)
		{
			if (this.pawn.Dead || this.pawn.Downed)
			{
				Log.Error("Dead/downed pawn " + this.pawn + " tried to start carry item.");
				return false;
			}
			this.NotifyBuildingsItemLost(item);
			if (this.container.TryAdd(item, count))
			{
				item.def.soundPickup.PlayOneShot(item.Position);
				return true;
			}
			return false;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (this.container.TryDrop(this.CarriedThing, dropLoc, mode, out resultingThing, placedAction))
			{
				if (this.pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					resultingThing.SetForbidden(true, false);
				}
				return true;
			}
			return false;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, int count, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (this.container.TryDrop(this.CarriedThing, dropLoc, mode, count, out resultingThing, placedAction))
			{
				if (this.pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					resultingThing.SetForbidden(true, false);
				}
				return true;
			}
			return false;
		}

		public void DestroyCarriedThing()
		{
			this.container.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public void CarryHandsTick()
		{
			this.container.ThingContainerTick();
		}
	}
}
