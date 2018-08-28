using RimWorld;
using System;

namespace Verse
{
	public static class LetterMaker
	{
		public static Letter MakeLetter(LetterDef def)
		{
			Letter letter = (Letter)Activator.CreateInstance(def.letterClass);
			letter.def = def;
			letter.ID = Find.UniqueIDsManager.GetNextLetterID();
			return letter;
		}

		public static ChoiceLetter MakeLetter(string label, string text, LetterDef def)
		{
			if (!typeof(ChoiceLetter).IsAssignableFrom(def.letterClass))
			{
				Log.Error(def + " is not a choice letter.", false);
				return null;
			}
			ChoiceLetter choiceLetter = (ChoiceLetter)LetterMaker.MakeLetter(def);
			choiceLetter.label = label;
			choiceLetter.text = text;
			return choiceLetter;
		}

		public static ChoiceLetter MakeLetter(string label, string text, LetterDef def, LookTargets lookTargets, Faction relatedFaction = null)
		{
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, def);
			choiceLetter.lookTargets = lookTargets;
			choiceLetter.relatedFaction = relatedFaction;
			return choiceLetter;
		}
	}
}
