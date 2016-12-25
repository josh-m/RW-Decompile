using System;

namespace Verse
{
	public class CameraMapConfig_Normal : CameraMapConfig
	{
		public CameraMapConfig_Normal()
		{
			this.dollyRateKeys = 50f;
			this.dollyRateMouseDrag = 6.5f;
			this.dollyRateScreenEdge = 35f;
			this.camSpeedDecayFactor = 0.85f;
			this.moveSpeedScale = 2f;
		}
	}
}
