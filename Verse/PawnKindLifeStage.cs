using System;

namespace Verse
{
	public class PawnKindLifeStage
	{
		[MustTranslate]
		public string label;

		[MustTranslate]
		public string labelPlural;

		[MustTranslate]
		public string labelMale;

		[MustTranslate]
		public string labelMalePlural;

		[MustTranslate]
		public string labelFemale;

		[MustTranslate]
		public string labelFemalePlural;

		[TranslationHandle(Priority = 200), Unsaved]
		public string untranslatedLabel;

		[TranslationHandle(Priority = 100), Unsaved]
		public string untranslatedLabelMale;

		[TranslationHandle, Unsaved]
		public string untranslatedLabelFemale;

		public GraphicData bodyGraphicData;

		public GraphicData femaleGraphicData;

		public GraphicData dessicatedBodyGraphicData;

		public GraphicData femaleDessicatedBodyGraphicData;

		public BodyPartToDrop butcherBodyPart;

		public void PostLoad()
		{
			this.untranslatedLabel = this.label;
			this.untranslatedLabelMale = this.labelMale;
			this.untranslatedLabelFemale = this.labelFemale;
		}

		public void ResolveReferences()
		{
			if (this.bodyGraphicData != null && this.bodyGraphicData.graphicClass == null)
			{
				this.bodyGraphicData.graphicClass = typeof(Graphic_Multi);
			}
			if (this.femaleGraphicData != null && this.femaleGraphicData.graphicClass == null)
			{
				this.femaleGraphicData.graphicClass = typeof(Graphic_Multi);
			}
			if (this.dessicatedBodyGraphicData != null && this.dessicatedBodyGraphicData.graphicClass == null)
			{
				this.dessicatedBodyGraphicData.graphicClass = typeof(Graphic_Multi);
			}
			if (this.femaleDessicatedBodyGraphicData != null && this.femaleDessicatedBodyGraphicData.graphicClass == null)
			{
				this.femaleDessicatedBodyGraphicData.graphicClass = typeof(Graphic_Multi);
			}
		}
	}
}
