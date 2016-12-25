using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemAvailability
	{
		private Map map;

		private Dictionary<int, bool> cachedResults = new Dictionary<int, bool>();

		public ItemAvailability(Map map)
		{
			this.map = map;
		}

		public void Tick()
		{
			this.cachedResults.Clear();
		}

		public bool ThingsAvailableAnywhere(ThingCountClass need, Pawn pawn)
		{
			int key = Gen.HashCombine<Faction>(need.GetHashCode(), pawn.Faction);
			bool flag;
			if (!this.cachedResults.TryGetValue(key, out flag))
			{
				List<Thing> list = this.map.listerThings.ThingsOfDef(need.thingDef);
				int num = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (!list[i].IsForbidden(pawn))
					{
						num += list[i].stackCount;
						if (num >= need.count)
						{
							break;
						}
					}
				}
				flag = (num >= need.count);
				this.cachedResults.Add(key, flag);
			}
			return flag;
		}
	}
}
