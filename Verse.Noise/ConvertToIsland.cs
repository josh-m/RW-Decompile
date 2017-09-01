using System;
using UnityEngine;

namespace Verse.Noise
{
	public class ConvertToIsland : ModuleBase
	{
		private const float WaterLevel = -0.12f;

		public Vector3 viewCenter;

		public float viewAngle;

		public ConvertToIsland() : base(1)
		{
		}

		public ConvertToIsland(Vector3 viewCenter, float viewAngle, ModuleBase input) : base(1)
		{
			this.viewCenter = viewCenter;
			this.viewAngle = viewAngle;
			this.modules[0] = input;
		}

		public override double GetValue(double x, double y, double z)
		{
			float num = Vector3.Angle(this.viewCenter, new Vector3((float)x, (float)y, (float)z));
			double value = this.modules[0].GetValue(x, y, z);
			float num2 = Mathf.Max(2.5f, this.viewAngle * 0.25f);
			float num3 = Mathf.Max(0.8f, this.viewAngle * 0.1f);
			if (num < this.viewAngle - num2)
			{
				return value;
			}
			float num4 = GenMath.LerpDouble(this.viewAngle - num2, this.viewAngle - num3, 0f, 0.62f, num);
			if (value > -0.11999999731779099)
			{
				return (value - -0.11999999731779099) * (double)(1f - num4 * 0.7f) - (double)(num4 * 0.3f) + -0.11999999731779099;
			}
			return value - (double)(num4 * 0.3f);
		}
	}
}
