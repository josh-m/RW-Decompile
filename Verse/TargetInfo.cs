using System;
using UnityEngine;

namespace Verse
{
	public struct TargetInfo : IEquatable<TargetInfo>
	{
		private Thing thingInt;

		private IntVec3 cellInt;

		public Thing Thing
		{
			get
			{
				return this.thingInt;
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

		public IntVec3 Center
		{
			get
			{
				if (this.thingInt == null)
				{
					Log.ErrorOnce("Got Center of TargetInfo with null Thing " + this.ToString(), 8822291);
					return this.cellInt;
				}
				return this.thingInt.PositionHeld;
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

		public bool ThingDestroyed
		{
			get
			{
				return this.Thing != null && this.Thing.Destroyed;
			}
		}

		public bool HasThing
		{
			get
			{
				return this.Thing != null;
			}
		}

		public bool IsValid
		{
			get
			{
				return this.thingInt != null || this.cellInt.IsValid;
			}
		}

		public static TargetInfo NullThing
		{
			get
			{
				return new TargetInfo(null);
			}
		}

		public static TargetInfo Invalid
		{
			get
			{
				return new TargetInfo(IntVec3.Invalid);
			}
		}

		public TargetInfo(Thing thing)
		{
			this.thingInt = thing;
			this.cellInt = IntVec3.Invalid;
		}

		public TargetInfo(IntVec3 cell)
		{
			this.thingInt = null;
			this.cellInt = cell;
		}

		public override bool Equals(object obj)
		{
			return obj is TargetInfo && this.Equals((TargetInfo)obj);
		}

		public bool Equals(TargetInfo other)
		{
			return this.cellInt == other.cellInt && this.thingInt == other.thingInt;
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
			if (this.Cell != IntVec3.Invalid)
			{
				return this.Cell.ToString();
			}
			return "Invalid";
		}

		public static implicit operator TargetInfo(Thing t)
		{
			return new TargetInfo(t);
		}

		public static implicit operator TargetInfo(IntVec3 vec)
		{
			return new TargetInfo(vec);
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
			return a.cellInt == b.cellInt;
		}

		public static bool operator !=(TargetInfo a, TargetInfo b)
		{
			if (a.Thing != null || b.Thing != null)
			{
				return a.Thing != b.Thing;
			}
			return a.cellInt != b.cellInt;
		}
	}
}
