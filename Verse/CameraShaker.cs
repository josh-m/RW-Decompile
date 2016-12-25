using System;
using UnityEngine;

namespace Verse
{
	public class CameraShaker
	{
		private const float ShakeDecayRate = 0.5f;

		private const float ShakeFrequency = 24f;

		private const float MaxShakeMag = 0.1f;

		private float curShakeMag;

		public Vector3 ShakeOffset
		{
			get
			{
				float x = Mathf.Sin(Time.realtimeSinceStartup * 24f) * this.curShakeMag;
				float y = Mathf.Sin(Time.realtimeSinceStartup * 24f * 1.05f) * this.curShakeMag;
				float z = Mathf.Sin(Time.realtimeSinceStartup * 24f * 1.1f) * this.curShakeMag;
				return new Vector3(x, y, z);
			}
		}

		public void DoShake(float mag)
		{
			this.curShakeMag += mag;
			if (this.curShakeMag > 0.1f)
			{
				this.curShakeMag = 0.1f;
			}
		}

		public void Update()
		{
			this.curShakeMag -= 0.5f * RealTime.realDeltaTime;
			if (this.curShakeMag < 0f)
			{
				this.curShakeMag = 0f;
			}
		}
	}
}
