using System;
using System.Collections.Generic;

namespace Verse
{
	public class MapGeneratorDef : Def
	{
		public MapGeneratorDef parent;

		public float selectionWeight = 1f;

		[Unsaved]
		private List<GenStepDef> genSteps;

		public List<GenStepDef> GenSteps
		{
			get
			{
				if (this.genSteps == null)
				{
					this.genSteps = new List<GenStepDef>();
					if (this.parent != null)
					{
						this.genSteps.AddRange(this.parent.GenSteps);
					}
					List<GenStepDef> allDefsListForReading = DefDatabase<GenStepDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].linkWithMapGenerator == this)
						{
							this.genSteps.Add(allDefsListForReading[i]);
						}
					}
				}
				return this.genSteps;
			}
		}
	}
}
