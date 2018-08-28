using System;

namespace RimWorld.Planet
{
	public class WorldCameraConfig_CarWithContinuousZoom : WorldCameraConfig_Car
	{
		public WorldCameraConfig_CarWithContinuousZoom()
		{
			this.zoomSpeed = 0.03f;
			this.zoomPreserveFactor = 1f;
			this.smoothZoom = true;
		}
	}
}
