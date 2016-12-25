using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public class RoofCollapseBufferResolver
	{
		private Map map;

		public RoofCollapseBufferResolver(Map map)
		{
			this.map = map;
		}

		public void CollapseRoofsMarkedToCollapse()
		{
			RoofCollapseBuffer roofCollapseBuffer = this.map.roofCollapseBuffer;
			if (roofCollapseBuffer.CellsMarkedToCollapse.Count > 0)
			{
				RoofCollapserImmediate.DropRoofInCells(roofCollapseBuffer.CellsMarkedToCollapse, this.map);
				if (roofCollapseBuffer.CrushedThingsForLetter.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("RoofCollapsed".Translate());
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("TheseThingsCrushed".Translate());
					HashSet<string> hashSet = new HashSet<string>();
					foreach (Thing current in roofCollapseBuffer.CrushedThingsForLetter)
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
					Letter let = new Letter("LetterLabelRoofCollapsed".Translate(), stringBuilder.ToString(), LetterType.BadNonUrgent, new TargetInfo(roofCollapseBuffer.CellsMarkedToCollapse[0], this.map, false));
					Find.LetterStack.ReceiveLetter(let, null);
				}
				else
				{
					string text = "RoofCollapsed".Translate();
					Messages.Message(text, new TargetInfo(roofCollapseBuffer.CellsMarkedToCollapse[0], this.map, false), MessageSound.Negative);
				}
				roofCollapseBuffer.Clear();
			}
		}
	}
}
