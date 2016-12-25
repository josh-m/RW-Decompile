using System;
using System.Collections.Generic;

namespace Verse
{
	public class GenStep_RandomSelector : GenStep
	{
		public List<RandomGenStepSelectorOption> options;

		public override void Generate(Map map)
		{
			RandomGenStepSelectorOption randomGenStepSelectorOption = this.options.RandomElementByWeight((RandomGenStepSelectorOption opt) => opt.weight);
			if (randomGenStepSelectorOption.genStep != null)
			{
				randomGenStepSelectorOption.genStep.Generate(map);
			}
			if (randomGenStepSelectorOption.def != null)
			{
				randomGenStepSelectorOption.def.genStep.Generate(map);
			}
		}
	}
}
