using System;
using Verse;

namespace RimWorld
{
	public class Building_Vent : Building_TempControl
	{
		public override void TickRare()
		{
			GenTemperature.EqualizeTemperaturesThroughBuilding(this, 14f, true);
		}
	}
}
