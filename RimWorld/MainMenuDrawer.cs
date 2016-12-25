using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class MainMenuDrawer
	{
		private const float PlayRectWidth = 170f;

		private const float WebRectWidth = 145f;

		private const float RightEdgeMargin = 50f;

		private const float TitleShift = 50f;

		private static bool anyMapFiles;

		private static readonly Vector2 PaneSize = new Vector2(450f, 450f);

		private static readonly Vector2 TitleSize = new Vector2(1032f, 146f);

		private static readonly Texture2D TexTitle = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle", true);

		private static readonly Vector2 LudeonLogoSize = new Vector2(200f, 58f);

		private static readonly Texture2D TexLudeonLogo = ContentFinder<Texture2D>.Get("UI/HeroArt/LudeonLogoSmall", true);

		public static void Init()
		{
			PlayerKnowledgeDatabase.Save();
			ShipCountdown.CancelCountdown();
			MainMenuDrawer.anyMapFiles = GenFilePaths.AllSavedGameFiles.Any<FileInfo>();
		}

		public static void MainMenuOnGUI()
		{
			VersionControl.DrawInfoInCorner();
			Rect rect = new Rect((float)(Screen.width / 2) - MainMenuDrawer.PaneSize.x / 2f, (float)(Screen.height / 2) - MainMenuDrawer.PaneSize.y / 2f + 50f, MainMenuDrawer.PaneSize.x, MainMenuDrawer.PaneSize.y);
			rect.x = (float)Screen.width - rect.width - 30f;
			Rect rect2 = new Rect(0f, rect.y - 30f, (float)Screen.width - 85f, 30f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.UpperRight;
			string text = "MainPageCredit".Translate();
			if (Screen.width < 990)
			{
				Rect position = rect2;
				position.xMin = position.xMax - Text.CalcSize(text).x;
				position.xMin -= 4f;
				position.xMax += 4f;
				GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
				GUI.DrawTexture(position, BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			Widgets.Label(rect2, text);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
			Vector2 a = MainMenuDrawer.TitleSize;
			if (a.x > (float)Screen.width)
			{
				a *= (float)Screen.width / a.x;
			}
			a *= 0.7f;
			Rect position2 = new Rect((float)Screen.width - a.x - 50f, rect2.y - a.y, a.x, a.y);
			GUI.DrawTexture(position2, MainMenuDrawer.TexTitle, ScaleMode.StretchToFill, true);
			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Rect position3 = new Rect((float)(Screen.width - 8) - MainMenuDrawer.LudeonLogoSize.x, 8f, MainMenuDrawer.LudeonLogoSize.x, MainMenuDrawer.LudeonLogoSize.y);
			GUI.DrawTexture(position3, MainMenuDrawer.TexLudeonLogo, ScaleMode.StretchToFill, true);
			GUI.color = Color.white;
			rect.yMin += 17f;
			MainMenuDrawer.DoMainMenuControls(rect, MainMenuDrawer.anyMapFiles);
		}

		public static void DoMainMenuControls(Rect rect, bool anyMapFiles)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 0f, 170f, rect.height);
			Rect rect3 = new Rect(rect2.xMax + 17f, 0f, 145f, rect.height);
			Text.Font = GameFont.Small;
			List<ListableOption> list = new List<ListableOption>();
			if (Current.ProgramState == ProgramState.Entry)
			{
				string label;
				if (!"Tutorial".CanTranslate())
				{
					label = "LearnToPlay".Translate();
				}
				else
				{
					label = "Tutorial".Translate();
				}
				list.Add(new ListableOption(label, delegate
				{
					MainMenuDrawer.InitLearnToPlay();
				}, null));
				list.Add(new ListableOption("NewColony".Translate(), delegate
				{
					Find.WindowStack.Add(new Page_SelectScenario());
				}, null));
			}
			if (Current.ProgramState == ProgramState.MapPlaying && !Current.Game.Info.permadeathMode)
			{
				list.Add(new ListableOption("Save".Translate(), delegate
				{
					MainMenuDrawer.CloseMainTab();
					Find.WindowStack.Add(new Dialog_MapList_Save());
				}, null));
			}
			ListableOption item;
			if (anyMapFiles && (Current.ProgramState != ProgramState.MapPlaying || !Current.Game.Info.permadeathMode))
			{
				item = new ListableOption("LoadGame".Translate(), delegate
				{
					MainMenuDrawer.CloseMainTab();
					Find.WindowStack.Add(new Dialog_MapList_Load());
				}, null);
				list.Add(item);
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				list.Add(new ListableOption("ReviewScenario".Translate(), delegate
				{
					Find.WindowStack.Add(new Dialog_Message(Find.Scenario.GetFullInformationText(), Find.Scenario.name));
				}, null));
			}
			item = new ListableOption("Options".Translate(), delegate
			{
				MainMenuDrawer.CloseMainTab();
				Find.WindowStack.Add(new Dialog_Options());
			}, "MenuButton-Options");
			list.Add(item);
			if (Current.ProgramState == ProgramState.Entry)
			{
				item = new ListableOption("Mods".Translate(), delegate
				{
					Find.WindowStack.Add(new Page_ModsConfig());
				}, null);
				list.Add(item);
				item = new ListableOption("Credits".Translate(), delegate
				{
					Find.WindowStack.Add(new Screen_Credits());
				}, null);
				list.Add(item);
			}
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				if (Current.Game.Info.permadeathMode)
				{
					item = new ListableOption("SaveAndQuitToMainMenu".Translate(), delegate
					{
						LongEventHandler.QueueLongEvent(delegate
						{
							GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
						}, "Entry", "SavingLongEvent", false, null);
					}, null);
					list.Add(item);
					item = new ListableOption("SaveAndQuitToOS".Translate(), delegate
					{
						LongEventHandler.QueueLongEvent(delegate
						{
							GameDataSaveLoader.SaveGame(Current.Game.Info.permadeathModeUniqueName);
							LongEventHandler.ExecuteWhenFinished(delegate
							{
								Root.Shutdown();
							});
						}, "SavingLongEvent", false, null);
					}, null);
					list.Add(item);
				}
				else
				{
					Action action = delegate
					{
						if (GameDataSaveLoader.CurrentMapStateIsValuable)
						{
							Find.WindowStack.Add(new Dialog_Confirm("ConfirmQuit".Translate(), delegate
							{
								RootMap.GoToMainMenu();
							}, true, null, true));
						}
						else
						{
							RootMap.GoToMainMenu();
						}
					};
					item = new ListableOption("QuitToMainMenu".Translate(), action, null);
					list.Add(item);
					Action action2 = delegate
					{
						if (GameDataSaveLoader.CurrentMapStateIsValuable)
						{
							Find.WindowStack.Add(new Dialog_Confirm("ConfirmQuit".Translate(), delegate
							{
								Root.Shutdown();
							}, true, null, true));
						}
						else
						{
							Root.Shutdown();
						}
					};
					item = new ListableOption("QuitToOS".Translate(), action2, null);
					list.Add(item);
				}
			}
			else
			{
				item = new ListableOption("QuitToOS".Translate(), delegate
				{
					Root.Shutdown();
				}, null);
				list.Add(item);
			}
			OptionListingUtility.DrawOptionListing(rect2, list);
			Text.Font = GameFont.Small;
			List<ListableOption> list2 = new List<ListableOption>();
			ListableOption item2 = new ListableOption_WebLink("FictionPrimer".Translate(), "https://docs.google.com/document/d/1pIZyKif0bFbBWten4drrm7kfSSfvBoJPgG9-ywfN8j8/pub", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("LudeonBlog".Translate(), "http://ludeon.com/blog", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("Forums".Translate(), "http://ludeon.com/forums", TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("OfficialWiki".Translate(), "http://rimworldwiki.com", TexButton.IconBlog);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansTwitter".Translate(), "https://twitter.com/TynanSylvester", TexButton.IconTwitter);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("TynansDesignBook".Translate(), "http://tynansylvester.com/book", TexButton.IconBook);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("HelpTranslate".Translate(), "http://ludeon.com/forums/index.php?topic=2933.0", TexButton.IconForums);
			list2.Add(item2);
			item2 = new ListableOption_WebLink("BuySoundtrack".Translate(), "http://www.lasgameaudio.co.uk/#!store/t04fw", TexButton.IconSoundtrack);
			list2.Add(item2);
			float num = OptionListingUtility.DrawOptionListing(rect3, list2);
			GUI.BeginGroup(rect3);
			if (Current.ProgramState == ProgramState.Entry && Widgets.ButtonImage(new Rect(0f, num + 10f, 64f, 32f), LanguageDatabase.activeLanguage.icon))
			{
				List<FloatMenuOption> list3 = new List<FloatMenuOption>();
				foreach (LoadedLanguage current in LanguageDatabase.AllLoadedLanguages)
				{
					LoadedLanguage localLang = current;
					list3.Add(new FloatMenuOption(localLang.FriendlyNameNative, delegate
					{
						LanguageDatabase.SelectLanguage(localLang);
						Prefs.Save();
					}, MenuOptionPriority.Medium, null, null, 0f, null));
				}
				Find.WindowStack.Add(new FloatMenu(list3));
			}
			GUI.EndGroup();
			GUI.EndGroup();
		}

		private static void InitLearnToPlay()
		{
			Current.Game = new Game();
			Current.Game.InitData = new GameInitData();
			Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
			Find.Scenario.PreConfigure();
			Current.Game.storyteller = new Storyteller(StorytellerDefOf.Tutor, DifficultyDefOf.VeryEasy);
			Page firstConfigPage = Current.Game.Scenario.GetFirstConfigPage();
			Page next = firstConfigPage.next;
			next.prev = null;
			Find.WindowStack.Add(next);
		}

		private static void CloseMainTab()
		{
			if (Current.ProgramState == ProgramState.MapPlaying)
			{
				Find.MainTabsRoot.EscapeCurrentTab(false);
			}
		}
	}
}
