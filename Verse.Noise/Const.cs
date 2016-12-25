using System;

namespace Verse.Noise
{
	public class Const : ModuleBase
	{
		private double m_value;

		public double Value
		{
			get
			{
				return this.m_value;
			}
			set
			{
				this.m_value = value;
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
			return this.m_value;
		}
	}
}
