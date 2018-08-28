using System;

namespace Verse
{
	public class CameraMapConfig_CarWithContinuousZoom : CameraMapConfig_Car
	{
		public CameraMapConfig_CarWithContinuousZoom()
		{
			this.zoomSpeed = 0.043f;
			this.zoomPreserveFactor = 1f;
			this.smoothZoom = true;
		}
	}
}
