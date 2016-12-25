using System;

namespace Verse.Noise
{
	public class CurveSimple : ModuleBase
	{
		private SimpleCurve curve;

		public CurveSimple(ModuleBase input, SimpleCurve curve) : base(1)
		{
			this.modules[0] = input;
			this.curve = curve;
		}

		public override double GetValue(double x, double y, double z)
		{
			return (double)this.curve.Evaluate((float)this.modules[0].GetValue(x, y, z));
		}
	}
}
