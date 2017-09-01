using RimWorld.Planet;
using System;

namespace RimWorld
{
	public class WorldObjectCompProperties_Timeout : WorldObjectCompProperties
	{
		public WorldObjectCompProperties_Timeout()
		{
			this.compClass = typeof(TimeoutComp);
		}
	}
}
