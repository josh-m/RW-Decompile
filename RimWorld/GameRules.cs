using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GameRules : IExposable
	{
		private List<Type> disallowedDesignatorTypes = new List<Type>();

		private List<BuildableDef> disallowedBuildings = new List<BuildableDef>();

		public void SetAllowDesignator(Type type, bool allowed)
		{
			if (allowed && this.disallowedDesignatorTypes.Contains(type))
			{
				this.disallowedDesignatorTypes.Remove(type);
			}
			if (!allowed && !this.disallowedDesignatorTypes.Contains(type))
			{
				this.disallowedDesignatorTypes.Add(type);
			}
			Find.ReverseDesignatorDatabase.Reinit();
		}

		public void SetAllowBuilding(BuildableDef building, bool allowed)
		{
			if (allowed && this.disallowedBuildings.Contains(building))
			{
				this.disallowedBuildings.Remove(building);
			}
			if (!allowed && !this.disallowedBuildings.Contains(building))
			{
				this.disallowedBuildings.Add(building);
			}
		}

		public bool DesignatorAllowed(Designator d)
		{
			Designator_Place designator_Place = d as Designator_Place;
			if (designator_Place != null)
			{
				return !this.disallowedBuildings.Contains(designator_Place.PlacingDef);
			}
			return !this.disallowedDesignatorTypes.Contains(d.GetType());
		}

		public void ExposeData()
		{
			Scribe_Collections.LookList<BuildableDef>(ref this.disallowedBuildings, "disallowedBuildings", LookMode.Undefined, new object[0]);
			Scribe_Collections.LookList<Type>(ref this.disallowedDesignatorTypes, "disallowedDesignatorTypes", LookMode.Undefined, new object[0]);
		}
	}
}
