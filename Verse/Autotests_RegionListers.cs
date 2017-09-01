using System;
using System.Collections.Generic;

namespace Verse
{
	public static class Autotests_RegionListers
	{
		private static Dictionary<Region, List<Thing>> expectedListers = new Dictionary<Region, List<Thing>>();

		private static List<Region> tmpTouchableRegions = new List<Region>();

		public static void CheckBugs(Map map)
		{
			Autotests_RegionListers.CalculateExpectedListers(map);
			Autotests_RegionListers.CheckThingRegisteredTwice(map);
			Autotests_RegionListers.CheckThingNotRegisteredButShould();
			Autotests_RegionListers.CheckThingRegisteredButShouldnt(map);
		}

		private static void CheckThingRegisteredTwice(Map map)
		{
			foreach (KeyValuePair<Region, List<Thing>> current in Autotests_RegionListers.expectedListers)
			{
				Autotests_RegionListers.CheckDuplicates(current.Value, current.Key, true);
			}
			foreach (Region current2 in map.regionGrid.AllRegions)
			{
				Autotests_RegionListers.CheckDuplicates(current2.ListerThings.AllThings, current2, false);
			}
		}

		private static void CheckDuplicates(List<Thing> lister, Region region, bool expected)
		{
			for (int i = 1; i < lister.Count; i++)
			{
				for (int j = 0; j < i; j++)
				{
					if (lister[i] == lister[j])
					{
						if (expected)
						{
							Log.Error(string.Concat(new object[]
							{
								"Region error: thing ",
								lister[i],
								" is expected to be registered twice in ",
								region,
								"? This should never happen."
							}));
						}
						else
						{
							Log.Error(string.Concat(new object[]
							{
								"Region error: thing ",
								lister[i],
								" is registered twice in ",
								region
							}));
						}
					}
				}
			}
		}

		private static void CheckThingNotRegisteredButShould()
		{
			foreach (KeyValuePair<Region, List<Thing>> current in Autotests_RegionListers.expectedListers)
			{
				List<Thing> value = current.Value;
				List<Thing> allThings = current.Key.ListerThings.AllThings;
				for (int i = 0; i < value.Count; i++)
				{
					if (!allThings.Contains(value[i]))
					{
						Log.Error(string.Concat(new object[]
						{
							"Region error: thing ",
							value[i],
							" at ",
							value[i].Position,
							" should be registered in ",
							current.Key,
							" but it's not."
						}));
					}
				}
			}
		}

		private static void CheckThingRegisteredButShouldnt(Map map)
		{
			foreach (Region current in map.regionGrid.AllRegions)
			{
				List<Thing> list;
				if (!Autotests_RegionListers.expectedListers.TryGetValue(current, out list))
				{
					list = null;
				}
				List<Thing> allThings = current.ListerThings.AllThings;
				for (int i = 0; i < allThings.Count; i++)
				{
					if (list == null || !list.Contains(allThings[i]))
					{
						Log.Error(string.Concat(new object[]
						{
							"Region error: thing ",
							allThings[i],
							" at ",
							allThings[i].Position,
							" is registered in ",
							current,
							" but it shouldn't be."
						}));
					}
				}
			}
		}

		private static void CalculateExpectedListers(Map map)
		{
			Autotests_RegionListers.expectedListers.Clear();
			List<Thing> allThings = map.listerThings.AllThings;
			for (int i = 0; i < allThings.Count; i++)
			{
				Thing thing = allThings[i];
				if (ListerThings.EverListable(thing.def, ListerThingsUse.Region))
				{
					RegionListersUpdater.GetTouchableRegions(thing, map, Autotests_RegionListers.tmpTouchableRegions, false);
					for (int j = 0; j < Autotests_RegionListers.tmpTouchableRegions.Count; j++)
					{
						Region key = Autotests_RegionListers.tmpTouchableRegions[j];
						List<Thing> list;
						if (!Autotests_RegionListers.expectedListers.TryGetValue(key, out list))
						{
							list = new List<Thing>();
							Autotests_RegionListers.expectedListers.Add(key, list);
						}
						list.Add(allThings[i]);
					}
				}
			}
		}
	}
}
