using System;

namespace Verse.Noise
{
	public class Const : ModuleBase
	{
		private double val;

		public double Value
		{
			get
			{
				return this.val;
			}
			set
			{
				this.val = value;
			}
		}

		public Const() : base(0)
		{
		}

		public Const(double value) : base(0)
		{
			this.Value = value;
		}

		public override double GetValue(double x, double y, double z)
		{
			return this.val;
		}
	}
}
