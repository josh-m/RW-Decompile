using System;

namespace Verse
{
	public sealed class ThingStackPartClass : IExposable
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
				if (value < 0)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Tried to set ThingStackPartClass stack count to ",
						value,
						". thing=",
						this.thing
					}));
					this.countInt = 0;
					return;
				}
				if (value > this.thing.stackCount)
				{
					Log.Warning(string.Concat(new object[]
					{
						"Tried to set ThingStackPartClass stack count to ",
						value,
						", but thing's stack count is only ",
						this.thing.stackCount,
						". thing=",
						this.thing
					}));
					this.countInt = this.thing.stackCount;
					return;
				}
				this.countInt = value;
			}
		}

		public ThingStackPartClass()
		{
		}

		public ThingStackPartClass(Thing thing, int count)
		{
			this.thing = thing;
			this.Count = count;
		}

		public void ExposeData()
		{
			Scribe_References.LookReference<Thing>(ref this.thing, "thing", false);
			Scribe_Values.LookValue<int>(ref this.countInt, "count", 1, false);
		}
	}
}
