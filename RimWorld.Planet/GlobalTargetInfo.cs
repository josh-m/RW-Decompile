using System;
using Verse;

namespace RimWorld.Planet
{
	public struct GlobalTargetInfo : IEquatable<GlobalTargetInfo>
	{
		public const char WorldObjectLoadIDMarker = '@';

		private Thing thingInt;

		private IntVec3 cellInt;

		private Map mapInt;

		private WorldObject worldObjectInt;

		private int tileInt;

		public bool IsValid
		{
			get
			{
				return this.thingInt != null || this.cellInt.IsValid || this.worldObjectInt != null || this.tileInt >= 0;
			}
		}

		public bool IsMapTarget
		{
			get
			{
				return this.HasThing || this.cellInt.IsValid;
			}
		}

		public bool IsWorldTarget
		{
			get
			{
				return this.HasWorldObject || this.tileInt >= 0;
			}
		}

		public bool HasThing
		{
			get
			{
				return this.Thing != null;
			}
		}

		public Thing Thing
		{
			get
			{
				return this.thingInt;
			}
		}

		public bool ThingDestroyed
		{
			get
			{
				return this.Thing != null && this.Thing.Destroyed;
			}
		}

		public bool HasWorldObject
		{
			get
			{
				return this.WorldObject != null;
			}
		}

		public WorldObject WorldObject
		{
			get
			{
				return this.worldObjectInt;
			}
		}

		public static GlobalTargetInfo Invalid
		{
			get
			{
				return new GlobalTargetInfo(IntVec3.Invalid, null, false);
			}
		}

		public IntVec3 Cell
		{
			get
			{
				if (this.thingInt != null)
				{
					return this.thingInt.PositionHeld;
				}
				return this.cellInt;
			}
		}

		public Map Map
		{
			get
			{
				if (this.thingInt != null)
				{
					return this.thingInt.MapHeld;
				}
				return this.mapInt;
			}
		}

		public int Tile
		{
			get
			{
				if (this.worldObjectInt != null)
				{
					return this.worldObjectInt.Tile;
				}
				if (this.tileInt >= 0)
				{
					return this.tileInt;
				}
				if (this.thingInt != null && this.thingInt.MapHeld != null)
				{
					return this.thingInt.MapHeld.Tile;
				}
				if (this.cellInt.IsValid && this.mapInt != null)
				{
					return this.mapInt.Tile;
				}
				return -1;
			}
		}

		public GlobalTargetInfo(Thing thing)
		{
			this.thingInt = thing;
			this.cellInt = IntVec3.Invalid;
			this.mapInt = null;
			this.worldObjectInt = null;
			this.tileInt = -1;
		}

		public GlobalTargetInfo(IntVec3 cell, Map map, bool allowNullMap = false)
		{
			if (!allowNullMap && cell.IsValid && map == null)
			{
				Log.Warning("Constructed GlobalTargetInfo with cell=" + cell + " and a null map.");
			}
			this.thingInt = null;
			this.cellInt = cell;
			this.mapInt = map;
			this.worldObjectInt = null;
			this.tileInt = -1;
		}

		public GlobalTargetInfo(WorldObject worldObject)
		{
			this.thingInt = null;
			this.cellInt = IntVec3.Invalid;
			this.mapInt = null;
			this.worldObjectInt = worldObject;
			this.tileInt = -1;
		}

		public GlobalTargetInfo(int tile)
		{
			this.thingInt = null;
			this.cellInt = IntVec3.Invalid;
			this.mapInt = null;
			this.worldObjectInt = null;
			this.tileInt = tile;
		}

		public override bool Equals(object obj)
		{
			return obj is GlobalTargetInfo && this.Equals((GlobalTargetInfo)obj);
		}

		public bool Equals(GlobalTargetInfo other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			if (this.thingInt != null)
			{
				return this.thingInt.GetHashCode();
			}
			if (this.cellInt.IsValid)
			{
				return Gen.HashCombine<Map>(this.cellInt.GetHashCode(), this.mapInt);
			}
			if (this.worldObjectInt != null)
			{
				return this.worldObjectInt.GetHashCode();
			}
			if (this.tileInt >= 0)
			{
				return this.tileInt;
			}
			return -1;
		}

		public override string ToString()
		{
			if (this.thingInt != null)
			{
				return this.thingInt.GetUniqueLoadID();
			}
			if (this.cellInt.IsValid)
			{
				return this.cellInt.ToString() + ", " + ((this.mapInt == null) ? "null" : this.mapInt.GetUniqueLoadID());
			}
			if (this.worldObjectInt != null)
			{
				return '@' + this.worldObjectInt.GetUniqueLoadID();
			}
			if (this.tileInt >= 0)
			{
				return this.tileInt.ToString();
			}
			return "null";
		}

		public static implicit operator GlobalTargetInfo(TargetInfo target)
		{
			if (target.HasThing)
			{
				return new GlobalTargetInfo(target.Thing);
			}
			return new GlobalTargetInfo(target.Cell, target.Map, false);
		}

		public static implicit operator GlobalTargetInfo(Thing t)
		{
			return new GlobalTargetInfo(t);
		}

		public static implicit operator GlobalTargetInfo(WorldObject o)
		{
			return new GlobalTargetInfo(o);
		}

		public static explicit operator LocalTargetInfo(GlobalTargetInfo targ)
		{
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to LocalTargetInfo but it had WorldObject " + targ.worldObjectInt, 134566);
				return LocalTargetInfo.Invalid;
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to LocalTargetInfo but it had tile " + targ.tileInt, 7833122);
				return LocalTargetInfo.Invalid;
			}
			if (!targ.IsValid)
			{
				return LocalTargetInfo.Invalid;
			}
			if (targ.thingInt != null)
			{
				return new LocalTargetInfo(targ.thingInt);
			}
			return new LocalTargetInfo(targ.cellInt);
		}

		public static explicit operator TargetInfo(GlobalTargetInfo targ)
		{
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to TargetInfo but it had WorldObject " + targ.worldObjectInt, 134566);
				return TargetInfo.Invalid;
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to TargetInfo but it had tile " + targ.tileInt, 7833122);
				return TargetInfo.Invalid;
			}
			if (!targ.IsValid)
			{
				return TargetInfo.Invalid;
			}
			if (targ.thingInt != null)
			{
				return new TargetInfo(targ.thingInt);
			}
			return new TargetInfo(targ.cellInt, targ.mapInt, false);
		}

		public static explicit operator IntVec3(GlobalTargetInfo targ)
		{
			if (targ.thingInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to IntVec3 but it had Thing " + targ.thingInt, 6324165);
			}
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to IntVec3 but it had WorldObject " + targ.worldObjectInt, 134566);
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to IntVec3 but it had tile " + targ.tileInt, 7833122);
			}
			return targ.Cell;
		}

		public static explicit operator Thing(GlobalTargetInfo targ)
		{
			if (targ.cellInt.IsValid)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to Thing but it had cell " + targ.cellInt, 631672);
			}
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to Thing but it had WorldObject " + targ.worldObjectInt, 134566);
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to Thing but it had tile " + targ.tileInt, 7833122);
			}
			return targ.thingInt;
		}

		public static explicit operator WorldObject(GlobalTargetInfo targ)
		{
			if (targ.thingInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to WorldObject but it had Thing " + targ.thingInt, 6324165);
			}
			if (targ.cellInt.IsValid)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to WorldObject but it had cell " + targ.cellInt, 631672);
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to WorldObject but it had tile " + targ.tileInt, 7833122);
			}
			return targ.worldObjectInt;
		}

		public static bool operator ==(GlobalTargetInfo a, GlobalTargetInfo b)
		{
			if (a.Thing != null || b.Thing != null)
			{
				return a.Thing == b.Thing;
			}
			if (a.cellInt.IsValid || b.cellInt.IsValid)
			{
				return a.cellInt == b.cellInt && a.mapInt == b.mapInt;
			}
			if (a.WorldObject != null || b.WorldObject != null)
			{
				return a.WorldObject == b.WorldObject;
			}
			return (a.tileInt < 0 && b.tileInt < 0) || a.tileInt == b.tileInt;
		}

		public static bool operator !=(GlobalTargetInfo a, GlobalTargetInfo b)
		{
			return !(a == b);
		}
	}
}
