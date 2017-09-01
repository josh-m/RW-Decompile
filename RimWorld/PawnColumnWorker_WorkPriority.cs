using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class PawnColumnWorker_WorkPriority : PawnColumnWorker
	{
		private const int LabelRowHeight = 50;

		private Vector2 cachedWorkLabelSize;

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork)
			{
				return;
			}
			Text.Font = GameFont.Medium;
			float x = rect.x + (rect.width - 25f) / 2f;
			float y = rect.y + 2.5f;
			bool incapable = this.IsIncapableOfWholeWorkType(pawn, this.def.workType);
			WidgetsWork.DrawWorkBoxFor(x, y, pawn, this.def.workType, incapable);
			Rect rect2 = new Rect(x, y, 25f, 25f);
			TooltipHandler.TipRegion(rect2, () => WidgetsWork.TipForPawnWorker(pawn, this.def.workType, incapable), pawn.thingIDNumber ^ this.def.workType.GetHashCode());
			Text.Font = GameFont.Small;
		}

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			Text.Font = GameFont.Small;
			if (this.cachedWorkLabelSize == default(Vector2))
			{
				this.cachedWorkLabelSize = Text.CalcSize(this.def.workType.labelShort);
			}
			Rect labelRect = this.GetLabelRect(rect);
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(labelRect, this.def.workType.labelShort);
			GUI.color = new Color(1f, 1f, 1f, 0.3f);
			Widgets.DrawLineVertical(labelRect.center.x, labelRect.yMax - 3f, rect.y + 50f - labelRect.yMax + 3f);
			Widgets.DrawLineVertical(labelRect.center.x + 1f, labelRect.yMax - 3f, rect.y + 50f - labelRect.yMax + 3f);
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public override int GetMinHeaderHeight(PawnTable table)
		{
			return 50;
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), 32);
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(39, this.GetMinWidth(table), this.GetMaxWidth(table));
		}

		public override int GetMaxWidth(PawnTable table)
		{
			return Mathf.Min(base.GetMaxWidth(table), 80);
		}

		private bool IsIncapableOfWholeWorkType(Pawn p, WorkTypeDef work)
		{
			for (int i = 0; i < work.workGiversByPriority.Count; i++)
			{
				bool flag = true;
				for (int j = 0; j < work.workGiversByPriority[i].requiredCapacities.Count; j++)
				{
					PawnCapacityDef capacity = work.workGiversByPriority[i].requiredCapacities[j];
					if (!p.health.capacities.CapableOf(capacity))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		protected override Rect GetInteractableHeaderRect(Rect headerRect, PawnTable table)
		{
			return this.GetLabelRect(headerRect);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return this.GetValueToCompare(a).CompareTo(this.GetValueToCompare(b));
		}

		private float GetValueToCompare(Pawn pawn)
		{
			if (pawn.workSettings == null || !pawn.workSettings.EverWork)
			{
				return -2f;
			}
			if (pawn.story != null && pawn.story.WorkTypeIsDisabled(this.def.workType))
			{
				return -1f;
			}
			return pawn.skills.AverageOfRelevantSkillsFor(this.def.workType);
		}

		private Rect GetLabelRect(Rect headerRect)
		{
			float x = headerRect.center.x;
			Rect result = new Rect(x - this.cachedWorkLabelSize.x / 2f, headerRect.y, this.cachedWorkLabelSize.x, this.cachedWorkLabelSize.y);
			if (this.def.moveWorkTypeLabelDown)
			{
				result.y += 20f;
			}
			return result;
		}

		protected override string GetHeaderTip(PawnTable table)
		{
			string text = string.Concat(new string[]
			{
				this.def.workType.gerundLabel,
				"\n\n",
				this.def.workType.description,
				"\n\n",
				PawnColumnWorker_WorkPriority.SpecificWorkListString(this.def.workType)
			});
			text += "\n";
			if (this.def.sortable)
			{
				text = text + "\n" + "ClickToSortByThisColumn".Translate();
			}
			if (Find.PlaySettings.useWorkPriorities)
			{
				text = text + "\n" + "WorkPriorityShiftClickTip".Translate();
			}
			else
			{
				text = text + "\n" + "WorkPriorityShiftClickEnableDisableTip".Translate();
			}
			return text;
		}

		private static string SpecificWorkListString(WorkTypeDef def)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < def.workGiversByPriority.Count; i++)
			{
				stringBuilder.Append(def.workGiversByPriority[i].LabelCap);
				if (def.workGiversByPriority[i].emergency)
				{
					stringBuilder.Append(" (" + "EmergencyWorkMarker".Translate() + ")");
				}
				if (i < def.workGiversByPriority.Count - 1)
				{
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		protected override void HeaderClicked(Rect headerRect, PawnTable table)
		{
			base.HeaderClicked(headerRect, table);
			if (Event.current.shift)
			{
				List<Pawn> pawnsListForReading = table.PawnsListForReading;
				for (int i = 0; i < pawnsListForReading.Count; i++)
				{
					Pawn pawn = pawnsListForReading[i];
					if (pawn.workSettings != null && pawn.workSettings.EverWork)
					{
						if (pawn.story == null || !pawn.story.WorkTypeIsDisabled(this.def.workType))
						{
							if (Find.PlaySettings.useWorkPriorities)
							{
								int priority = pawn.workSettings.GetPriority(this.def.workType);
								if (Event.current.button == 0 && priority != 1)
								{
									int num = priority - 1;
									if (num < 0)
									{
										num = 4;
									}
									pawn.workSettings.SetPriority(this.def.workType, num);
								}
								if (Event.current.button == 1 && priority != 0)
								{
									int num2 = priority + 1;
									if (num2 > 4)
									{
										num2 = 0;
									}
									pawn.workSettings.SetPriority(this.def.workType, num2);
								}
							}
							else if (pawn.workSettings.GetPriority(this.def.workType) > 0)
							{
								if (Event.current.button == 1)
								{
									pawn.workSettings.SetPriority(this.def.workType, 0);
								}
							}
							else if (Event.current.button == 0)
							{
								pawn.workSettings.SetPriority(this.def.workType, 3);
							}
						}
					}
				}
				if (Find.PlaySettings.useWorkPriorities)
				{
					if (Event.current.button == 0)
					{
						SoundDefOf.AmountIncrement.PlayOneShotOnCamera(null);
					}
					else if (Event.current.button == 1)
					{
						SoundDefOf.AmountDecrement.PlayOneShotOnCamera(null);
					}
				}
				else if (Event.current.button == 0)
				{
					SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera(null);
				}
				else if (Event.current.button == 1)
				{
					SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera(null);
				}
			}
		}
	}
}
