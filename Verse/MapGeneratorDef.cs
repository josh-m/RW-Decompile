using System;
using System.Collections.Generic;

namespace Verse
{
	public class MapGeneratorDef : Def
	{
		[Unsaved]
		private List<GenStepDef> genSteps;

		public List<GenStepDef> GenStepsInOrder
		{
			get
			{
				if (this.genSteps == null)
				{
					this.genSteps = new List<GenStepDef>();
					List<GenStepDef> allDefsListForReading = DefDatabase<GenStepDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].mapGenerator == this)
						{
							this.genSteps.Add(allDefsListForReading[i]);
						}
					}
					this.genSteps.SortBy((GenStepDef x) => x.order, (GenStepDef x) => x.index);
				}
				return this.genSteps;
			}
		}
	}
}
