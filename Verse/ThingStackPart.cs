using System;

namespace Verse
{
	public struct ThingStackPart
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
	}
}
