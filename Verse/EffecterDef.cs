using System;
using System.Collections.Generic;

namespace Verse
{
	public class EffecterDef : Def
	{
		public List<SubEffecterDef> children;

		public Effecter Spawn()
		{
			return new Effecter(this);
		}

		public static EffecterDef Named(string defName)
		{
			return DefDatabase<EffecterDef>.GetNamed(defName, true);
		}
	}
}
