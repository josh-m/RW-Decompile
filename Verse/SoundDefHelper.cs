using System;

namespace Verse
{
	public static class SoundDefHelper
	{
		public static bool NullOrUndefined(this SoundDef def)
		{
			return def == null || def.isUndefined;
		}
	}
}
