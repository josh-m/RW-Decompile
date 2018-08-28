using System;

namespace Verse
{
	public struct ThingCount : IEquatable<ThingCount>, IExposable
	{
		private Thing thing;

		private int count;

		public Thing Thing
		{
			get
			{
				return this.thing;
			}
		}

		public int Count
		{
			get
			{
				return this.count;
			}
		}

		public ThingCount(Thing thing, int count)
		{
			if (count < 0)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ThingCount stack count to ",
					count,
					". thing=",
					thing
				}), false);
				count = 0;
			}
			if (count > thing.stackCount)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ThingCount stack count to ",
					count,
					", but thing's stack count is only ",
					thing.stackCount,
					". thing=",
					thing
				}), false);
				count = thing.stackCount;
			}
			this.thing = thing;
			this.count = count;
		}

		public void ExposeData()
		{
			Scribe_References.Look<Thing>(ref this.thing, "thing", false);
			Scribe_Values.Look<int>(ref this.count, "count", 1, false);
		}

		public ThingCount WithCount(int newCount)
		{
			return new ThingCount(this.thing, newCount);
		}

		public override bool Equals(object obj)
		{
			return obj is ThingCount && this.Equals((ThingCount)obj);
		}

		public bool Equals(ThingCount other)
		{
			return this == other;
		}

		public static bool operator ==(ThingCount a, ThingCount b)
		{
			return a.thing == b.thing && a.count == b.count;
		}

		public static bool operator !=(ThingCount a, ThingCount b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine<Thing>(this.count, this.thing);
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.count,
				"x ",
				(this.thing == null) ? "null" : this.thing.LabelShort,
				")"
			});
		}

		public static implicit operator ThingCount(ThingCountClass t)
		{
			if (t == null)
			{
				return new ThingCount(null, 0);
			}
			return new ThingCount(t.thing, t.Count);
		}
	}
}
