using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ShortcutKeys
	{
		public void ShortcutKeysOnGUI()
		{
			if (KeyBindingDefOf.NextColonist.KeyDownEvent)
			{
				Find.Selector.SelectNextColonist();
				Event.current.Use();
			}
			if (KeyBindingDefOf.PreviousColonist.KeyDownEvent)
			{
				Find.Selector.SelectPreviousColonist();
				Event.current.Use();
			}
		}
	}
}
