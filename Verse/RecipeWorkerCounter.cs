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
			ThingCountClass thingCountClass = this.recipe.products[0];
			if (thingCountClass.thingDef.CountAsResource)
			{
				return bill.Map.resourceCounter.GetCount(thingCountClass.thingDef);
			}
			int num = bill.Map.listerThings.ThingsOfDef(thingCountClass.thingDef).Count;
			if (thingCountClass.thingDef.Minifiable)
			{
				List<Thing> list = bill.Map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
				for (int i = 0; i < list.Count; i++)
				{
					MinifiedThing minifiedThing = (MinifiedThing)list[i];
					if (minifiedThing.InnerThing.def == thingCountClass.thingDef)
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
