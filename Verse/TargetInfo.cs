using System;
using UnityEngine;

namespace Verse
{
	public struct TargetInfo : IEquatable<TargetInfo>
	{
		private Thing thingInt;

		private IntVec3 cellInt;

		private Map mapInt;

		public bool IsValid
		{
			get
			{
				return this.thingInt != null || this.cellInt.IsValid;
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

		public static TargetInfo Invalid
		{
			get
			{
				return new TargetInfo(IntVec3.Invalid, null, false);
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

		public Vector3 CenterVector3
		{
			get
			{
				if (this.thingInt != null)
				{
					return this.thingInt.DrawPos;
				}
				return this.cellInt.ToVector3Shifted();
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

		public TargetInfo(Thing thing)
		{
			this.thingInt = thing;
			this.cellInt = IntVec3.Invalid;
			this.mapInt = null;
		}

		public TargetInfo(IntVec3 cell, Map map, bool allowNullMap = false)
		{
			if (!allowNullMap && cell.IsValid && map == null)
			{
				Log.Warning("Constructed TargetInfo with cell=" + cell + " and a null map.");
			}
			this.thingInt = null;
			this.cellInt = cell;
			this.mapInt = map;
		}

		public override bool Equals(object obj)
		{
			return obj is TargetInfo && this.Equals((TargetInfo)obj);
		}

		public bool Equals(TargetInfo other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			if (this.thingInt != null)
			{
				return this.thingInt.GetHashCode();
			}
			return Gen.HashCombine<Map>(this.cellInt.GetHashCode(), this.mapInt);
		}

		public override string ToString()
		{
			if (this.Thing != null)
			{
				return this.Thing.GetUniqueLoadID();
			}
			if (this.Cell.IsValid)
			{
				return this.Cell.ToString() + ", " + ((this.mapInt == null) ? "null" : this.mapInt.GetUniqueLoadID());
			}
			return "null";
		}

		public static implicit operator TargetInfo(Thing t)
		{
			return new TargetInfo(t);
		}

		public static explicit operator LocalTargetInfo(TargetInfo t)
		{
			if (t.HasThing)
			{
				return new LocalTargetInfo(t.Thing);
			}
			return new LocalTargetInfo(t.Cell);
		}

		public static explicit operator IntVec3(TargetInfo targ)
		{
			if (targ.thingInt != null)
			{
				Log.ErrorOnce("Casted TargetInfo to IntVec3 but it had Thing " + targ.thingInt, 6324165);
			}
			return targ.Cell;
		}

		public static explicit operator Thing(TargetInfo targ)
		{
			if (targ.cellInt.IsValid)
			{
				Log.ErrorOnce("Casted TargetInfo to Thing but it had cell " + targ.cellInt, 631672);
			}
			return targ.thingInt;
		}

		public static bool operator ==(TargetInfo a, TargetInfo b)
		{
			if (a.Thing != null || b.Thing != null)
			{
				return a.Thing == b.Thing;
			}
			return (!a.cellInt.IsValid && !b.cellInt.IsValid) || (a.cellInt == b.cellInt && a.mapInt == b.mapInt);
		}

		public static bool operator !=(TargetInfo a, TargetInfo b)
		{
			return !(a == b);
		}
	}
}
