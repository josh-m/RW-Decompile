using System;

namespace Verse
{
	public abstract class GenStep
	{
		public GenStepDef def;

		public abstract void Generate(Map map);
	}
}
