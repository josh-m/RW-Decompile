using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TrainingCardUtility
	{
		private const float RowHeight = 28f;

		private const float InfoHeaderHeight = 50f;

		private static readonly Texture2D TrainedTrainableTex = ContentFinder<Texture2D>.Get("UI/Icons/FixedCheck", true);

		public static void DrawTrainingCard(Rect rect, Pawn pawn)
		{
			GUI.BeginGroup(rect);
			string label = "TrainableIntelligence".Translate() + ": " + pawn.RaceProps.trainableIntelligence.GetLabel();
			Widgets.Label(new Rect(0f, 0f, rect.width, 25f), label);
			if (pawn.training.IsCompleted(TrainableDefOf.Obedience))
			{
				Rect rect2 = new Rect(0f, 20f, rect.width, 25f);
				Widgets.Label(rect2, "Master".Translate() + ": ");
				rect2.xMin = rect2.center.x;
				string label2 = TrainableUtility.MasterString(pawn);
				if (Widgets.ButtonText(rect2, label2, true, false, true))
				{
					TrainableUtility.OpenMasterSelectMenu(pawn);
				}
			}
			float num = 50f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				if (TrainingCardUtility.TryDrawTrainableRow(new Rect(0f, num, rect.width, 28f), pawn, trainableDefsInListOrder[i]))
				{
					num += 28f;
				}
			}
			GUI.EndGroup();
		}

		private static bool TryDrawTrainableRow(Rect rect, Pawn pawn, TrainableDef td)
		{
			bool flag = pawn.training.IsCompleted(td);
			bool flag2;
			AcceptanceReport canTrain = pawn.training.CanAssignToTrain(td, out flag2);
			if (!flag2)
			{
				return false;
			}
			Widgets.DrawHighlightIfMouseover(rect);
			Rect rect2 = rect;
			rect2.width -= 50f;
			rect2.xMin += (float)td.indent * 10f;
			Rect rect3 = rect;
			rect3.xMin = rect3.xMax - 50f + 17f;
			if (!flag)
			{
				TrainingCardUtility.DoTrainableCheckbox(rect2, pawn, td, canTrain, true, false);
			}
			else
			{
				Text.Anchor = TextAnchor.MiddleLeft;
				Widgets.Label(rect2, td.LabelCap);
				Text.Anchor = TextAnchor.UpperLeft;
			}
			if (flag)
			{
				GUI.color = Color.green;
			}
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect3, pawn.training.GetSteps(td) + " / " + td.steps);
			Text.Anchor = TextAnchor.UpperLeft;
			if (DebugSettings.godMode && !pawn.training.IsCompleted(td))
			{
				Rect rect4 = rect3;
				rect4.yMin = rect4.yMax - 10f;
				rect4.xMin = rect4.xMax - 10f;
				if (Widgets.ButtonText(rect4, "+", true, false, true))
				{
					pawn.training.Train(td, pawn.Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>());
				}
			}
			TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
			GUI.color = Color.white;
			return true;
		}

		public static void DoTrainableCheckbox(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain, bool drawLabel, bool doTooltip)
		{
			bool flag = pawn.training.IsCompleted(td);
			if (flag)
			{
				if (!drawLabel)
				{
					GUI.DrawTexture(rect, TrainingCardUtility.TrainedTrainableTex);
				}
			}
			else
			{
				bool wanted = pawn.training.GetWanted(td);
				bool flag2 = wanted;
				if (drawLabel)
				{
					Widgets.CheckboxLabeled(rect, td.LabelCap, ref wanted, !canTrain.Accepted);
				}
				else
				{
					Widgets.Checkbox(rect.position, ref wanted, rect.width, !canTrain.Accepted);
				}
				if (wanted != flag2)
				{
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnimalTraining, KnowledgeAmount.Total);
					TrainingCardUtility.SetWantedRecursive(td, pawn, wanted);
				}
			}
			if (doTooltip)
			{
				TrainingCardUtility.DoTrainableTooltip(rect, pawn, td, canTrain);
			}
		}

		private static void DoTrainableTooltip(Rect rect, Pawn pawn, TrainableDef td, AcceptanceReport canTrain)
		{
			TooltipHandler.TipRegion(rect, delegate
			{
				string text = td.LabelCap + "\n\n" + td.description;
				if (!canTrain.Accepted)
				{
					text = text + "\n\n" + canTrain.Reason;
				}
				else if (!td.prerequisites.NullOrEmpty<TrainableDef>())
				{
					text += "\n";
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						if (!pawn.training.IsCompleted(td.prerequisites[i]))
						{
							text = text + "\n" + "TrainingNeedsPrerequisite".Translate(new object[]
							{
								td.prerequisites[i].LabelCap
							});
						}
					}
				}
				return text;
			}, (int)(rect.y * 612f + rect.x));
		}

		private static void SetWantedRecursive(TrainableDef td, Pawn pawn, bool checkOn)
		{
			pawn.training.SetWanted(td, checkOn);
			if (checkOn)
			{
				if (td.prerequisites != null)
				{
					for (int i = 0; i < td.prerequisites.Count; i++)
					{
						TrainingCardUtility.SetWantedRecursive(td.prerequisites[i], pawn, true);
					}
				}
			}
			else
			{
				IEnumerable<TrainableDef> enumerable = from t in DefDatabase<TrainableDef>.AllDefsListForReading
				where t.prerequisites != null && t.prerequisites.Contains(td)
				select t;
				foreach (TrainableDef current in enumerable)
				{
					TrainingCardUtility.SetWantedRecursive(current, pawn, false);
				}
			}
		}
	}
}
