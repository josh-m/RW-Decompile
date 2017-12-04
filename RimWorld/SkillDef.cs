using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class SkillDef : Def
	{
		public string skillLabel;

		public bool usuallyDefinedInBackstories = true;

		public bool pawnCreatorSummaryVisible;

		public WorkTags disablingWorkTags;

		public override void PostLoad()
		{
			if (this.label == null)
			{
				this.label = this.skillLabel;
			}
		}

		public bool IsDisabled(WorkTags combinedDisabledWorkTags, IEnumerable<WorkTypeDef> disabledWorkTypes)
		{
			if ((combinedDisabledWorkTags & this.disablingWorkTags) != WorkTags.None)
			{
				return true;
			}
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			bool result = false;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				for (int j = 0; j < workTypeDef.relevantSkills.Count; j++)
				{
					if (workTypeDef.relevantSkills[j] == this)
					{
						if (!disabledWorkTypes.Contains(workTypeDef))
						{
							return false;
						}
						result = true;
					}
				}
			}
			return result;
		}
	}
}
