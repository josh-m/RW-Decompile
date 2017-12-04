using System;
using Verse;

namespace RimWorld
{
	public class CompProperties_PsychicDrone : CompProperties
	{
		public int droneLevelIncreaseInterval = 150000;

		public CompProperties_PsychicDrone()
		{
			this.compClass = typeof(CompPsychicDrone);
		}
	}
}
