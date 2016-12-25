using System;

namespace Verse
{
	public sealed class ThingStackPart : IExposable
	{
		public Thing thing;

		private int countInt;

		public int Count
		{
			get
			{
				return this.countInt;
			}
			set
			{
				if (value <= 0)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Tried to set ThingStackPart stack count to ",
						value,
						". thing=",
						this.thing
					}));
					return;
				}
				if (value > this.thing.stackCount)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Tried to set ThingStackPart stack count to ",
						value,
						", but thing's stack count is only ",
						this.thing.stackCount,
						". thing=",
						this.thing
					}));
					return;
				}
				this.countInt = value;
			}
		}

		public ThingStackPart()
		{
		}

		public ThingStackPart(Thing thing, int count)
		{
			this.thing = thing;
			this.Count = count;
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Thing>(ref this.thing, "thing", false);
			Scribe_Values.LookValue<int>(ref this.countInt, "count", 1, false);
		}

		public Thing Split()
		{
			if (this.Count <= 0)
			{
				return null;
			}
			Thing result = this.thing.SplitOff(this.Count);
			this.countInt = 0;
			return result;
		}
	}
}
