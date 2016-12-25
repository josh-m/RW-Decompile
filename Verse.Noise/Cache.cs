using System;

namespace Verse.Noise
{
	public class Cache : ModuleBase
	{
		private double m_value;

		private bool m_cached;

		private double m_x;

		private double m_y;

		private double m_z;

		public override ModuleBase this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				base[index] = value;
				this.m_cached = false;
			}
		}

		public Cache() : base(1)
		{
		}

		public Cache(ModuleBase input) : base(1)
		{
			this.modules[0] = input;
		}

		public override double GetValue(double x, double y, double z)
		{
			if (!this.m_cached || this.m_x != x || this.m_y != y || this.m_z != z)
			{
				this.m_value = this.modules[0].GetValue(x, y, z);
				this.m_x = x;
				this.m_y = y;
				this.m_z = z;
			}
			this.m_cached = true;
			return this.m_value;
		}
	}
}
