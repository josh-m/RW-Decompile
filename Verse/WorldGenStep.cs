using System;

namespace Verse
{
	public abstract class WorldGenStep
	{
		public WorldGenStepDef def;

		public abstract void GenerateFresh(string seed);

		public virtual void GenerateWithoutWorldData(string seed)
		{
			this.GenerateFresh(seed);
		}

		public virtual void GenerateFromScribe(string seed)
		{
		}
	}
}
