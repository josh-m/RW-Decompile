using System;

namespace Verse
{
	public class CameraMapConfig_ContinuousPan : CameraMapConfig
	{
		public CameraMapConfig_ContinuousPan()
		{
			this.dollyRateKeys = 10f;
			this.dollyRateMouseDrag = 4f;
			this.dollyRateScreenEdge = 5f;
			this.camSpeedDecayFactor = 1f;
			this.moveSpeedScale = 1f;
		}
	}
}
