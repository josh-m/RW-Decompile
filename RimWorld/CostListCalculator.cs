using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class CostListCalculator
	{
		private static Dictionary<int, List<ThingCountClass>> cachedCosts = new Dictionary<int, List<ThingCountClass>>(FastIntComparer.Instance);

		public static void Reset()
		{
			CostListCalculator.cachedCosts.Clear();
		}

		public static List<ThingCountClass> CostListAdjusted(this Thing thing)
		{
			return thing.def.CostListAdjusted(thing.Stuff, true);
		}

		public static List<ThingCountClass> CostListAdjusted(this BuildableDef entDef, ThingDef stuff, bool errorOnNullStuff = true)
		{
			int key = CostListCalculator.RequestHash(entDef, stuff);
			List<ThingCountClass> list;
			if (!CostListCalculator.cachedCosts.TryGetValue(key, out list))
			{
				list = new List<ThingCountClass>();
				int num = 0;
				if (entDef.MadeFromStuff)
				{
					if (errorOnNullStuff && stuff == null)
					{
						Log.Error("Cannot get AdjustedCostList for " + entDef + " with null Stuff.");
						return null;
					}
					if (stuff != null)
					{
						num = Mathf.RoundToInt((float)entDef.costStuffCount / stuff.VolumePerUnit);
						if (num < 1)
						{
							num = 1;
						}
					}
					else
					{
						num = entDef.costStuffCount;
					}
				}
				else if (stuff != null)
				{
					Log.Error(string.Concat(new object[]
					{
						"Got AdjustedCostList for ",
						entDef,
						" with stuff ",
						stuff,
						" but is not MadeFromStuff."
					}));
				}
				bool flag = false;
				if (entDef.costList != null)
				{
					for (int i = 0; i < entDef.costList.Count; i++)
					{
						ThingCountClass thingCountClass = entDef.costList[i];
						if (thingCountClass.thingDef == stuff)
						{
							list.Add(new ThingCountClass(thingCountClass.thingDef, thingCountClass.count + num));
							flag = true;
						}
						else
						{
							list.Add(thingCountClass);
						}
					}
				}
				if (!flag && num > 0)
				{
					list.Add(new ThingCountClass(stuff, num));
				}
				CostListCalculator.cachedCosts.Add(key, list);
			}
			return list;
		}

		private static int RequestHash(BuildableDef entDef, ThingDef stuff)
		{
			int num = (int)entDef.shortHash;
			if (stuff != null)
			{
				num += (int)stuff.shortHash << 16;
			}
			return num;
		}
	}
}
