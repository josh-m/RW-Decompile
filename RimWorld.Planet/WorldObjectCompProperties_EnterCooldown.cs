using System;

namespace RimWorld.Planet
{
	public class WorldObjectCompProperties_EnterCooldown : WorldObjectCompProperties
	{
		public bool autoStartOnMapRemoved = true;

		public float durationDays = 4f;

		public WorldObjectCompProperties_EnterCooldown()
		{
			this.compClass = typeof(EnterCooldownComp);
		}
	}
}
