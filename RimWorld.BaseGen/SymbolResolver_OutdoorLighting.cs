using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_OutdoorLighting : SymbolResolver
	{
		private static List<CompGlower> nearbyGlowers = new List<CompGlower>();

		private const float Margin = 2f;

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef;
			if (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Industrial)
			{
				thingDef = ThingDefOf.StandingLamp;
			}
			else
			{
				thingDef = ThingDefOf.TorchLamp;
			}
			this.FindNearbyGlowers(rp.rect);
			for (int i = 0; i < rp.rect.Area / 4; i++)
			{
				IntVec3 randomCell = rp.rect.RandomCell;
				if (randomCell.Standable(map) && randomCell.GetFirstItem(map) == null && randomCell.GetFirstPawn(map) == null && randomCell.GetFirstBuilding(map) == null)
				{
					Region region = randomCell.GetRegion(map, RegionType.Set_Passable);
					if (region != null && region.Room.PsychologicallyOutdoors && region.Room.UsesOutdoorTemperature)
					{
						if (!this.AnyGlowerNearby(randomCell))
						{
							if (!BaseGenUtility.AnyDoorAdjacentCardinalTo(randomCell, map))
							{
								if (!rp.spawnBridgeIfTerrainCantSupportThing.HasValue || rp.spawnBridgeIfTerrainCantSupportThing.Value)
								{
									BaseGenUtility.CheckSpawnBridgeUnder(thingDef, randomCell, Rot4.North);
								}
								Thing thing = GenSpawn.Spawn(thingDef, randomCell, map, WipeMode.Vanish);
								if (thing.def.CanHaveFaction && thing.Faction != rp.faction)
								{
									thing.SetFaction(rp.faction, null);
								}
								SymbolResolver_OutdoorLighting.nearbyGlowers.Add(thing.TryGetComp<CompGlower>());
							}
						}
					}
				}
			}
			SymbolResolver_OutdoorLighting.nearbyGlowers.Clear();
		}

		private void FindNearbyGlowers(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_OutdoorLighting.nearbyGlowers.Clear();
			rect = rect.ExpandedBy(4);
			rect = rect.ClipInsideMap(map);
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				Region region = iterator.Current.GetRegion(map, RegionType.Set_Passable);
				if (region != null && region.Room.PsychologicallyOutdoors)
				{
					List<Thing> thingList = iterator.Current.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						CompGlower compGlower = thingList[i].TryGetComp<CompGlower>();
						if (compGlower != null)
						{
							SymbolResolver_OutdoorLighting.nearbyGlowers.Add(compGlower);
						}
					}
				}
				iterator.MoveNext();
			}
		}

		private bool AnyGlowerNearby(IntVec3 c)
		{
			for (int i = 0; i < SymbolResolver_OutdoorLighting.nearbyGlowers.Count; i++)
			{
				if (c.InHorDistOf(SymbolResolver_OutdoorLighting.nearbyGlowers[i].parent.Position, SymbolResolver_OutdoorLighting.nearbyGlowers[i].Props.glowRadius + 2f))
				{
					return true;
				}
			}
			return false;
		}
	}
}
