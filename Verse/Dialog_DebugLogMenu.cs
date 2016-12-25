using RimWorld;
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.Steam;

namespace Verse
{
	public class Dialog_DebugLogMenu : Dialog_DebugOptionLister
	{
		public override bool IsDebug
		{
			get
			{
				return true;
			}
		}

		public Dialog_DebugLogMenu()
		{
			this.forcePause = true;
		}

		protected override void DoListingItems()
		{
			this.listing.Label("Logs");
			base.DebugAction("Trader stock gen data", delegate
			{
				TraderStockGenerator.LogGenerationData();
			});
			base.DebugAction("Quality gen data", delegate
			{
				QualityUtility.LogGenerationData();
			});
			base.DebugAction("Song selection info", delegate
			{
				Find.MusicManagerPlay.LogSongSelectionData();
			});
			base.DebugAction("Plant data", delegate
			{
				GenPlant.LogPlantData();
			});
			base.DebugAction("Age injuries", delegate
			{
				AgeInjuryUtility.LogOldInjuryCalculations();
			});
			base.DebugAction("Pawn groups made", delegate
			{
				PawnGroupMakerUtility.LogPawnGroupsMade();
			});
			base.DebugAction("All loaded assets", delegate
			{
				DebugLogWriter.LogAllLoadedAssets();
			});
			base.DebugAction("All graphics in database", delegate
			{
				GraphicDatabase.DebugLogAllGraphics();
			});
			base.DebugAction("Rand tests", delegate
			{
				Rand.LogRandTests();
			});
			base.DebugAction("Steam Workshop status", delegate
			{
				Workshop.LogStatus();
			});
			base.DebugAction("Math perf", delegate
			{
				GenMath.LogTestMathPerf();
			});
			base.DebugAction("MeshPool stats", delegate
			{
				MeshPool.LogStats();
			});
			base.DebugAction("Lords", delegate
			{
				Find.VisibleMap.lordManager.LogLords();
			});
			base.DebugAction("Tribal solid backstories", delegate
			{
				Dialog_DebugLogMenu.LogSolidBackstoriesWithSpawnCategory("Tribal");
			});
			base.DebugAction("Pod contents test", delegate
			{
				IncidentWorker_ResourcePodCrash.DebugLogPodContentsChoices();
			});
			base.DebugAction("Path cost ignore repeaters", delegate
			{
				PathGrid.LogPathCostIgnoreRepeaters();
			});
			base.DebugAction("Key strings", delegate
			{
				Dialog_DebugLogMenu.LogKeyStrings();
			});
			base.DebugAction("Animal stock gen", delegate
			{
				StockGenerator_Animals.LogStockGeneration();
			});
			MethodInfo[] methods = typeof(DataAnalysisLogger).GetMethods(BindingFlags.Static | BindingFlags.Public);
			MethodInfo mi;
			for (int i = 0; i < methods.Length; i++)
			{
				mi = methods[i];
				string name = mi.Name;
				if (name.StartsWith("DoLog_"))
				{
					base.DebugAction(GenText.SplitCamelCase(name.Substring(6)), delegate
					{
						mi.Invoke(null, null);
					});
				}
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				base.DebugAction("Plant proportions", delegate
				{
					GenPlant.LogPlantProportions();
				});
				base.DebugAction("Database tales list", delegate
				{
					Find.TaleManager.LogTales();
				});
				base.DebugAction("Database tales interest", delegate
				{
					Find.TaleManager.LogTaleInterestSummary();
				});
				base.DebugAction("Database tales descs", delegate
				{
					TaleTester.LogTalesInDatabase();
				});
				base.DebugAction("Random tales descs", delegate
				{
					TaleTester.LogGeneratedTales(40);
				});
				base.DebugAction("Taleless descs", delegate
				{
					TaleTester.LogDescriptionsTaleless();
				});
				base.DebugAction("Temperature data", delegate
				{
					Find.VisibleMap.mapTemperature.DebugLogTemps();
				});
				base.DebugAction("Weather chances", delegate
				{
					Find.VisibleMap.weatherDecider.LogWeatherChances();
				});
				base.DebugAction("Celestial glow", delegate
				{
					GenCelestial.LogSunGlowForYear();
				});
				base.DebugAction("ListerPawns", delegate
				{
					Find.VisibleMap.mapPawns.LogListedPawns();
				});
				base.DebugAction("Wind speeds", delegate
				{
					Find.VisibleMap.windManager.LogWindSpeeds();
				});
				base.DebugAction("Kidnapped pawns", delegate
				{
					Find.FactionManager.LogKidnappedPawns();
				});
				base.DebugAction("World pawns", delegate
				{
					Find.WorldPawns.LogWorldPawns();
				});
				base.DebugAction("Draw list", delegate
				{
					Find.VisibleMap.dynamicDrawManager.LogDynamicDrawThings();
				});
				base.DebugAction("Future incidents", delegate
				{
					StorytellerUtility.DebugLogTestFutureIncidents();
				});
			}
			this.listing.Gap(12f);
			Text.Font = GameFont.Small;
			this.listing.Label("Tables");
			base.DebugAction("Population intent", delegate
			{
				Find.Storyteller.intenderPopulation.DoTable_PopulationIntents();
			});
			base.DebugAction("Pop-adj recruit difficulty", delegate
			{
				PawnUtility.DoTable_PopIntentRecruitDifficulty();
			});
			MethodInfo[] methods2 = typeof(DataAnalysisTableMaker).GetMethods(BindingFlags.Static | BindingFlags.Public);
			MethodInfo mi2;
			for (int j = 0; j < methods2.Length; j++)
			{
				mi2 = methods2[j];
				string name2 = mi2.Name;
				if (name2.StartsWith("DoTable_"))
				{
					base.DebugAction(GenText.SplitCamelCase(name2.Substring(8)), delegate
					{
						mi2.Invoke(null, null);
					});
				}
			}
		}

		private static void LogSolidBackstoriesWithSpawnCategory(string spawnCategory)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PawnBio current in SolidBioDatabase.allBios)
			{
				if (current.adulthood.spawnCategories.Contains(spawnCategory))
				{
					stringBuilder.AppendLine(current.ToString());
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		private static void LogKeyStrings()
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (IEnumerator enumerator = Enum.GetValues(typeof(KeyCode)).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyCode keyCode = (KeyCode)((int)enumerator.Current);
					stringBuilder.AppendLine(keyCode.ToString() + " - " + keyCode.ToStringReadable());
				}
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
