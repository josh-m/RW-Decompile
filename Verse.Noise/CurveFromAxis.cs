using System;
using UnityEngine;

namespace Verse.Noise
{
	public class CurveFromAxis : ModuleBase
	{
		public SimpleCurve curve;

		public CurveFromAxis() : base(0)
		{
		}

		public CurveFromAxis(SimpleCurve curve) : base(0)
		{
			this.curve = curve;
		}

		public override double GetValue(double x, double y, double z)
		{
			float x2 = Mathf.Abs((float)x);
			return (double)this.curve.Evaluate(x2);
		}
	}
}
