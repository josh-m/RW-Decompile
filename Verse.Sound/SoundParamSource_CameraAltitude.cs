using System;

namespace Verse.Sound
{
	public class SoundParamSource_CameraAltitude : SoundParamSource
	{
		public override string Label
		{
			get
			{
				return "Camera altitude";
			}
		}

		public override float ValueFor(Sample samp)
		{
			return Find.Camera.transform.position.y;
		}
	}
}
