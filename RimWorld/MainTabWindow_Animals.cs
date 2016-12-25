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

		private const float SpaceBetweenIcons = 2f;

		private const float IconSize = 24f;

		private const float SpaceBetweenColumnCategories = 6f;

		private static readonly Texture2D FollowDraftedIcon = Resources.Load<Texture2D>("Textures/UI/Icons/Animal/FollowDrafted");

		private static readonly Texture2D FollowFieldworkIcon = Resources.Load<Texture2D>("Textures/UI/Icons/Animal/FollowFieldwork");

		private static readonly Texture2D PregnantIcon = Resources.Load<Texture2D>("Textures/UI/Icons/Pregnant");

		private static readonly Texture2D SlaughterIcon = Resources.Load<Texture2D>("Textures/UI/Icons/Animal/Slaughter");

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(1010f, 65f + (float)base.PawnsCount * 30f + 65f);
			}
		}

		protected override void BuildPawnList()
		{
			this.pawns = (from p in Find.VisibleMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
			where p.RaceProps.Animal
			orderby p.RaceProps.petness descending, p.RaceProps.baseBodySize, p.def.label
			select p).ToList<Pawn>();
		}

		public override void DoWindowContents(Rect fillRect)
		{
			base.DoWindowContents(fillRect);
			Rect position = new Rect(0f, 0f, fillRect.width, 65f);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerCenter;
			float num = 165f;
			num += 6f;
			num += 50f;
			num += 6f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				Rect rect = new Rect(num, position.height - 24f, 24f, 24f);
				GUI.DrawTexture(rect, trainableDefsInListOrder[i].Icon);
				TooltipHandler.TipRegion(rect, trainableDefsInListOrder[i].LabelCap);
				num += 24f;
				if (i < trainableDefsInListOrder.Count - 1)
				{
					num += 2f;
				}
			}
			num += 6f;
			Rect rect2 = new Rect(num, position.height - 24f, 24f, 24f);
			GUI.DrawTexture(rect2, MainTabWindow_Animals.FollowDraftedIcon);
			TooltipHandler.TipRegion(rect2, "FollowDraftedTip".Translate());
			num += 24f;
			num += 2f;
			Rect rect3 = new Rect(num, position.height - 24f, 24f, 24f);
			GUI.DrawTexture(rect3, MainTabWindow_Animals.FollowFieldworkIcon);
			TooltipHandler.TipRegion(rect3, "FollowFieldworkTip".Translate());
			num += 24f;
			num += 6f;
			Rect rect4 = new Rect(num, 0f, 170f, position.height + 3f);
			Widgets.Label(rect4, "Master".Translate());
			num += 170f;
			num += 6f;
			Rect rect5 = new Rect(num, position.height - 24f, 24f, 24f);
			GUI.DrawTexture(rect5, MainTabWindow_Animals.SlaughterIcon);
			TooltipHandler.TipRegion(rect5, "DesignatorSlaughter".Translate());
			num += 24f;
			num += 6f;
			Rect rect6 = new Rect(num, 0f, 350f, Mathf.Round(position.height / 2f));
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(rect6, "ManageAreas".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Dialog_ManageAreas(Find.VisibleMap));
			}
			Text.Font = GameFont.Tiny;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect7 = new Rect(num, 0f, 350f, position.height + 3f);
			Widgets.Label(rect7, "AllowedArea".Translate());
			num += 350f;
			GUI.EndGroup();
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			Rect rect8 = new Rect(0f, position.height, fillRect.width, fillRect.height - position.height);
			base.DrawRows(rect8);
		}

		protected override void DrawPawnRow(Rect rect, Pawn p)
		{
			GUI.BeginGroup(rect);
			float num = 165f;
			num += 6f;
			this.DoIcons(rect, p, ref num);
			num += 6f;
			this.DoTrainingCheckboxes(rect, p, ref num);
			num += 6f;
			this.DoFollowCheckboxes(rect, p, ref num);
			num += 6f;
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
			num += 6f;
			if (p.MapHeld != null)
			{
				this.DoSlaughterCheckbox(rect, p, ref num);
			}
			num += 6f;
			Rect rect4 = new Rect(num, 0f, 350f, rect.height);
			AreaAllowedGUI.DoAllowedAreaSelectors(rect4, p, AllowedAreaMode.Animal);
			num += 350f;
			num += 6f;
			this.DoExtraIcons(rect, p, ref num);
			GUI.EndGroup();
		}

		private void DoSlaughterCheckbox(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			Vector2 topLeft = new Vector2(curX, y);
			Designation designation = p.MapHeld.designationManager.DesignationOn(p, DesignationDefOf.Slaughter);
			bool flag = designation != null;
			bool flag2 = flag;
			Widgets.Checkbox(topLeft, ref flag, 24f, false);
			TooltipHandler.TipRegion(new Rect(topLeft.x, topLeft.y, 24f, 24f), "DesignatorSlaughterDesc".Translate());
			if (flag2 != flag)
			{
				if (flag)
				{
					p.MapHeld.designationManager.AddDesignation(new Designation(p, DesignationDefOf.Slaughter));
					SlaughterDesignatorUtility.CheckWarnAboutBondedAnimal(p);
				}
				else
				{
					p.MapHeld.designationManager.RemoveDesignation(designation);
				}
			}
			curX += 24f;
		}

		private void DoTrainingCheckboxes(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			List<TrainableDef> trainableDefsInListOrder = TrainableUtility.TrainableDefsInListOrder;
			for (int i = 0; i < trainableDefsInListOrder.Count; i++)
			{
				TrainableDef td = trainableDefsInListOrder[i];
				float x = curX;
				curX += 24f;
				if (i < trainableDefsInListOrder.Count - 1)
				{
					curX += 2f;
				}
				bool flag;
				AcceptanceReport canTrain = p.training.CanAssignToTrain(td, out flag);
				if (flag && canTrain.Accepted)
				{
					Rect rect2 = new Rect(x, y, 24f, 24f);
					TrainingCardUtility.DoTrainableCheckbox(rect2, p, td, canTrain, false, true);
				}
			}
		}

		private void DoFollowCheckboxes(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			if (p.training.IsCompleted(TrainableDefOf.Obedience))
			{
				Widgets.Checkbox(curX, y, ref p.playerSettings.followDrafted, 24f, false);
			}
			curX += 24f;
			curX += 2f;
			if (p.training.IsCompleted(TrainableDefOf.Obedience))
			{
				Widgets.Checkbox(curX, y, ref p.playerSettings.followFieldwork, 24f, false);
			}
			curX += 24f;
		}

		private void DoIcons(Rect rect, Pawn p, ref float curX)
		{
			float y = (rect.height - 24f) / 2f;
			Rect rect2 = new Rect(curX, y, 24f, 24f);
			GUI.DrawTexture(rect2, p.gender.GetIcon());
			TooltipHandler.TipRegion(rect2, p.gender.GetLabel().CapitalizeFirst());
			curX += 24f;
			curX += 2f;
			Rect rect3 = new Rect(curX, y, 24f, 24f);
			GUI.DrawTexture(rect3, p.ageTracker.CurLifeStageRace.GetIcon(p));
			TooltipHandler.TipRegion(rect3, p.ageTracker.CurLifeStage.LabelCap);
			curX += 24f;
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
		}
	}
}
