using System;

namespace Verse
{
	public class RoomStatScoreStage
	{
		public float minScore = -3.40282347E+38f;

		public string label;

		[TranslationHandle, Unsaved]
		public string untranslatedLabel;

		public void PostLoad()
		{
			this.untranslatedLabel = this.label;
		}
	}
}
