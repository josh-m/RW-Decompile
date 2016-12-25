using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class WorkTypeDefsUtility
	{
		public static IEnumerable<WorkTypeDef> WorkTypeDefsInPriorityOrder
		{
			get
			{
				return from wt in DefDatabase<WorkTypeDef>.AllDefs
				orderby wt.naturalPriority descending
				select wt;
			}
		}

		public static string LabelTranslated(this WorkTags tags)
		{
			switch (tags)
			{
			case WorkTags.None:
				return "WorkTagNone".Translate();
			case (WorkTags)1:
			case (WorkTags)3:
				IL_1C:
				if (tags == WorkTags.ManualSkilled)
				{
					return "WorkTagManualSkilled".Translate();
				}
				if (tags == WorkTags.Violent)
				{
					return "WorkTagViolent".Translate();
				}
				if (tags == WorkTags.Caring)
				{
					return "WorkTagCaring".Translate();
				}
				if (tags == WorkTags.Social)
				{
					return "WorkTagSocial".Translate();
				}
				if (tags == WorkTags.Scary)
				{
					return "WorkTagScary".Translate();
				}
				if (tags == WorkTags.Animals)
				{
					return "WorkTagAnimals".Translate();
				}
				if (tags == WorkTags.Artistic)
				{
					return "WorkTagArtistic".Translate();
				}
				if (tags == WorkTags.Crafting)
				{
					return "WorkTagCrafting".Translate();
				}
				if (tags == WorkTags.Cooking)
				{
					return "WorkTagCooking".Translate();
				}
				if (tags == WorkTags.Firefighting)
				{
					return "WorkTagFirefighting".Translate();
				}
				if (tags == WorkTags.Cleaning)
				{
					return "WorkTagCleaning".Translate();
				}
				if (tags == WorkTags.Hauling)
				{
					return "WorkTagHauling".Translate();
				}
				if (tags == WorkTags.PlantWork)
				{
					return "WorkTagPlantWork".Translate();
				}
				if (tags != WorkTags.Mining)
				{
					Log.Error("Unknown or mixed worktags for naming: " + (int)tags);
					return "Worktag";
				}
				return "WorkTagMining".Translate();
			case WorkTags.Intellectual:
				return "WorkTagIntellectual".Translate();
			case WorkTags.ManualDumb:
				return "WorkTagManualDumb".Translate();
			}
			goto IL_1C;
		}

		public static bool OverlapsWithOnAnyWorkType(this WorkTags a, WorkTags b)
		{
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				if ((workTypeDef.workTags & a) != WorkTags.None && (workTypeDef.workTags & b) != WorkTags.None)
				{
					return true;
				}
			}
			return false;
		}
	}
}
