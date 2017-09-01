using System;

namespace Verse
{
	public struct ThingStackPart : IEquatable<ThingStackPart>
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

		public ThingStackPart(Thing thing, int count)
		{
			if (count < 0)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ThingStackPart stack count to ",
					count,
					". thing=",
					thing
				}));
				count = 0;
			}
			if (count > thing.stackCount)
			{
				Log.Warning(string.Concat(new object[]
				{
					"Tried to set ThingStackPart stack count to ",
					count,
					", but thing's stack count is only ",
					thing.stackCount,
					". thing=",
					thing
				}));
				count = thing.stackCount;
			}
			this.thing = thing;
			this.count = count;
		}

		public ThingStackPart WithCount(int newCount)
		{
			return new ThingStackPart(this.thing, newCount);
		}

		public override bool Equals(object obj)
		{
			return obj is ThingStackPart && this.Equals((ThingStackPart)obj);
		}

		public bool Equals(ThingStackPart other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine<Thing>(this.count, this.thing);
		}

		public static bool operator ==(ThingStackPart a, ThingStackPart b)
		{
			return a.thing == b.thing && a.count == b.count;
		}

		public static bool operator !=(ThingStackPart a, ThingStackPart b)
		{
			return !(a == b);
		}
	}
}
