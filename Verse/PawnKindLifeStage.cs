using System;

namespace Verse
{
	public class PawnKindLifeStage
	{
		public string label;

		public string labelPlural;

		public string labelMale;

		public string labelMalePlural;

		public string labelFemale;

		public string labelFemalePlural;

		public GraphicData bodyGraphicData;

		public GraphicData femaleGraphicData;

		public GraphicData dessicatedBodyGraphicData;

		public BodyPartToDrop dropBodyPart;

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
		}
	}
}
