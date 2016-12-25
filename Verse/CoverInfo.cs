using System;

namespace Verse
{
	public struct CoverInfo
	{
		private Thing thingInt;

		private float blockChanceInt;

		public Thing Thing
		{
			get
			{
				return this.thingInt;
			}
		}

		public float BlockChance
		{
			get
			{
				return this.blockChanceInt;
			}
		}

		public static CoverInfo Invalid
		{
			get
			{
				return new CoverInfo(null, -999f);
			}
		}

		public CoverInfo(Thing thing, float blockChance)
		{
			this.thingInt = thing;
			this.blockChanceInt = blockChance;
		}
	}
}
