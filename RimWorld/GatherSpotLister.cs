using System;
using System.Collections.Generic;

namespace RimWorld
{
	public class GatherSpotLister
	{
		public List<CompGatherSpot> activeSpots = new List<CompGatherSpot>();

		public void RegisterActivated(CompGatherSpot spot)
		{
			if (!this.activeSpots.Contains(spot))
			{
				this.activeSpots.Add(spot);
			}
		}

		public void RegisterDeactivated(CompGatherSpot spot)
		{
			if (this.activeSpots.Contains(spot))
			{
				this.activeSpots.Remove(spot);
			}
		}
	}
}
