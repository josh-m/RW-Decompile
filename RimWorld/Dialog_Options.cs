using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_Options : Window
	{
		private const float SubOptionTabWidth = 40f;

		private static readonly float[] UIScales = new float[]
		{
			1f,
			1.25f,
			1.5f,
			1.75f,
			2f,
			2.5f,
			3f,
			3.5f,
			4f
		};

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(900f, 700f);
			}
		}

		public Dialog_Options()
		{
			this.closeOnEscapeKey = true;
			this.doCloseButton = true;
			this.doCloseX = true;
			this.forcePause = true;
			this.absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = inRect.AtZero();
			rect.yMax -= 45f;
			Listing_Standard listing_Standard = new Listing_Standard(rect);
			listing_Standard.ColumnWidth = (rect.width - 34f) / 3f;
			Text.Font = GameFont.Medium;
			listing_Standard.Label("Audiovisuals".Translate());
			Text.Font = GameFont.Small;
			listing_Standard.Gap(12f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("GameVolume".Translate());
			Prefs.VolumeGame = listing_Standard.Slider(Prefs.VolumeGame, 0f, 1f);
			listing_Standard.Label("MusicVolume".Translate());
			Prefs.VolumeMusic = listing_Standard.Slider(Prefs.VolumeMusic, 0f, 1f);
			if (listing_Standard.ButtonTextLabeled("Resolution".Translate(), Dialog_Options.ResToString(Screen.width, Screen.height)))
			{
				Find.WindowStack.Add(new Dialog_ResolutionPicker());
			}
			bool customCursorEnabled = Prefs.CustomCursorEnabled;
			listing_Standard.CheckboxLabeled("CustomCursor".Translate(), ref customCursorEnabled, null);
			Prefs.CustomCursorEnabled = customCursorEnabled;
			bool fullScreen = Screen.fullScreen;
			bool flag = fullScreen;
			listing_Standard.CheckboxLabeled("Fullscreen".Translate(), ref fullScreen, null);
			if (fullScreen != flag)
			{
				ResolutionUtility.SafeSetFullscreen(fullScreen);
			}
			listing_Standard.Label("UIScale".Translate());
			float[] uIScales = Dialog_Options.UIScales;
			for (int i = 0; i < uIScales.Length; i++)
			{
				float num = uIScales[i];
				if (listing_Standard.RadioButton(num.ToString() + "x", Prefs.UIScale == num, 8f))
				{
					if (!ResolutionUtility.UIScaleSafeWithResolution(num, Screen.currentResolution))
					{
						Messages.Message("MessageScreenResTooSmallForUIScale".Translate(), MessageSound.RejectInput);
					}
					else
					{
						ResolutionUtility.SafeSetUIScale(num);
					}
				}
			}
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label("Gameplay".Translate());
			Text.Font = GameFont.Small;
			listing_Standard.Gap(12f);
			listing_Standard.Gap(12f);
			if (listing_Standard.ButtonText("KeyboardConfig".Translate(), null))
			{
				Find.WindowStack.Add(new Dialog_KeyBindings());
			}
			if (listing_Standard.ButtonText("ChooseLanguage".Translate(), null))
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					Messages.Message("ChangeLanguageFromMainMenu".Translate(), MessageSound.RejectInput);
				}
				else
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (LoadedLanguage current in LanguageDatabase.AllLoadedLanguages)
					{
						LoadedLanguage localLang = current;
						list.Add(new FloatMenuOption(localLang.FriendlyNameNative, delegate
						{
							LanguageDatabase.SelectLanguage(localLang);
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
			}
			if ((Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) && listing_Standard.ButtonText("OpenSaveGameDataFolder".Translate(), null))
			{
				Application.OpenURL(GenFilePaths.SaveDataFolderPath);
			}
			bool adaptiveTrainingEnabled = Prefs.AdaptiveTrainingEnabled;
			listing_Standard.CheckboxLabeled("LearningHelper".Translate(), ref adaptiveTrainingEnabled, null);
			Prefs.AdaptiveTrainingEnabled = adaptiveTrainingEnabled;
			if (listing_Standard.ButtonText("ResetAdaptiveTutor".Translate(), null))
			{
				Messages.Message("AdaptiveTutorIsReset".Translate(), MessageSound.Benefit);
				PlayerKnowledgeDatabase.ResetPersistent();
			}
			bool runInBackground = Prefs.RunInBackground;
			listing_Standard.CheckboxLabeled("RunInBackground".Translate(), ref runInBackground, null);
			Prefs.RunInBackground = runInBackground;
			bool edgeScreenScroll = Prefs.EdgeScreenScroll;
			listing_Standard.CheckboxLabeled("EdgeScreenScroll".Translate(), ref edgeScreenScroll, null);
			Prefs.EdgeScreenScroll = edgeScreenScroll;
			bool pauseOnLoad = Prefs.PauseOnLoad;
			listing_Standard.CheckboxLabeled("PauseOnLoad".Translate(), ref pauseOnLoad, null);
			Prefs.PauseOnLoad = pauseOnLoad;
			bool pauseOnUrgentLetter = Prefs.PauseOnUrgentLetter;
			listing_Standard.CheckboxLabeled("PauseOnUrgentLetter".Translate(), ref pauseOnUrgentLetter, null);
			Prefs.PauseOnUrgentLetter = pauseOnUrgentLetter;
			bool showRealtimeClock = Prefs.ShowRealtimeClock;
			listing_Standard.CheckboxLabeled("ShowRealtimeClock".Translate(), ref showRealtimeClock, null);
			Prefs.ShowRealtimeClock = showRealtimeClock;
			bool plantWindSway = Prefs.PlantWindSway;
			listing_Standard.CheckboxLabeled("PlantWindSway".Translate(), ref plantWindSway, null);
			Prefs.PlantWindSway = plantWindSway;
			int maxNumberOfPlayerHomes = Prefs.MaxNumberOfPlayerHomes;
			listing_Standard.Label("MaxNumberOfPlayerHomes".Translate(new object[]
			{
				maxNumberOfPlayerHomes
			}));
			int num2 = Mathf.RoundToInt(listing_Standard.Slider((float)maxNumberOfPlayerHomes, 1f, 5f));
			Prefs.MaxNumberOfPlayerHomes = num2;
			if (maxNumberOfPlayerHomes != num2 && num2 > 1)
			{
				TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.MaxNumberOfPlayerHomes);
			}
			if (listing_Standard.ButtonTextLabeled("TemperatureMode".Translate(), Prefs.TemperatureMode.ToStringHuman()))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				using (IEnumerator enumerator2 = Enum.GetValues(typeof(TemperatureDisplayMode)).GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						TemperatureDisplayMode localTmode2 = (TemperatureDisplayMode)((byte)enumerator2.Current);
						TemperatureDisplayMode localTmode = localTmode2;
						list2.Add(new FloatMenuOption(localTmode.ToString(), delegate
						{
							Prefs.TemperatureMode = localTmode;
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			float autosaveIntervalDays = Prefs.AutosaveIntervalDays;
			string text = "Days".Translate();
			string text2 = "Day".Translate().ToLower();
			if (listing_Standard.ButtonTextLabeled("AutosaveInterval".Translate(), autosaveIntervalDays + " " + ((autosaveIntervalDays != 1f) ? text : text2)))
			{
				List<FloatMenuOption> list3 = new List<FloatMenuOption>();
				if (Prefs.DevMode)
				{
					list3.Add(new FloatMenuOption("0.125 " + text + "(debug)", delegate
					{
						Prefs.AutosaveIntervalDays = 0.125f;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
					list3.Add(new FloatMenuOption("0.25 " + text + "(debug)", delegate
					{
						Prefs.AutosaveIntervalDays = 0.25f;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				list3.Add(new FloatMenuOption("0.5 " + text + string.Empty, delegate
				{
					Prefs.AutosaveIntervalDays = 0.5f;
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				list3.Add(new FloatMenuOption(1.ToString() + " " + text2, delegate
				{
					Prefs.AutosaveIntervalDays = 1f;
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				list3.Add(new FloatMenuOption(3.ToString() + " " + text, delegate
				{
					Prefs.AutosaveIntervalDays = 3f;
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				list3.Add(new FloatMenuOption(7.ToString() + " " + text, delegate
				{
					Prefs.AutosaveIntervalDays = 7f;
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				list3.Add(new FloatMenuOption(14.ToString() + " " + text, delegate
				{
					Prefs.AutosaveIntervalDays = 14f;
				}, MenuOptionPriority.Default, null, null, 0f, null, null));
				Find.WindowStack.Add(new FloatMenu(list3));
			}
			if (Current.ProgramState == ProgramState.Playing && Current.Game.Info.permadeathMode && Prefs.AutosaveIntervalDays > 1f)
			{
				GUI.color = Color.red;
				listing_Standard.Label("MaxPermadeathAutosaveIntervalInfo".Translate(new object[]
				{
					1f
				}));
				GUI.color = Color.white;
			}
			if (Current.ProgramState == ProgramState.Playing && listing_Standard.ButtonText("ChangeStoryteller".Translate(), "OptionsButton-ChooseStoryteller") && TutorSystem.AllowAction("ChooseStoryteller"))
			{
				Find.WindowStack.Add(new Page_SelectStorytellerInGame());
			}
			if (listing_Standard.ButtonTextLabeled("ShowAnimalNames".Translate(), Prefs.AnimalNameMode.ToStringHuman()))
			{
				List<FloatMenuOption> list4 = new List<FloatMenuOption>();
				using (IEnumerator enumerator3 = Enum.GetValues(typeof(AnimalNameDisplayMode)).GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						AnimalNameDisplayMode localMode2 = (AnimalNameDisplayMode)((byte)enumerator3.Current);
						AnimalNameDisplayMode localMode = localMode2;
						list4.Add(new FloatMenuOption(localMode.ToStringHuman(), delegate
						{
							Prefs.AnimalNameMode = localMode;
						}, MenuOptionPriority.Default, null, null, 0f, null, null));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list4));
			}
			bool devMode = Prefs.DevMode;
			listing_Standard.CheckboxLabeled("DevelopmentMode".Translate(), ref devMode, null);
			Prefs.DevMode = devMode;
			if (Prefs.DevMode)
			{
				bool resetModsConfigOnCrash = Prefs.ResetModsConfigOnCrash;
				listing_Standard.CheckboxLabeled("ResetModsConfigOnCrash".Translate(), ref resetModsConfigOnCrash, null);
				Prefs.ResetModsConfigOnCrash = resetModsConfigOnCrash;
				bool logVerbose = Prefs.LogVerbose;
				listing_Standard.CheckboxLabeled("LogVerbose".Translate(), ref logVerbose, null);
				Prefs.LogVerbose = logVerbose;
			}
			listing_Standard.NewColumn();
			Text.Font = GameFont.Medium;
			listing_Standard.Label(string.Empty);
			Text.Font = GameFont.Small;
			listing_Standard.Gap(12f);
			listing_Standard.Gap(12f);
			listing_Standard.Label("NamesYouWantToSee".Translate());
			Prefs.PreferredNames.RemoveAll((string n) => n.NullOrEmpty());
			for (int j = 0; j < Prefs.PreferredNames.Count; j++)
			{
				string name = Prefs.PreferredNames[j];
				PawnBio pawnBio = (from b in SolidBioDatabase.allBios
				where b.name.ToString() == name
				select b).FirstOrDefault<PawnBio>();
				if (pawnBio == null)
				{
					name += " [N]";
				}
				else
				{
					PawnBioType bioType = pawnBio.BioType;
					if (bioType != PawnBioType.BackstoryInGame)
					{
						if (bioType == PawnBioType.PirateKing)
						{
							name += " [PK]";
						}
					}
					else
					{
						name += " [B]";
					}
				}
				Rect rect2 = listing_Standard.GetRect(24f);
				Widgets.Label(rect2, name);
				Rect butRect = new Rect(rect2.xMax - 24f, rect2.y, 24f, 24f);
				if (Widgets.ButtonImage(butRect, TexButton.DeleteX))
				{
					Prefs.PreferredNames.RemoveAt(j);
					SoundDefOf.TickLow.PlayOneShotOnCamera();
				}
			}
			if (Prefs.PreferredNames.Count < 6 && listing_Standard.ButtonText("AddName".Translate() + "...", null))
			{
				Find.WindowStack.Add(new Dialog_AddPreferredName());
			}
			listing_Standard.End();
		}

		public override void PreClose()
		{
			base.PreClose();
			Prefs.Save();
		}

		public static string ResToString(int width, int height)
		{
			string text = width + "x" + height;
			if (width == 1280 && height == 720)
			{
				text += " (720p)";
			}
			if (width == 1920 && height == 1080)
			{
				text += " (1080p)";
			}
			return text;
		}
	}
}
