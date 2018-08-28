using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Ship_Populate : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			if (!rp.thrustAxis.HasValue)
			{
				Log.ErrorOnce("No thrust axis when generating ship parts", 50627817, false);
			}
			foreach (KeyValuePair<ThingDef, int> current in ShipUtility.RequiredParts())
			{
				for (int i = 0; i < current.Value; i++)
				{
					Rot4 rotation = Rot4.Random;
					if (current.Key == ThingDefOf.Ship_Engine && rp.thrustAxis.HasValue)
					{
						rotation = rp.thrustAxis.Value;
					}
					this.AttemptToPlace(current.Key, rp.rect, rotation, rp.faction);
				}
			}
		}

		public void AttemptToPlace(ThingDef thingDef, CellRect rect, Rot4 rotation, Faction faction)
		{
			Map map = BaseGen.globalSettings.map;
			Thing thing;
			IntVec3 loc = (from cell in rect.Cells.InRandomOrder(null)
			where GenConstruct.CanPlaceBlueprintAt(thingDef, cell, rotation, map, false, null).Accepted && GenAdj.OccupiedRect(cell, rotation, thingDef.Size).AdjacentCellsCardinal.Any(delegate(IntVec3 edgeCell)
			{
				bool arg_42_0;
				if (edgeCell.InBounds(map))
				{
					arg_42_0 = edgeCell.GetThingList(map).Any((Thing thing) => thing.def == ThingDefOf.Ship_Beam);
				}
				else
				{
					arg_42_0 = false;
				}
				return arg_42_0;
			})
			select cell).FirstOrFallback(IntVec3.Invalid);
			if (loc.IsValid)
			{
				thing = ThingMaker.MakeThing(thingDef, null);
				thing.SetFaction(faction, null);
				CompHibernatable compHibernatable = thing.TryGetComp<CompHibernatable>();
				if (compHibernatable != null)
				{
					compHibernatable.State = HibernatableStateDefOf.Hibernating;
				}
				GenSpawn.Spawn(thing, loc, BaseGen.globalSettings.map, rotation, WipeMode.Vanish, false);
			}
		}
	}
}
