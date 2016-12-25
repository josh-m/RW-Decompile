using System;

namespace Verse.Noise
{
	public class Max : ModuleBase
	{
		public Max() : base(2)
		{
		}

		public Max(ModuleBase lhs, ModuleBase rhs) : base(2)
		{
			this.modules[0] = lhs;
			this.modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = this.modules[0].GetValue(x, y, z);
			double value2 = this.modules[1].GetValue(x, y, z);
			return Math.Max(value, value2);
		}
	}
}
