using System;
using UnityEngine;

namespace Verse
{
	public class JitterHandler
	{
		protected Vector3 JitterOffset = new Vector3(0f, 0f, 0f);

		public float DamageJitterDistance = 0.17f;

		public float JitterDropPerTick = 0.018f;

		public float JitterMax = 0.35f;

		public Vector3 CurrentJitterOffset
		{
			get
			{
				return this.JitterOffset;
			}
		}

		public void JitterHandlerTick()
		{
			if (this.JitterOffset.sqrMagnitude < this.JitterDropPerTick * this.JitterDropPerTick)
			{
				this.JitterOffset = new Vector3(0f, 0f, 0f);
			}
			else
			{
				this.JitterOffset -= this.JitterOffset.normalized * this.JitterDropPerTick;
			}
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (dinfo.Def.hasForcefulImpact)
			{
				this.AddOffset(this.DamageJitterDistance, dinfo.Angle);
			}
		}

		public void AddOffset(float Distance, float Direction)
		{
			this.JitterOffset += Quaternion.AngleAxis(Direction, Vector3.up) * Vector3.forward * Distance;
			if (this.JitterOffset.sqrMagnitude > this.JitterMax * this.JitterMax)
			{
				this.JitterOffset *= this.JitterMax / this.JitterOffset.magnitude;
			}
		}
	}
}
