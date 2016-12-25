using System;
using UnityEngine;

namespace Verse
{
	public class JitterHandler
	{
		private Vector3 curOffset = new Vector3(0f, 0f, 0f);

		private float DamageJitterDistance = 0.17f;

		private float JitterDropPerTick = 0.018f;

		private float JitterMax = 0.35f;

		public Vector3 CurrentOffset
		{
			get
			{
				return this.curOffset;
			}
		}

		public void JitterHandlerTick()
		{
			if (this.curOffset.sqrMagnitude < this.JitterDropPerTick * this.JitterDropPerTick)
			{
				this.curOffset = new Vector3(0f, 0f, 0f);
			}
			else
			{
				this.curOffset -= this.curOffset.normalized * this.JitterDropPerTick;
			}
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (dinfo.Def.hasForcefulImpact)
			{
				this.AddOffset(this.DamageJitterDistance, dinfo.Angle);
			}
		}

		public void AddOffset(float dist, float dir)
		{
			this.curOffset += Quaternion.AngleAxis(dir, Vector3.up) * Vector3.forward * dist;
			if (this.curOffset.sqrMagnitude > this.JitterMax * this.JitterMax)
			{
				this.curOffset *= this.JitterMax / this.curOffset.magnitude;
			}
		}
	}
}
