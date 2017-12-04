using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class CostListCalculator
	{
		private struct CostListPair : IEquatable<CostListCalculator.CostListPair>
		{
			public BuildableDef buildable;

			public ThingDef stuff;

			public CostListPair(BuildableDef buildable, ThingDef stuff)
			{
				this.buildable = buildable;
				this.stuff = stuff;
			}

			public override int GetHashCode()
			{
				int seed = 0;
				seed = Gen.HashCombine<BuildableDef>(seed, this.buildable);
				return Gen.HashCombine<ThingDef>(seed, this.stuff);
			}

			public override bool Equals(object obj)
			{
				return obj is CostListCalculator.CostListPair && this.Equals((CostListCalculator.CostListPair)obj);
			}

			public bool Equals(CostListCalculator.CostListPair other)
			{
				return this == other;
			}

			public static bool operator ==(CostListCalculator.CostListPair lhs, CostListCalculator.CostListPair rhs)
			{
				return lhs.buildable == rhs.buildable && lhs.stuff == rhs.stuff;
			}

			public static bool operator !=(CostListCalculator.CostListPair lhs, CostListCalculator.CostListPair rhs)
			{
				return !(lhs == rhs);
			}
		}

		private class FastCostListPairComparer : IEqualityComparer<CostListCalculator.CostListPair>
		{
			public static readonly CostListCalculator.FastCostListPairComparer Instance = new CostListCalculator.FastCostListPairComparer();

			public bool Equals(CostListCalculator.CostListPair x, CostListCalculator.CostListPair y)
			{
				return x == y;
			}

			public int GetHashCode(CostListCalculator.CostListPair obj)
			{
				return obj.GetHashCode();
			}
		}

		private static Dictionary<CostListCalculator.CostListPair, List<ThingCountClass>> cachedCosts = new Dictionary<CostListCalculator.CostListPair, List<ThingCountClass>>(CostListCalculator.FastCostListPairComparer.Instance);

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
			CostListCalculator.CostListPair key = new CostListCalculator.CostListPair(entDef, stuff);
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
	}
}
