using RimWorld.Planet;
using System;

namespace Verse
{
	public static class LetterMaker
	{
		public static Letter MakeLetter(LetterDef def)
		{
			Letter letter = (Letter)Activator.CreateInstance(def.letterClass);
			letter.def = def;
			return letter;
		}

		public static ChoiceLetter MakeLetter(string label, string text, LetterDef def)
		{
			if (!typeof(ChoiceLetter).IsAssignableFrom(def.letterClass))
			{
				Log.Error(def + " is not a choice letter.");
				return null;
			}
			ChoiceLetter choiceLetter = (ChoiceLetter)LetterMaker.MakeLetter(def);
			choiceLetter.label = label;
			choiceLetter.text = text;
			return choiceLetter;
		}

		public static ChoiceLetter MakeLetter(string label, string text, LetterDef def, GlobalTargetInfo lookTarget)
		{
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, def);
			choiceLetter.lookTarget = lookTarget;
			return choiceLetter;
		}
	}
}
