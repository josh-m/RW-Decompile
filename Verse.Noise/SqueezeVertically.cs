using System;

namespace Verse.Noise
{
	public class SqueezeVertically : ModuleBase
	{
		private float factor;

		public SqueezeVertically(ModuleBase input, float factor) : base(1)
		{
			this.modules[0] = input;
			this.factor = factor;
		}

		public override double GetValue(double x, double y, double z)
		{
			return this.modules[0].GetValue(x, y * (double)this.factor, z);
		}
	}
}
