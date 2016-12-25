using System;

namespace Verse.Noise
{
	public class Select : ModuleBase
	{
		private double m_fallOff;

		private double m_raw;

		private double m_min = -1.0;

		private double m_max = 1.0;

		public ModuleBase Controller
		{
			get
			{
				return this.modules[2];
			}
			set
			{
				this.modules[2] = value;
			}
		}

		public double FallOff
		{
			get
			{
				return this.m_fallOff;
			}
			set
			{
				double num = this.m_max - this.m_min;
				this.m_raw = value;
				this.m_fallOff = ((value <= num / 2.0) ? value : (num / 2.0));
			}
		}

		public double Maximum
		{
			get
			{
				return this.m_max;
			}
			set
			{
				this.m_max = value;
				this.FallOff = this.m_raw;
			}
		}

		public double Minimum
		{
			get
			{
				return this.m_min;
			}
			set
			{
				this.m_min = value;
				this.FallOff = this.m_raw;
			}
		}

		public Select() : base(3)
		{
		}

		public Select(ModuleBase inputA, ModuleBase inputB, ModuleBase controller) : base(3)
		{
			this.modules[0] = inputA;
			this.modules[1] = inputB;
			this.modules[2] = controller;
		}

		public Select(double min, double max, double fallOff, ModuleBase inputA, ModuleBase inputB) : this(inputA, inputB, null)
		{
			this.m_min = min;
			this.m_max = max;
			this.FallOff = fallOff;
		}

		public void SetBounds(double min, double max)
		{
			this.m_min = min;
			this.m_max = max;
			this.FallOff = this.m_fallOff;
		}

		public override double GetValue(double x, double y, double z)
		{
			double value = this.modules[2].GetValue(x, y, z);
			if (this.m_fallOff > 0.0)
			{
				if (value < this.m_min - this.m_fallOff)
				{
					return this.modules[0].GetValue(x, y, z);
				}
				if (value < this.m_min + this.m_fallOff)
				{
					double num = this.m_min - this.m_fallOff;
					double num2 = this.m_min + this.m_fallOff;
					double position = Utils.MapCubicSCurve((value - num) / (num2 - num));
					return Utils.InterpolateLinear(this.modules[0].GetValue(x, y, z), this.modules[1].GetValue(x, y, z), position);
				}
				if (value < this.m_max - this.m_fallOff)
				{
					return this.modules[1].GetValue(x, y, z);
				}
				if (value < this.m_max + this.m_fallOff)
				{
					double num3 = this.m_max - this.m_fallOff;
					double num4 = this.m_max + this.m_fallOff;
					double position = Utils.MapCubicSCurve((value - num3) / (num4 - num3));
					return Utils.InterpolateLinear(this.modules[1].GetValue(x, y, z), this.modules[0].GetValue(x, y, z), position);
				}
				return this.modules[0].GetValue(x, y, z);
			}
			else
			{
				if (value < this.m_min || value > this.m_max)
				{
					return this.modules[0].GetValue(x, y, z);
				}
				return this.modules[1].GetValue(x, y, z);
			}
		}
	}
}
