using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public static class ReverseDesignatorDatabase
	{
		private static List<Designator> desList;

		public static List<Designator> AllDesignators
		{
			get
			{
				if (ReverseDesignatorDatabase.desList == null)
				{
					ReverseDesignatorDatabase.InitDesignators();
				}
				return ReverseDesignatorDatabase.desList;
			}
		}

		public static void Reinit()
		{
			ReverseDesignatorDatabase.desList = null;
		}

		public static T Get<T>() where T : Designator
		{
			if (ReverseDesignatorDatabase.desList == null)
			{
				ReverseDesignatorDatabase.InitDesignators();
			}
			for (int i = 0; i < ReverseDesignatorDatabase.desList.Count; i++)
			{
				T t = ReverseDesignatorDatabase.desList[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		private static void InitDesignators()
		{
			ReverseDesignatorDatabase.desList = new List<Designator>();
			ReverseDesignatorDatabase.desList.Add(new Designator_Cancel());
			ReverseDesignatorDatabase.desList.Add(new Designator_Claim());
			ReverseDesignatorDatabase.desList.Add(new Designator_Deconstruct());
			ReverseDesignatorDatabase.desList.Add(new Designator_Uninstall());
			ReverseDesignatorDatabase.desList.Add(new Designator_Haul());
			ReverseDesignatorDatabase.desList.Add(new Designator_Hunt());
			ReverseDesignatorDatabase.desList.Add(new Designator_Slaughter());
			ReverseDesignatorDatabase.desList.Add(new Designator_Tame());
			ReverseDesignatorDatabase.desList.Add(new Designator_PlantsCut());
			ReverseDesignatorDatabase.desList.Add(new Designator_PlantsHarvest());
			ReverseDesignatorDatabase.desList.Add(new Designator_Mine());
			ReverseDesignatorDatabase.desList.Add(new Designator_Strip());
			ReverseDesignatorDatabase.desList.Add(new Designator_RearmTrap());
			ReverseDesignatorDatabase.desList.Add(new Designator_Open());
			ReverseDesignatorDatabase.desList.RemoveAll((Designator des) => !Current.Game.Rules.DesignatorAllowed(des));
		}
	}
}
