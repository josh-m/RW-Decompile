using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class MoteSplash : Mote
	{
		public const float VelocityFootstep = 1.5f;

		public const float SizeFootstep = 2f;

		public const float VelocityGunfire = 4f;

		public const float SizeGunfire = 1f;

		public const float VelocityExplosion = 20f;

		public const float SizeExplosion = 6f;

		private float targetSize;

		private float velocity;

		public override DrawTargetDef DrawLayer
		{
			get
			{
				return DrawTargetDefOf.WaterHeight;
			}
		}

		protected override float LifespanSecs
		{
			get
			{
				return this.targetSize / this.velocity;
			}
		}

		public void Initialize(Vector3 position, float size, float velocity)
		{
			this.exactPosition = position;
			this.targetSize = size;
			this.velocity = velocity;
			base.Scale = 0f;
		}

		protected override void TimeInterval(float deltaTime)
		{
			base.TimeInterval(deltaTime);
			if (base.Destroyed)
			{
				return;
			}
			float scale = base.AgeSecs * this.velocity;
			base.Scale = scale;
		}

		public float CalculatedIntensity()
		{
			return Mathf.Sqrt(this.targetSize) / 10f;
		}

		public float CalculatedAlpha()
		{
			float num = Mathf.Clamp01(base.AgeSecs * 10f);
			num = 1f;
			float num2 = Mathf.Clamp01(1f - base.AgeSecs / this.LifespanSecs);
			return num * num2 * this.CalculatedIntensity();
		}

		public float CalculatedShockwaveSpan()
		{
			float num = Mathf.Sqrt(this.targetSize) * 0.8f;
			num = Mathf.Min(num, this.exactScale.x);
			return num / this.exactScale.x;
		}
	}
}
