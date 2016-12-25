using System;

namespace Verse.Noise
{
	public class Multiply : ModuleBase
	{
		public Multiply() : base(2)
		{
		}

		public Multiply(ModuleBase lhs, ModuleBase rhs) : base(2)
		{
			this.modules[0] = lhs;
			this.modules[1] = rhs;
		}

		public override double GetValue(double x, double y, double z)
		{
			return this.modules[0].GetValue(x, y, z) * this.modules[1].GetValue(x, y, z);
		}
	}
}
