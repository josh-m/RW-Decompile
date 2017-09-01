using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class KeyBindingDefGenerator
	{
		[DebuggerHidden]
		public static IEnumerable<KeyBindingCategoryDef> ImpliedKeyBindingCategoryDefs()
		{
			List<KeyBindingCategoryDef> gameUniversalCats = (from d in DefDatabase<KeyBindingCategoryDef>.AllDefs
			where d.isGameUniversal
			select d).ToList<KeyBindingCategoryDef>();
			foreach (DesignationCategoryDef def in DefDatabase<DesignationCategoryDef>.AllDefs)
			{
				KeyBindingCategoryDef catDef = new KeyBindingCategoryDef();
				catDef.defName = "Architect_" + def.defName;
				catDef.label = def.label + " tab";
				catDef.description = "Key bindings for the \"" + def.LabelCap + "\" section of the Architect menu";
				catDef.checkForConflicts.AddRange(gameUniversalCats);
				for (int i = 0; i < gameUniversalCats.Count; i++)
				{
					gameUniversalCats[i].checkForConflicts.Add(catDef);
				}
				def.bindingCatDef = catDef;
				yield return catDef;
			}
		}

		[DebuggerHidden]
		public static IEnumerable<KeyBindingDef> ImpliedKeyBindingDefs()
		{
			foreach (MainButtonDef mainTab in from td in DefDatabase<MainButtonDef>.AllDefs
			orderby td.order
			select td)
			{
				if (mainTab.defaultHotKey != KeyCode.None)
				{
					KeyBindingDef keyDef = new KeyBindingDef();
					keyDef.label = "Toggle " + mainTab.label + " tab";
					keyDef.defName = "MainTab_" + mainTab.defName;
					keyDef.category = KeyBindingCategoryDefOf.MainTabs;
					keyDef.defaultKeyCodeA = mainTab.defaultHotKey;
					mainTab.hotKey = keyDef;
					yield return keyDef;
				}
			}
		}
	}
}
