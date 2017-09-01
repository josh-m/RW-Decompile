using System;

namespace Verse
{
	public struct ThingCount : IEquatable<ThingCount>
	{
		private ThingDef thingDef;

		private int count;

		public ThingDef ThingDef
		{
			get
			{
				return this.thingDef;
			}
		}

		public int Count
		{
			get
			{
				return this.count;
			}
		}

		public ThingCount(ThingDef thingDef, int count)
		{
			if (count < 0)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ThingCount count to ",
					count,
					". thingDef=",
					thingDef
				}));
				count = 0;
			}
			this.thingDef = thingDef;
			this.count = count;
		}

		public ThingCount WithCount(int newCount)
		{
			return new ThingCount(this.thingDef, newCount);
		}

		public override bool Equals(object obj)
		{
			return obj is ThingCount && this.Equals((ThingCount)obj);
		}

		public bool Equals(ThingCount other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine<ThingDef>(this.count, this.thingDef);
		}

		public static bool operator ==(ThingCount a, ThingCount b)
		{
			return a.thingDef == b.thingDef && a.count == b.count;
		}

		public static bool operator !=(ThingCount a, ThingCount b)
		{
			return !(a == b);
		}
	}
}
