using System;
using Verse;

namespace RimWorld
{
	public interface IPlantToGrowSettable
	{
		ThingDef GetPlantDefToGrow();

		void SetPlantDefToGrow(ThingDef plantDef);

		bool CanAcceptSowNow();
	}
}
