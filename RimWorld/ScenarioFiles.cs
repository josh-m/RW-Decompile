using System;
using System.Collections.Generic;
using System.IO;
using Verse;
using Verse.Steam;

namespace RimWorld
{
	public static class ScenarioFiles
	{
		private static List<Scenario> scenariosLocal = new List<Scenario>();

		private static List<Scenario> scenariosWorkshop = new List<Scenario>();

		public static IEnumerable<Scenario> AllScenariosLocal
		{
			get
			{
				return ScenarioFiles.scenariosLocal;
			}
		}

		public static IEnumerable<Scenario> AllScenariosWorkshop
		{
			get
			{
				return ScenarioFiles.scenariosWorkshop;
			}
		}

		public static void RecacheData()
		{
			ScenarioFiles.scenariosLocal.Clear();
			foreach (FileInfo current in GenFilePaths.AllCustomScenarioFiles)
			{
				Scenario item;
				if (GameDataSaveLoader.TryLoadScenario(current.FullName, ScenarioCategory.CustomLocal, out item))
				{
					ScenarioFiles.scenariosLocal.Add(item);
				}
			}
			ScenarioFiles.scenariosWorkshop.Clear();
			foreach (WorkshopItem current2 in WorkshopItems.AllSubscribedItems)
			{
				WorkshopItem_Scenario workshopItem_Scenario = current2 as WorkshopItem_Scenario;
				if (workshopItem_Scenario != null)
				{
					ScenarioFiles.scenariosWorkshop.Add(workshopItem_Scenario.GetScenario());
				}
			}
		}
	}
}
