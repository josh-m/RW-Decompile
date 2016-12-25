using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class BuildCopyCommandUtility
	{
		public static Command BuildCopyCommand(BuildableDef buildable, ThingDef stuff)
		{
			Designator_Build des = BuildCopyCommandUtility.FindDesignator(buildable);
			if (des == null)
			{
				return null;
			}
			if (!des.Visible)
			{
				return null;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.action = delegate
			{
				SoundDefOf.SelectDesignator.PlayOneShotOnCamera();
				des.SetStuffDef(stuff);
				Find.DesignatorManager.Select(des);
			};
			command_Action.defaultLabel = "CommandBuildCopy".Translate();
			command_Action.defaultDesc = "CommandBuildCopyDesc".Translate();
			command_Action.icon = des.icon;
			command_Action.iconProportions = des.iconProportions;
			command_Action.iconDrawScale = des.iconDrawScale;
			command_Action.iconTexCoords = des.iconTexCoords;
			if (stuff != null)
			{
				command_Action.defaultIconColor = stuff.stuffProps.color;
			}
			else
			{
				command_Action.defaultIconColor = buildable.IconDrawColor;
			}
			command_Action.hotKey = KeyBindingDefOf.Misc11;
			return command_Action;
		}

		private static Designator_Build FindDesignator(BuildableDef buildable)
		{
			List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				foreach (Designator current in allDefsListForReading[i].ResolvedAllowedDesignators)
				{
					Designator_Build designator_Build = current as Designator_Build;
					if (designator_Build != null && designator_Build.PlacingDef == buildable)
					{
						return designator_Build;
					}
				}
			}
			return null;
		}
	}
}
