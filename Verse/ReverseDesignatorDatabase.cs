using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class ReverseDesignatorDatabase
	{
		private List<Designator> desList;

		public List<Designator> AllDesignators
		{
			get
			{
				if (this.desList == null)
				{
					this.InitDesignators();
				}
				return this.desList;
			}
		}

		public void Reinit()
		{
			this.desList = null;
		}

		public T Get<T>() where T : Designator
		{
			if (this.desList == null)
			{
				this.InitDesignators();
			}
			for (int i = 0; i < this.desList.Count; i++)
			{
				T t = this.desList[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		private void InitDesignators()
		{
			this.desList = new List<Designator>();
			this.desList.Add(new Designator_Cancel());
			this.desList.Add(new Designator_Claim());
			this.desList.Add(new Designator_Deconstruct());
			this.desList.Add(new Designator_Uninstall());
			this.desList.Add(new Designator_Haul());
			this.desList.Add(new Designator_Hunt());
			this.desList.Add(new Designator_Slaughter());
			this.desList.Add(new Designator_Tame());
			this.desList.Add(new Designator_PlantsCut());
			this.desList.Add(new Designator_PlantsHarvest());
			this.desList.Add(new Designator_Mine());
			this.desList.Add(new Designator_Strip());
			this.desList.Add(new Designator_RearmTrap());
			this.desList.Add(new Designator_Open());
			this.desList.RemoveAll((Designator des) => !Current.Game.Rules.DesignatorAllowed(des));
		}
	}
}
