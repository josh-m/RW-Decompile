using System;
using Verse;

namespace RimWorld
{
	public interface IPlantToGrowSettable
	{
		Map Map
		{
			get;
		}

		ThingDef GetPlantDefToGrow();

		void SetPlantDefToGrow(ThingDef plantDef);

		bool CanAcceptSowNow();
	}
}
