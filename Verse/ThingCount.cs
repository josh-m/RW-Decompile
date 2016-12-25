using System;

namespace Verse
{
	public struct ThingCount
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
	}
}
