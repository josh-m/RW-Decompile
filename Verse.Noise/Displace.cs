using System;

namespace Verse.Noise
{
	public class Displace : ModuleBase
	{
		public ModuleBase X
		{
			get
			{
				return this.modules[1];
			}
			set
			{
				this.modules[1] = value;
			}
		}

		public ModuleBase Y
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

		public ModuleBase Z
		{
			get
			{
				return this.modules[3];
			}
			set
			{
				this.modules[3] = value;
			}
		}

		public Displace() : base(4)
		{
		}

		public Displace(ModuleBase input, ModuleBase x, ModuleBase y, ModuleBase z) : base(4)
		{
			this.modules[0] = input;
			this.modules[1] = x;
			this.modules[2] = y;
			this.modules[3] = z;
		}

		public override double GetValue(double x, double y, double z)
		{
			double x2 = x + this.modules[1].GetValue(x, y, z);
			double y2 = y + this.modules[2].GetValue(x, y, z);
			double z2 = z + this.modules[3].GetValue(x, y, z);
			return this.modules[0].GetValue(x2, y2, z2);
		}
	}
}
