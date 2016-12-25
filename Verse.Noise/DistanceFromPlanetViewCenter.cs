using System;
using UnityEngine;

namespace Verse.Noise
{
	public class DistanceFromPlanetViewCenter : ModuleBase
	{
		public Vector3 viewCenter;

		public float viewAngle;

		public bool invert;

		public DistanceFromPlanetViewCenter() : base(0)
		{
		}

		public DistanceFromPlanetViewCenter(Vector3 viewCenter, float viewAngle, bool invert = false) : base(0)
		{
			this.viewCenter = viewCenter;
			this.viewAngle = viewAngle;
			this.invert = invert;
		}

		public override double GetValue(double x, double y, double z)
		{
			float valueInt = this.GetValueInt(x, y, z);
			if (this.invert)
			{
				return (double)(1f - valueInt);
			}
			return (double)valueInt;
		}

		private float GetValueInt(double x, double y, double z)
		{
			if (this.viewAngle >= 180f)
			{
				return 0f;
			}
			float num = Vector3.Angle(this.viewCenter, new Vector3((float)x, (float)y, (float)z));
			return Mathf.Min(num / this.viewAngle, 1f);
		}
	}
}
