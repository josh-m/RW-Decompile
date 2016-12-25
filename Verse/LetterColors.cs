using System;
using UnityEngine;

namespace Verse
{
	public static class LetterColors
	{
		private static readonly Color Good;

		private static readonly Color BadNonUrgent;

		private static readonly Color BadUrgent;

		private static readonly Color FlashGood;

		private static readonly Color FlashBadNonUrgent;

		private static readonly Color FlashBadUrgent;

		static LetterColors()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ColorInt colorInt = new ColorInt(120, 176, 216);
			LetterColors.Good = colorInt.ToColor;
			LetterColors.BadNonUrgent = new Color(0.8f, 0.77f, 0.53f);
			LetterColors.BadUrgent = new Color(0.8f, 0.45f, 0.45f);
			LetterColors.FlashGood = LetterColors.Good.SaturationChanged(1.3f);
			LetterColors.FlashBadNonUrgent = LetterColors.BadNonUrgent.SaturationChanged(1.5f);
			LetterColors.FlashBadUrgent = LetterColors.BadUrgent.SaturationChanged(1.9f);
		}

		public static Color GetColor(this LetterType lt)
		{
			switch (lt)
			{
			case LetterType.Good:
				return LetterColors.Good;
			case LetterType.BadNonUrgent:
				return LetterColors.BadNonUrgent;
			case LetterType.BadUrgent:
				return LetterColors.BadUrgent;
			default:
				throw new NotImplementedException();
			}
		}

		public static Color GetColorFlash(this LetterType lt)
		{
			switch (lt)
			{
			case LetterType.Good:
				return LetterColors.FlashGood;
			case LetterType.BadNonUrgent:
				return LetterColors.FlashBadNonUrgent;
			case LetterType.BadUrgent:
				return LetterColors.FlashBadUrgent;
			default:
				throw new NotImplementedException();
			}
		}
	}
}
