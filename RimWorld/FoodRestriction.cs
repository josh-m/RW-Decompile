using System;
using Verse;

namespace RimWorld
{
	public class FoodRestriction : IExposable, ILoadReferenceable
	{
		public int id;

		public string label;

		public ThingFilter filter = new ThingFilter();

		public FoodRestriction(int id, string label)
		{
			this.id = id;
			this.label = label;
		}

		public FoodRestriction()
		{
		}

		public bool Allows(ThingDef def)
		{
			return this.filter.Allows(def);
		}

		public bool Allows(Thing thing)
		{
			return this.filter.Allows(thing);
		}

		public void ExposeData()
		{
			Scribe_Values.Look<int>(ref this.id, "id", 0, false);
			Scribe_Values.Look<string>(ref this.label, "label", null, false);
			Scribe_Deep.Look<ThingFilter>(ref this.filter, "filter", new object[0]);
		}

		public string GetUniqueLoadID()
		{
			return "FoodRestriction_" + this.label + this.id;
		}
	}
}
