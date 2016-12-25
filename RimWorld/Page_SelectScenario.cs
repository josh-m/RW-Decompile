using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Page_SelectScenario : Page
	{
		private const float ScenarioEntryHeight = 62f;

		private Scenario curScen;

		private Vector2 infoScrollPosition = Vector2.zero;

		private static readonly Texture2D CanUploadIcon = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/CanUpload", true);

		private Vector2 scenariosScrollPosition = Vector2.zero;

		private float totalScenarioListHeight;

		public override string PageTitle
		{
			get
			{
				return "ChooseScenario".Translate();
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();
			this.infoScrollPosition = Vector2.zero;
			ScenarioLister.MarkDirty();
			this.EnsureValidSelection();
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DrawPageTitle(rect);
			Rect mainRect = base.GetMainRect(rect, 0f, false);
			GUI.BeginGroup(mainRect);
			Rect rect2 = new Rect(0f, 0f, mainRect.width * 0.35f, mainRect.height).Rounded();
			this.DoScenarioSelectionList(rect2);
			Rect rect3 = new Rect(rect2.xMax + 17f, 0f, mainRect.width - rect2.width - 17f, mainRect.height).Rounded();
			ScenarioUI.DrawScenarioInfo(rect3, this.curScen, ref this.infoScrollPosition);
			GUI.EndGroup();
			string midLabel = "ScenarioEditor".Translate();
			base.DoBottomButtons(rect, null, midLabel, new Action(this.GoToScenarioEditor), true);
		}

		private bool CanEditScenario(Scenario scen)
		{
			return scen.Category == ScenarioCategory.CustomLocal || scen.CanToUploadToWorkshop();
		}

		private void GoToScenarioEditor()
		{
			Scenario scen = (!this.CanEditScenario(this.curScen)) ? this.curScen.CopyForEditing() : this.curScen;
			Page_ScenarioEditor page_ScenarioEditor = new Page_ScenarioEditor(scen);
			page_ScenarioEditor.prev = this;
			Find.WindowStack.Add(page_ScenarioEditor);
			this.Close(true);
		}

		private void DoScenarioSelectionList(Rect rect)
		{
			rect.xMax += 2f;
			Rect rect2 = new Rect(0f, 0f, rect.width - 16f - 2f, this.totalScenarioListHeight + 250f);
			Widgets.BeginScrollView(rect, ref this.scenariosScrollPosition, rect2);
			Rect rect3 = rect2.AtZero();
			rect3.height = 999999f;
			Listing_Standard listing_Standard = new Listing_Standard(rect3);
			listing_Standard.ColumnWidth = rect2.width;
			Text.Font = GameFont.Small;
			this.ListScenariosOnListing(listing_Standard, ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef));
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Small;
			listing_Standard.Label("ScenariosCustom".Translate());
			this.ListScenariosOnListing(listing_Standard, ScenarioLister.ScenariosInCategory(ScenarioCategory.CustomLocal));
			listing_Standard.Gap(12f);
			Text.Font = GameFont.Small;
			listing_Standard.Label("ScenariosSteamWorkshop".Translate());
			if (listing_Standard.ButtonText("OpenSteamWorkshop".Translate(), null))
			{
				SteamUtility.OpenSteamWorkshopPage();
			}
			this.ListScenariosOnListing(listing_Standard, ScenarioLister.ScenariosInCategory(ScenarioCategory.SteamWorkshop));
			listing_Standard.End();
			this.totalScenarioListHeight = listing_Standard.CurHeight;
			Widgets.EndScrollView();
		}

		private void ListScenariosOnListing(Listing_Standard listing, IEnumerable<Scenario> scenarios)
		{
			bool flag = false;
			foreach (Scenario current in scenarios)
			{
				if (flag)
				{
					listing.Gap(12f);
				}
				Scenario scen = current;
				Rect rect = listing.GetRect(62f);
				this.DoScenarioListEntry(rect, scen);
				flag = true;
			}
			if (!flag)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				listing.Label("(" + "NoneLower".Translate() + ")");
				GUI.color = Color.white;
			}
		}

		private void DoScenarioListEntry(Rect rect, Scenario scen)
		{
			bool flag = this.curScen == scen;
			Color color = flag ? new Color(0.32f, 0.28f, 0.21f) : new Color(0.21f, 0.21f, 0.21f);
			Widgets.DrawBoxSolid(rect, color);
			GUI.color = color * 1.8f;
			Widgets.DrawBox(rect, 1);
			GUI.color = Color.white;
			Widgets.DrawHighlightIfMouseover(rect);
			MouseoverSounds.DoRegion(rect);
			Rect rect2 = rect.ContractedBy(4f);
			Text.Font = GameFont.Small;
			Rect rect3 = rect2;
			rect3.height = Text.CalcHeight(scen.name, rect3.width);
			Widgets.Label(rect3, scen.name);
			Text.Font = GameFont.Tiny;
			Rect rect4 = rect2;
			rect4.yMin = rect3.yMax;
			Widgets.Label(rect4, scen.GetSummary());
			if (scen.enabled)
			{
				WidgetRow widgetRow = new WidgetRow(rect.xMax, rect.y, UIDirection.LeftThenDown, 99999f, 4f);
				if (scen.Category == ScenarioCategory.CustomLocal && widgetRow.ButtonIcon(TexButton.DeleteX, "Delete".Translate()))
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDelete".Translate(new object[]
					{
						scen.File.Name
					}), delegate
					{
						scen.File.Delete();
						ScenarioLister.MarkDirty();
					}, true, null));
				}
				if (scen.Category == ScenarioCategory.SteamWorkshop && widgetRow.ButtonIcon(TexButton.DeleteX, "Unsubscribe".Translate()))
				{
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnsubscribe".Translate(new object[]
					{
						scen.File.Name
					}), delegate
					{
						scen.enabled = false;
						if (this.curScen == scen)
						{
							this.curScen = null;
							this.EnsureValidSelection();
						}
						Workshop.Unsubscribe(scen);
					}, true, null));
				}
				if (scen.GetPublishedFileId() != PublishedFileId_t.Invalid)
				{
					if (widgetRow.ButtonIcon(ContentSource.SteamWorkshop.GetIcon(), "WorkshopPage".Translate()))
					{
						SteamUtility.OpenWorkshopPage(scen.GetPublishedFileId());
					}
					if (scen.CanToUploadToWorkshop())
					{
						widgetRow.Icon(Page_SelectScenario.CanUploadIcon, "CanBeUpdatedOnWorkshop".Translate());
					}
				}
				if (!flag && Widgets.ButtonInvisible(rect, false))
				{
					this.curScen = scen;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			if (this.curScen == null)
			{
				return false;
			}
			Page_SelectScenario.BeginScenarioConfiguration(this.curScen, this);
			return true;
		}

		public static void BeginScenarioConfiguration(Scenario scen, Page originPage)
		{
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = scen;
			Current.Game.Scenario.PreConfigure();
			Page firstConfigPage = Current.Game.Scenario.GetFirstConfigPage();
			if (firstConfigPage == null)
			{
				PageUtility.InitGameStart();
				return;
			}
			originPage.next = firstConfigPage;
			firstConfigPage.prev = originPage;
		}

		private void EnsureValidSelection()
		{
			if (this.curScen == null || !ScenarioLister.ScenarioIsListedAnywhere(this.curScen))
			{
				this.curScen = ScenarioLister.ScenariosInCategory(ScenarioCategory.FromDef).FirstOrDefault<Scenario>();
			}
		}

		internal void Notify_ScenarioListChanged()
		{
			PublishedFileId_t selModId = this.curScen.GetPublishedFileId();
			this.curScen = ScenarioLister.AllScenarios().FirstOrDefault((Scenario sc) => sc.GetPublishedFileId() == selModId);
			this.EnsureValidSelection();
		}

		internal void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid)
		{
			if (this.curScen != null && this.curScen.GetPublishedFileId() == pfid)
			{
				this.curScen = null;
			}
			this.EnsureValidSelection();
		}
	}
}
