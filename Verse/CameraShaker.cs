using System;
using UnityEngine;

namespace Verse
{
	public class CameraShaker
	{
		private float curShakeMag;

		private const float ShakeDecayRate = 0.5f;

		private const float ShakeFrequency = 24f;

		private const float MaxShakeMag = 0.2f;

		public float CurShakeMag
		{
			get
			{
				return this.curShakeMag;
			}
			set
			{
				this.curShakeMag = Mathf.Clamp(value, 0f, 0.2f);
			}
		}

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
			if (mag <= 0f)
			{
				return;
			}
			this.CurShakeMag += mag;
		}

		public void SetMinShake(float mag)
		{
			this.CurShakeMag = Mathf.Max(this.CurShakeMag, mag);
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
