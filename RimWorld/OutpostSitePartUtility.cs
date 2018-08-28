using RimWorld.Planet;
using System;

namespace RimWorld
{
	public static class OutpostSitePartUtility
	{
		public static int GetPawnGroupMakerSeed(SiteCoreOrPartParams parms)
		{
			return parms.randomValue;
		}
	}
}
