using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class WaterSplash : Projectile
	{
		protected override void Impact(Thing HitThing)
		{
			base.Impact(HitThing);
			List<Thing> list = new List<Thing>();
			foreach (Thing current in Find.ThingGrid.ThingsAt(base.Position))
			{
				if (current.def == ThingDefOf.Fire)
				{
					list.Add(current);
				}
			}
			foreach (Thing current2 in list)
			{
				current2.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
