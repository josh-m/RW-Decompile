using RimWorld.Planet;
using System;
using UnityEngine;

namespace Verse
{
	public struct LocalTargetInfo : IEquatable<LocalTargetInfo>
	{
		private Thing thingInt;

		private IntVec3 cellInt;

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

		public static LocalTargetInfo Invalid
		{
			get
			{
				return new LocalTargetInfo(IntVec3.Invalid);
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

		public LocalTargetInfo(Thing thing)
		{
			this.thingInt = thing;
			this.cellInt = IntVec3.Invalid;
		}

		public LocalTargetInfo(IntVec3 cell)
		{
			this.thingInt = null;
			this.cellInt = cell;
		}

		public TargetInfo ToTargetInfo(Map map)
		{
			if (!this.IsValid)
			{
				return TargetInfo.Invalid;
			}
			if (this.Thing != null)
			{
				return new TargetInfo(this.Thing);
			}
			return new TargetInfo(this.Cell, map, false);
		}

		public GlobalTargetInfo ToGlobalTargetInfo(Map map)
		{
			if (!this.IsValid)
			{
				return GlobalTargetInfo.Invalid;
			}
			if (this.Thing != null)
			{
				return new GlobalTargetInfo(this.Thing);
			}
			return new GlobalTargetInfo(this.Cell, map, false);
		}

		public override bool Equals(object obj)
		{
			return obj is LocalTargetInfo && this.Equals((LocalTargetInfo)obj);
		}

		public bool Equals(LocalTargetInfo other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			if (this.thingInt != null)
			{
				return this.thingInt.GetHashCode();
			}
			return this.cellInt.GetHashCode();
		}

		public override string ToString()
		{
			if (this.Thing != null)
			{
				return this.Thing.GetUniqueLoadID();
			}
			if (this.Cell.IsValid)
			{
				return this.Cell.ToString();
			}
			return "null";
		}

		public static implicit operator LocalTargetInfo(Thing t)
		{
			return new LocalTargetInfo(t);
		}

		public static implicit operator LocalTargetInfo(IntVec3 c)
		{
			return new LocalTargetInfo(c);
		}

		public static explicit operator IntVec3(LocalTargetInfo targ)
		{
			if (targ.thingInt != null)
			{
				Log.ErrorOnce("Casted LocalTargetInfo to IntVec3 but it had Thing " + targ.thingInt, 6324165);
			}
			return targ.Cell;
		}

		public static explicit operator Thing(LocalTargetInfo targ)
		{
			if (targ.cellInt.IsValid)
			{
				Log.ErrorOnce("Casted LocalTargetInfo to Thing but it had cell " + targ.cellInt, 631672);
			}
			return targ.thingInt;
		}

		public static bool operator ==(LocalTargetInfo a, LocalTargetInfo b)
		{
			if (a.Thing != null || b.Thing != null)
			{
				return a.Thing == b.Thing;
			}
			return (!a.cellInt.IsValid && !b.cellInt.IsValid) || a.cellInt == b.cellInt;
		}

		public static bool operator !=(LocalTargetInfo a, LocalTargetInfo b)
		{
			return !(a == b);
		}
	}
}
