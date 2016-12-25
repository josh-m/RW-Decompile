using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Animals : MainTabWindow_PawnList
	{
		private const float TopAreaHeight = 65f;

		private const float MasterWidth = 170f;

		private const float AreaAllowedWidth = 350f;

		private const float PaddingBeforeIcons = 5f;

		private const float PaddingBeforeExtraIcons = 5f;

		private const float SpaceBetweenIcons = 2f;

		private const float IconSize = 24f;

		private const float SpaceBetweenColumnCategories = 7f;

		private static readonly Texture2D PregnantIcon = Resources.Load<Texture2D>("Textures/UI/Icons/Pregnant");

		private static readonly Texture2D SlaughterIcon = Resources.Load<Texture2D>("Textures/UI/Icons/Slaughter");

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(1010f, 65f + (float)base.PawnsCount * 30f + 65f);
			}
		}

		protected override void BuildPawnList()
		{
			this.pawns = (from p in Find.MapPawns.PawnsInFaction(Faction.OfPlayer)
			where p.RaceProps.Animal
			orderby p.RaceProps.petness descending, p.RaceProps.baseBodySize, p.def.label
			select p).ToList<Pawn>();
		}

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			Rect position = new Rect(0f, 0f, fillRect.width, 65f);
			GUI.BeginGroup(position);
			float num = 165f;
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerCenter;
			num += 5f;
			num += 52f;
			num += 7f;
			Rect rect = new Rect(num, position.height - 24f, 24f, 24f);
			GUI.DrawTexture(rect, MainTabWindow_Animals.SlaughterIcon);
			TooltipHandler.TipRegion(rect, "DesignatorSlaughter".Translate());
			num += 33f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				Rect rect2 = new Rect(num, position.height - 24f, 24f, 24f);
				GUI.DrawTexture(rect2, trainableDefsInListOrder[i].Icon);
				TooltipHandler.TipRegion(rect2, trainableDefsInListOrder[i].LabelCap);
				num += 26f;
			}
			Rect rect3 = new Rect(num, 0f, 170f, position.height + 3f);
			Widgets.Label(rect3, "Master".Translate());
			num += 170f;
			Rect rect4 = new Rect(num, 0f, 350f, Mathf.Round(position.height / 2f));
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(rect4, "ManageAreas".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageAreas());
			}
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect5 = new Rect(num, 0f, 350f, position.height + 3f);
			Widgets.Label(rect5, "AllowedArea".Translate());
			num += 350f;
			GUI.EndGroup();
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			Rect rect6 = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
			base.DrawRows(rect6);
		}

		protected override void DrawPawnRow(Rect rect, Pawn p)
		{
			GUI.BeginGroup(rect);
			float num = 165f;
			num += 5f;
			this.DoIcons(rect, p, ref num);
			this.DoSlaughterCheckbox(rect, p, ref num);
			this.DoTrainingCheckboxes(rect, p, ref num);
			if (p.training.IsCompleted(TrainableDefOf.Obedience))
			{
				Rect rect2 = new Rect(num, 0f, 170f, rect.height);
				Rect rect3 = rect2.ContractedBy(2f);
				string label = TrainableUtility.MasterString(p);
				Text.Font = GameFont.Small;
				if (Widgets.ButtonText(rect3, label, true, false, true))
				{
					TrainableUtility.OpenMasterSelectMenu(p);
				}
			}
			num += 170f;
			Rect rect4 = new Rect(num, 0f, 350f, rect.height);
			AreaAllowedGUI.DoAllowedAreaSelectors(rect4, p, AllowedAreaMode.Animal);
			num += 350f;
			num += 5f;
			this.DoExtraIcons(rect, p, ref num);
			GUI.EndGroup();
		}

		private void DoSlaughterCheckbox(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			Vector2 topLeft = new Vector2(curX, y);
			Designation designation = Find.DesignationManager.DesignationOn(p, DesignationDefOf.Slaughter);
			bool flag = designation != null;
			bool flag2 = flag;
			Widgets.Checkbox(topLeft, ref flag, 24f, false);
			TooltipHandler.TipRegion(new Rect(topLeft.x, topLeft.y, 24f, 24f), "DesignatorSlaughterDesc".Translate());
			if (flag2 != flag)
			{
				if (flag)
				{
					Find.DesignationManager.AddDesignation(new Designation(p, DesignationDefOf.Slaughter));
				}
				else
				{
					Find.DesignationManager.RemoveDesignation(designation);
				}
			}
			curX += 26f;
			curX += 7f;
		}

		private void DoTrainingCheckboxes(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				TrainableDef td = trainableDefsInListOrder[i];
				Vector2 vector = new Vector2(curX, y);
				curX += 26f;
				bool flag;
				AcceptanceReport canTrain = p.training.CanAssignToTrain(td, out flag);
				if (flag && canTrain.Accepted)
				{
					Rect rect2 = new Rect(vector.x, vector.y, 24f, 24f);
					TrainingCardUtility.DoTrainableCheckbox(rect2, p, td, canTrain, false, true);
				}
			}
			curX += 7f;
		}

		private void DoIcons(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			Rect rect2 = new Rect(curX, y, 24f, 24f);
			GUI.DrawTexture(rect2, p.gender.GetIcon());
			TooltipHandler.TipRegion(rect2, p.gender.GetLabel().CapitalizeFirst());
			curX += 26f;
			Rect rect3 = new Rect(curX, y, 24f, 24f);
			GUI.DrawTexture(rect3, p.ageTracker.CurLifeStageRace.GetIcon(p));
			TooltipHandler.TipRegion(rect3, p.ageTracker.CurLifeStage.LabelCap);
			curX += 26f;
			curX += 7f;
		}

		private void DoExtraIcons(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)p.health.hediffSet.hediffs.Find((Hediff x) => x.def == HediffDefOf.Pregnant && x.Visible);
			if (hediff_Pregnant != null)
			{
				Rect rect2 = new Rect(curX, y, 24f, 24f);
				GUI.DrawTexture(rect2, MainTabWindow_Animals.PregnantIcon);
				float gestationProgress = hediff_Pregnant.GestationProgress;
				int num = (int)(p.RaceProps.gestationPeriodDays * 60000f);
				int numTicks = (int)(gestationProgress * (float)num);
				TooltipHandler.TipRegion(rect2, new TipSignal("PregnantIconDesc".Translate(new object[]
				{
					numTicks.ToStringTicksToPeriod(true),
					num.ToStringTicksToPeriod(true)
				}), rect2.GetHashCode()));
				curX += 26f;
			}
			curX += 7f;
		}
	}
}
