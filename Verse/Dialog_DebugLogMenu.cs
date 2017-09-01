using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;

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
			this.listing.Label("Logs", -1f);
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
					StorytellerUtility.DebugLogTestFutureIncidents(false);
				});
				base.DebugAction("Future incidents (visible map)", delegate
				{
					StorytellerUtility.DebugLogTestFutureIncidents(true);
				});
				base.DebugAction("Map pawns", delegate
				{
					Find.VisibleMap.mapPawns.LogListedPawns();
				});
			}
			this.listing.Gap(12f);
			Text.Font = GameFont.Small;
			this.listing.Label("Tables", -1f);
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
			this.listing.Gap(12f);
			this.listing.Label("UI", -1f);
			base.DebugAction("Pawn column", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				List<PawnColumnDef> allDefsListForReading = DefDatabase<PawnColumnDef>.AllDefsListForReading;
				for (int k = 0; k < allDefsListForReading.Count; k++)
				{
					PawnColumnDef localDef = allDefsListForReading[k];
					list.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_PawnTableTest(localDef));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}
	}
}
