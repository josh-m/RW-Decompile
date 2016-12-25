using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class TrainableUtility
	{
		private static List<TrainableDef> defsInListOrder = new List<TrainableDef>();

		public static List<TrainableDef> TrainableDefsInListOrder
		{
			get
			{
				return TrainableUtility.defsInListOrder;
			}
		}

		public static void Reset()
		{
			TrainableUtility.defsInListOrder.Clear();
			TrainableUtility.defsInListOrder.AddRange(from td in DefDatabase<TrainableDef>.AllDefsListForReading
			orderby td.listPriority descending
			select td);
			bool flag;
			do
			{
				flag = false;
				for (int i = 0; i < TrainableUtility.defsInListOrder.Count; i++)
				{
					TrainableDef trainableDef = TrainableUtility.defsInListOrder[i];
					if (trainableDef.prerequisites != null)
					{
						for (int j = 0; j < trainableDef.prerequisites.Count; j++)
						{
							if (trainableDef.indent <= trainableDef.prerequisites[j].indent)
							{
								trainableDef.indent = trainableDef.prerequisites[j].indent + 1;
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			while (flag);
		}

		public static string MasterString(Pawn pawn)
		{
			return (pawn.playerSettings.master == null) ? ("(" + "NoneLower".Translate() + ")") : RelationsUtility.LabelWithBondInfo(pawn.playerSettings.master, pawn);
		}

		public static void OpenMasterSelectMenu(Pawn p)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
			{
				p.playerSettings.master = null;
			}, MenuOptionPriority.Default, null, null, 0f, null, null));
			foreach (Pawn current in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				string text = RelationsUtility.LabelWithBondInfo(current, p);
				int level = current.skills.GetSkill(SkillDefOf.Animals).Level;
				int num = Mathf.RoundToInt(p.GetStatValue(StatDefOf.MinimumHandlingSkill, true));
				Action action;
				if (level >= num)
				{
					Pawn localCol = current;
					action = delegate
					{
						p.playerSettings.master = localCol;
					};
				}
				else
				{
					action = null;
					text = text + " (" + "SkillTooLow".Translate(new object[]
					{
						SkillDefOf.Animals.LabelCap,
						level,
						num
					}) + ")";
				}
				list.Add(new FloatMenuOption(text, action, MenuOptionPriority.Default, null, null, 0f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
