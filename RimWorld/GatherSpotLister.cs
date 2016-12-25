using System;
using System.Collections.Generic;

namespace RimWorld
{
	public static class GatherSpotLister
	{
		public static List<CompGatherSpot> activeSpots = new List<CompGatherSpot>();

		public static void Reinit()
		{
			GatherSpotLister.activeSpots.Clear();
		}

		public static void RegisterActivated(CompGatherSpot spot)
		{
			if (!GatherSpotLister.activeSpots.Contains(spot))
			{
				GatherSpotLister.activeSpots.Add(spot);
			}
		}

		public static void RegisterDeactivated(CompGatherSpot spot)
		{
			if (GatherSpotLister.activeSpots.Contains(spot))
			{
				GatherSpotLister.activeSpots.Remove(spot);
			}
		}
	}
}
