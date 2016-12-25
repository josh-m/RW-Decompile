using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public struct ThingAmount
	{
		public Thing thing;

		public int count;

		public ThingAmount(Thing thing, int count)
		{
			this.thing = thing;
			this.count = count;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.count,
				"x ",
				(this.thing == null) ? "null" : this.thing.ThingID,
				")"
			});
		}

		public static void AddToList(List<ThingAmount> list, Thing thing, int countToAdd)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].thing == thing)
				{
					list[i] = new ThingAmount(list[i].thing, list[i].count + countToAdd);
					return;
				}
			}
			list.Add(new ThingAmount(thing, countToAdd));
		}
	}
}
