using System;

namespace Verse
{
	public static class AnimalNameDisplayModeExtension
	{
		public static string ToStringHuman(this AnimalNameDisplayMode mode)
		{
			if (mode == AnimalNameDisplayMode.None)
			{
				return "None".Translate();
			}
			if (mode == AnimalNameDisplayMode.TameNamed)
			{
				return "AnimalNameDisplayMode_TameNamed".Translate();
			}
			if (mode != AnimalNameDisplayMode.TameAll)
			{
				throw new NotImplementedException();
			}
			return "AnimalNameDisplayMode_TameAll".Translate();
		}
	}
}
