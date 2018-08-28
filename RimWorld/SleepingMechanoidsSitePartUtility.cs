using RimWorld.Planet;
using System;

namespace RimWorld
{
	public static class SleepingMechanoidsSitePartUtility
	{
		public static int GetPawnGroupMakerSeed(SiteCoreOrPartParams parms)
		{
			return parms.randomValue;
		}
	}
}
