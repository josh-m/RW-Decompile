using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public static class RoofCollapseBufferResolver
	{
		public static void CollapseRoofsMarkedToCollapse()
		{
			if (RoofCollapseBuffer.CellsMarkedToCollapse.Count > 0)
			{
				RoofCollapserImmediate.DropRoofInCells(RoofCollapseBuffer.CellsMarkedToCollapse);
				if (RoofCollapseBuffer.CrushedThingsForLetter.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("RoofCollapsed".Translate());
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("TheseThingsCrushed".Translate());
					HashSet<string> hashSet = new HashSet<string>();
					foreach (Thing current in RoofCollapseBuffer.CrushedThingsForLetter)
					{
						string item = current.LabelShort.CapitalizeFirst();
						if (current.def.category == ThingCategory.Pawn)
						{
							item = current.LabelCap;
						}
						if (!hashSet.Contains(item))
						{
							hashSet.Add(item);
						}
					}
					foreach (string current2 in hashSet)
					{
						stringBuilder.AppendLine("    -" + current2);
					}
					Letter let = new Letter("LetterLabelRoofCollapsed".Translate(), stringBuilder.ToString(), LetterType.BadNonUrgent, RoofCollapseBuffer.CellsMarkedToCollapse[0]);
					Find.LetterStack.ReceiveLetter(let, null);
				}
				else
				{
					string text = "RoofCollapsed".Translate();
					Messages.Message(text, RoofCollapseBuffer.CellsMarkedToCollapse[0], MessageSound.Negative);
				}
				RoofCollapseBuffer.Clear();
			}
		}
	}
}
