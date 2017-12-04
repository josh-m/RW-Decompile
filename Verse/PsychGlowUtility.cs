using System;

namespace Verse
{
	public static class PsychGlowUtility
	{
		public static string GetLabel(this PsychGlow gl)
		{
			if (gl == PsychGlow.Dark)
			{
				return "Dark".Translate();
			}
			if (gl == PsychGlow.Lit)
			{
				return "Lit".Translate();
			}
			if (gl != PsychGlow.Overlit)
			{
				throw new ArgumentException();
			}
			return "LitBrightly".Translate();
		}
	}
}
