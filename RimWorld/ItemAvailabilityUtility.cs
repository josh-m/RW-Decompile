using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ItemAvailabilityUtility
	{
		private static Dictionary<int, bool> cachedResults = new Dictionary<int, bool>();

		public static void Tick()
		{
			ItemAvailabilityUtility.cachedResults.Clear();
		}

		public static bool ThingsAvailableAnywhere(ThingCount need, Pawn pawn)
		{
			int key = Gen.HashCombine<Faction>(need.GetHashCode(), pawn.Faction);
			bool flag;
			if (!ItemAvailabilityUtility.cachedResults.TryGetValue(key, out flag))
			{
				List<Thing> list = Find.Map.listerThings.ThingsOfDef(need.thingDef);
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
				ItemAvailabilityUtility.cachedResults.Add(key, flag);
			}
			return flag;
		}
	}
}
