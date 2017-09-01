using System;

namespace Verse
{
	public abstract class WorldGenStep
	{
		public WorldGenStepDef def;

		public abstract void GenerateFresh(string seed);

		public abstract void GenerateFromScribe(string seed);
	}
}
