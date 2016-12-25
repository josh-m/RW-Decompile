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

		public List<GenStepDef> GenStepsInOrder
		{
			get
			{
				if (this.genSteps == null)
				{
					this.genSteps = new List<GenStepDef>();
					if (this.parent != null)
					{
						this.genSteps.AddRange(this.parent.GenStepsInOrder);
					}
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
