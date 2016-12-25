using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Pawn_CarryTracker : IExposable, IThingContainerOwner
	{
		public Pawn pawn;

		public ThingContainer innerContainer;

		public Thing CarriedThing
		{
			get
			{
				if (this.innerContainer.Count == 0)
				{
					return null;
				}
				return this.innerContainer[0];
			}
		}

		public bool Full
		{
			get
			{
				return this.AvailableStackSpace(this.CarriedThing.def) <= 0;
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
			this.innerContainer = new ThingContainer(this, true, LookMode.Deep);
		}

		public void ExposeData()
		{
			Scribe_Deep.LookDeep<ThingContainer>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
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

		public int AvailableStackSpace(ThingDef td)
		{
			int num = this.MaxStackSpaceEver(td);
			if (this.CarriedThing != null)
			{
				num -= this.CarriedThing.stackCount;
			}
			return num;
		}

		public int MaxStackSpaceEver(ThingDef td)
		{
			float f = this.pawn.GetStatValue(StatDefOf.CarryingCapacity, true) / td.VolumePerUnit;
			int b = Mathf.RoundToInt(f);
			return Mathf.Min(td.stackLimit, b);
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
			if (this.innerContainer.TryAdd(item, true))
			{
				item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, this.pawn.Map, false));
				return true;
			}
			return false;
		}

		public int TryStartCarry(Thing item, int count)
		{
			if (this.pawn.Dead || this.pawn.Downed)
			{
				Log.Error("Dead/downed pawn " + this.pawn + " tried to start carry item.");
				return 0;
			}
			this.NotifyBuildingsItemLost(item);
			int num = this.innerContainer.TryAdd(item, count);
			if (num > 0)
			{
				item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, this.pawn.Map, false));
			}
			return num;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (this.innerContainer.TryDrop(this.CarriedThing, dropLoc, this.pawn.MapHeld, mode, out resultingThing, placedAction))
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
			if (this.innerContainer.TryDrop(this.CarriedThing, dropLoc, this.pawn.MapHeld, mode, count, out resultingThing, placedAction))
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
			this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
		}

		public void CarryHandsTick()
		{
			this.innerContainer.ThingContainerTick();
		}
	}
}
