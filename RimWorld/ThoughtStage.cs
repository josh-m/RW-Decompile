using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public class ThoughtStage
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string labelSocial;

		[MustTranslate]
		public string description;

		public float baseMoodEffect;

		public float baseOpinionOffset;

		public bool visible = true;

		[TranslationHandle(Priority = 100), Unsaved]
		public string untranslatedLabel;

		[TranslationHandle, Unsaved]
		public string untranslatedLabelSocial;

		public void PostLoad()
		{
			this.untranslatedLabel = this.label;
			this.untranslatedLabelSocial = this.labelSocial;
		}

		[DebuggerHidden]
		public IEnumerable<string> ConfigErrors()
		{
			if (!this.labelSocial.NullOrEmpty() && this.labelSocial == this.label)
			{
				yield return "labelSocial is the same as label. labelSocial is unnecessary in this case";
			}
			if (this.baseMoodEffect != 0f && this.description.NullOrEmpty())
			{
				yield return "affects mood but doesn't have a description";
			}
		}
	}
}
