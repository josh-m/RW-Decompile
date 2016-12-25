using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Page_SelectLandingSite : Page
	{
		private const float GapBetweenBottomButtons = 10f;

		private const float UseTwoRowsIfScreenWidthBelow = 1180f;

		public override string PageTitle
		{
			get
			{
				return "SelectLandingSite".Translate();
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return Vector2.zero;
			}
		}

		protected override float Margin
		{
			get
			{
				return 0f;
			}
		}

		public Page_SelectLandingSite()
		{
			this.wantsRenderedWorld = true;
			this.absorbInputAroundWindow = false;
			this.shadowAlpha = 0f;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			Find.World.UI.Reset();
			((MainTabWindow_World)MainTabDefOf.World.Window).everOpened = false;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			Find.GameInitData.ChooseRandomStartingTile();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.WorldCameraMovement, OpportunityType.Important);
			TutorSystem.Notify_Event("PageStart-SelectLandingSite");
		}

		public override void DoWindowContents(Rect rect)
		{
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			Text.Anchor = TextAnchor.UpperCenter;
			base.DrawPageTitle(new Rect(0f, 5f, (float)UI.screenWidth, 300f));
			Text.Anchor = TextAnchor.UpperLeft;
			this.DoCustomBottomButtons();
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			int selectedTile = Find.World.UI.SelectedTile;
			if (selectedTile < 0)
			{
				Messages.Message("MustSelectLandingSite".Translate(), MessageSound.RejectInput);
				return false;
			}
			Tile tile = Find.WorldGrid[selectedTile];
			if (!tile.biome.canBuildBase)
			{
				Messages.Message("CannotLandBiome".Translate(new object[]
				{
					tile.biome.label
				}), MessageSound.RejectInput);
				return false;
			}
			if (!tile.biome.implemented)
			{
				Messages.Message("BiomeNotImplemented".Translate() + ": " + tile.biome.label, MessageSound.RejectInput);
				return false;
			}
			if (tile.hilliness == Hilliness.Impassable)
			{
				Messages.Message("CannotLandImpassableMountains".Translate(), MessageSound.RejectInput);
				return false;
			}
			Faction faction = Find.World.factionManager.FactionAtTile(selectedTile);
			if (faction != null)
			{
				Messages.Message("BaseAlreadyThere".Translate(new object[]
				{
					faction.Name
				}), MessageSound.RejectInput);
				return false;
			}
			if (!TutorSystem.AllowAction("ChooseBiome-" + tile.biome.defName + "-" + tile.hilliness.ToString()))
			{
				return false;
			}
			Find.GameInitData.startingTile = selectedTile;
			return true;
		}

		private void DoCustomBottomButtons()
		{
			int num = (!TutorSystem.TutorialMode) ? 4 : 3;
			int num2;
			if (num >= 4 && (float)UI.screenWidth < 1180f)
			{
				num2 = 2;
			}
			else
			{
				num2 = 1;
			}
			int num3 = Mathf.CeilToInt((float)num / (float)num2);
			float num4 = Page.BottomButSize.x * (float)num3 + 10f * (float)(num3 + 1);
			float num5 = (float)num2 * Page.BottomButSize.y + 10f * (float)(num2 + 1);
			Rect rect = new Rect(((float)UI.screenWidth - num4) / 2f, (float)UI.screenHeight - num5 - 4f, num4, num5);
			if (Find.WindowStack.IsOpen<WorldInspectPane>() && rect.x < InspectPaneUtility.PaneSize.x + 4f)
			{
				rect.x = InspectPaneUtility.PaneSize.x + 4f;
			}
			Widgets.DrawWindowBackground(rect);
			float num6 = rect.xMin + 10f;
			float num7 = rect.yMin + 10f;
			Text.Font = GameFont.Small;
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "Back".Translate(), true, false, true) && this.CanDoBack())
			{
				this.DoBack();
			}
			num6 += Page.BottomButSize.x + 10f;
			if (!TutorSystem.TutorialMode)
			{
				if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "Advanced".Translate(), true, false, true))
				{
					Find.WindowStack.Add(new Dialog_AdvancedGameConfig(Find.World.UI.SelectedTile));
				}
				num6 += Page.BottomButSize.x + 10f;
			}
			if (num2 == 2)
			{
				num6 = rect.xMin + 10f;
				num7 += Page.BottomButSize.y + 10f;
			}
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "SelectRandomSite".Translate(), true, false, true))
			{
				SoundDefOf.Click.PlayOneShotOnCamera();
				Find.World.UI.SelectedTile = TileFinder.RandomStartingTile();
				Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(Find.World.UI.SelectedTile));
			}
			num6 += Page.BottomButSize.x + 10f;
			if (Widgets.ButtonText(new Rect(num6, num7, Page.BottomButSize.x, Page.BottomButSize.y), "Next".Translate(), true, false, true) && this.CanDoNext())
			{
				this.DoNext();
			}
			num6 += Page.BottomButSize.x + 10f;
			GenUI.AbsorbClicksInRect(rect);
		}
	}
}
