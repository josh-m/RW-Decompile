using System;
using System.Collections.Generic;

namespace Verse
{
	public class KeyBindingCategoryDef : Def
	{
		public bool isGameUniversal;

		public List<KeyBindingCategoryDef> checkForConflicts = new List<KeyBindingCategoryDef>();

		public static KeyBindingCategoryDef Named(string defName)
		{
			return DefDatabase<KeyBindingCategoryDef>.GetNamed(defName, true);
		}
	}
}
