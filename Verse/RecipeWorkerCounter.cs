using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class RecipeWorkerCounter
	{
		public RecipeDef recipe;

		public virtual bool CanCountProducts(Bill_Production bill)
		{
			return this.recipe.specialProducts == null && this.recipe.products != null && this.recipe.products.Count == 1;
		}

		public virtual int CountProducts(Bill_Production bill)
		{
			ThingCount thingCount = this.recipe.products[0];
			if (thingCount.thingDef.CountAsResource)
			{
				return Find.ResourceCounter.GetCount(thingCount.thingDef);
			}
			int num = Find.ListerThings.ThingsOfDef(thingCount.thingDef).Count;
			if (thingCount.thingDef.Minifiable)
			{
				List<Thing> list = Find.ListerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
				for (int i = 0; i < list.Count; i++)
				{
					MinifiedThing minifiedThing = (MinifiedThing)list[i];
					if (minifiedThing.InnerThing.def == thingCount.thingDef)
					{
						num++;
					}
				}
			}
			return num;
		}

		public virtual string ProductsDescription(Bill_Production bill)
		{
			return null;
		}
	}
}
