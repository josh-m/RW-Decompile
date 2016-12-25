using System;

namespace Verse.Noise
{
	public class ScaleBias : ModuleBase
	{
		private double scale = 1.0;

		private double bias;

		public double Bias
		{
			get
			{
				return this.bias;
			}
			set
			{
				this.bias = value;
			}
		}

		public double Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				this.scale = value;
			}
		}

		public ScaleBias() : base(1)
		{
		}

		public ScaleBias(ModuleBase input) : base(1)
		{
			this.modules[0] = input;
		}

		public ScaleBias(double scale, double bias, ModuleBase input) : base(1)
		{
			this.modules[0] = input;
			this.Bias = bias;
			this.Scale = scale;
		}

		public override double GetValue(double x, double y, double z)
		{
			return this.modules[0].GetValue(x, y, z) * this.scale + this.bias;
		}
	}
}
