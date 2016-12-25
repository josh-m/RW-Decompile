using System;

namespace Verse
{
	public abstract class RandomNumberGenerator
	{
		public uint seed = (uint)DateTime.Now.GetHashCode();

		public abstract int GetInt(uint iterations);

		public float GetFloat(uint iterations)
		{
			return (float)(((double)this.GetInt(iterations) - -2147483648.0) / 4294967295.0);
		}
	}
}
