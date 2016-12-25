using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace RimWorld
{
	public static class ScenarioLister
	{
		private static bool dirty = true;

		[DebuggerHidden]
		public static IEnumerable<Scenario> AllScenarios()
		{
			ScenarioLister.RecacheIfDirty();
			foreach (ScenarioDef scenDef in DefDatabase<ScenarioDef>.AllDefs)
			{
				yield return scenDef.scenario;
			}
			foreach (Scenario scen in ScenarioFiles.AllScenariosLocal)
			{
				yield return scen;
			}
			foreach (Scenario scen2 in ScenarioFiles.AllScenariosWorkshop)
			{
				yield return scen2;
			}
		}

		[DebuggerHidden]
		public static IEnumerable<Scenario> ScenariosInCategory(ScenarioCategory cat)
		{
			ScenarioLister.RecacheIfDirty();
			if (cat == ScenarioCategory.FromDef)
			{
				foreach (ScenarioDef scenDef in DefDatabase<ScenarioDef>.AllDefs)
				{
					yield return scenDef.scenario;
				}
			}
			else if (cat == ScenarioCategory.CustomLocal)
			{
				foreach (Scenario scen in ScenarioFiles.AllScenariosLocal)
				{
					yield return scen;
				}
			}
			else if (cat == ScenarioCategory.SteamWorkshop)
			{
				foreach (Scenario scen2 in ScenarioFiles.AllScenariosWorkshop)
				{
					yield return scen2;
				}
			}
		}

		public static bool ScenarioIsListedAnywhere(Scenario scen)
		{
			ScenarioLister.RecacheIfDirty();
			foreach (ScenarioDef current in DefDatabase<ScenarioDef>.AllDefs)
			{
				if (current.scenario == scen)
				{
					bool result = true;
					return result;
				}
			}
			foreach (Scenario current2 in ScenarioFiles.AllScenariosLocal)
			{
				if (scen == current2)
				{
					bool result = true;
					return result;
				}
			}
			return false;
		}

		public static void MarkDirty()
		{
			ScenarioLister.dirty = true;
		}

		private static void RecacheIfDirty()
		{
			if (ScenarioLister.dirty)
			{
				ScenarioLister.RecacheData();
			}
		}

		private static void RecacheData()
		{
			ScenarioLister.dirty = false;
			int num = ScenarioLister.ScenarioListHash();
			ScenarioFiles.RecacheData();
			if (ScenarioLister.ScenarioListHash() != num && !LongEventHandler.ShouldWaitForEvent)
			{
				Page_SelectScenario page_SelectScenario = Find.WindowStack.WindowOfType<Page_SelectScenario>();
				if (page_SelectScenario != null)
				{
					page_SelectScenario.Notify_ScenarioListChanged();
				}
			}
		}

		public static int ScenarioListHash()
		{
			int num = 9826121;
			foreach (Scenario current in ScenarioLister.AllScenarios())
			{
				num ^= 791 * current.GetHashCode() * 6121;
			}
			return num;
		}
	}
}
