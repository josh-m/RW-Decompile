using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class GenConstruct
	{
		private static string ConstructionSkillTooLowTrans;

		public static void Reset()
		{
			GenConstruct.ConstructionSkillTooLowTrans = "ConstructionSkillTooLow".Translate();
		}

		public static Blueprint_Build PlaceBlueprintForBuild(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff)
		{
			Blueprint_Build blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(sourceDef.blueprintDef, null);
			blueprint_Build.SetFactionDirect(faction);
			blueprint_Build.stuffToUse = stuff;
			GenSpawn.Spawn(blueprint_Build, center, map, rotation, false);
			return blueprint_Build;
		}

		public static Blueprint_Install PlaceBlueprintForInstall(MinifiedThing itemToInstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		{
			Blueprint_Install blueprint_Install = (Blueprint_Install)ThingMaker.MakeThing(itemToInstall.InnerThing.def.installBlueprintDef, null);
			blueprint_Install.SetThingToInstallFromMinified(itemToInstall);
			blueprint_Install.SetFactionDirect(faction);
			GenSpawn.Spawn(blueprint_Install, center, map, rotation, false);
			return blueprint_Install;
		}

		public static Blueprint_Install PlaceBlueprintForReinstall(Building buildingToReinstall, IntVec3 center, Map map, Rot4 rotation, Faction faction)
		{
			Blueprint_Install blueprint_Install = (Blueprint_Install)ThingMaker.MakeThing(buildingToReinstall.def.installBlueprintDef, null);
			blueprint_Install.SetBuildingToReinstall(buildingToReinstall);
			blueprint_Install.SetFactionDirect(faction);
			GenSpawn.Spawn(blueprint_Install, center, map, rotation, false);
			return blueprint_Install;
		}

		public static bool CanBuildOnTerrain(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, Thing thingToIgnore = null)
		{
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null && !c.GetTerrain(map).changeable)
			{
				return false;
			}
			CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
			cellRect.ClipInsideMap(map);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				TerrainDef terrainDef2 = map.terrainGrid.TerrainAt(iterator.Current);
				if (!terrainDef2.affordances.Contains(entDef.terrainAffordanceNeeded))
				{
					return false;
				}
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i] != thingToIgnore)
					{
						TerrainDef terrainDef3 = thingList[i].def.entityDefToBuild as TerrainDef;
						if (terrainDef3 != null && !terrainDef3.affordances.Contains(entDef.terrainAffordanceNeeded))
						{
							return false;
						}
					}
				}
				iterator.MoveNext();
			}
			return true;
		}

		public static Thing MiniToInstallOrBuildingToReinstall(Blueprint b)
		{
			Blueprint_Install blueprint_Install = b as Blueprint_Install;
			if (blueprint_Install != null)
			{
				return blueprint_Install.MiniToInstallOrBuildingToReinstall;
			}
			return null;
		}

		public static bool CanConstruct(Thing t, Pawn p, bool forced = false)
		{
			if (GenConstruct.FirstBlockingThing(t, p) != null)
			{
				return false;
			}
			LocalTargetInfo target = t;
			PathEndMode peMode = PathEndMode.Touch;
			Danger maxDanger = (!forced) ? p.NormalMaxDanger() : Danger.Deadly;
			if (!p.CanReserveAndReach(target, peMode, maxDanger, 1, -1, null, forced))
			{
				return false;
			}
			if (t.IsBurning())
			{
				return false;
			}
			if (p.skills.GetSkill(SkillDefOf.Construction).Level < t.def.constructionSkillPrerequisite)
			{
				JobFailReason.Is(GenConstruct.ConstructionSkillTooLowTrans);
				return false;
			}
			return true;
		}

		public static int AmountNeededByOf(IConstructible c, ThingDef resDef)
		{
			foreach (ThingCountClass current in c.MaterialsNeeded())
			{
				if (current.thingDef == resDef)
				{
					return current.count;
				}
			}
			return 0;
		}

		public static AcceptanceReport CanPlaceBlueprintAt(BuildableDef entDef, IntVec3 center, Rot4 rot, Map map, bool godMode = false, Thing thingToIgnore = null)
		{
			CellRect cellRect = GenAdj.OccupiedRect(center, rot, entDef.Size);
			CellRect.CellRectIterator iterator = cellRect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (!current.InBounds(map))
				{
					return new AcceptanceReport("OutOfBounds".Translate());
				}
				if (current.InNoBuildEdgeArea(map) && !DebugSettings.godMode)
				{
					return "TooCloseToMapEdge".Translate();
				}
				iterator.MoveNext();
			}
			if (center.Fogged(map))
			{
				return "CannotPlaceInUndiscovered".Translate();
			}
			List<Thing> thingList = center.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing != thingToIgnore)
				{
					if (thing.Position == center && thing.Rotation == rot)
					{
						if (thing.def == entDef)
						{
							return new AcceptanceReport("IdenticalThingExists".Translate());
						}
						if (thing.def.entityDefToBuild == entDef)
						{
							if (thing is Blueprint)
							{
								return new AcceptanceReport("IdenticalBlueprintExists".Translate());
							}
							return new AcceptanceReport("IdenticalThingExists".Translate());
						}
					}
				}
			}
			ThingDef thingDef = entDef as ThingDef;
			if (thingDef != null && thingDef.hasInteractionCell)
			{
				IntVec3 c = ThingUtility.InteractionCellWhenAt(thingDef, center, rot, map);
				if (!c.InBounds(map))
				{
					return new AcceptanceReport("InteractionSpotOutOfBounds".Translate());
				}
				List<Thing> list = map.thingGrid.ThingsListAtFast(c);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] != thingToIgnore)
					{
						if (list[j].def.passability == Traversability.Impassable)
						{
							return new AcceptanceReport("InteractionSpotBlocked".Translate(new object[]
							{
								list[j].LabelNoCount
							}).CapitalizeFirst());
						}
						Blueprint blueprint = list[j] as Blueprint;
						if (blueprint != null && blueprint.def.entityDefToBuild.passability == Traversability.Impassable)
						{
							return new AcceptanceReport("InteractionSpotWillBeBlocked".Translate(new object[]
							{
								blueprint.LabelNoCount
							}).CapitalizeFirst());
						}
					}
				}
			}
			if (entDef.passability != Traversability.Standable)
			{
				foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(center, rot, entDef.Size))
				{
					if (current2.InBounds(map))
					{
						thingList = current2.GetThingList(map);
						for (int k = 0; k < thingList.Count; k++)
						{
							Thing thing2 = thingList[k];
							if (thing2 != thingToIgnore)
							{
								Blueprint blueprint2 = thing2 as Blueprint;
								ThingDef thingDef3;
								if (blueprint2 != null)
								{
									ThingDef thingDef2 = blueprint2.def.entityDefToBuild as ThingDef;
									if (thingDef2 == null)
									{
										goto IL_37E;
									}
									thingDef3 = thingDef2;
								}
								else
								{
									thingDef3 = thing2.def;
								}
								if (thingDef3.hasInteractionCell && cellRect.Contains(ThingUtility.InteractionCellWhenAt(thingDef3, thing2.Position, thing2.Rotation, thing2.Map)))
								{
									return new AcceptanceReport("WouldBlockInteractionSpot".Translate(new object[]
									{
										entDef.label,
										thingDef3.label
									}).CapitalizeFirst());
								}
							}
							IL_37E:;
						}
					}
				}
			}
			TerrainDef terrainDef = entDef as TerrainDef;
			if (terrainDef != null)
			{
				if (map.terrainGrid.TerrainAt(center) == terrainDef)
				{
					return new AcceptanceReport("TerrainIsAlready".Translate(new object[]
					{
						terrainDef.label
					}));
				}
				if (map.designationManager.DesignationAt(center, DesignationDefOf.SmoothFloor) != null)
				{
					return new AcceptanceReport("SpaceBeingSmoothed".Translate());
				}
			}
			if (!GenConstruct.CanBuildOnTerrain(entDef, center, map, rot, thingToIgnore))
			{
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			}
			if (!godMode)
			{
				CellRect.CellRectIterator iterator2 = cellRect.GetIterator();
				while (!iterator2.Done())
				{
					thingList = iterator2.Current.GetThingList(map);
					for (int l = 0; l < thingList.Count; l++)
					{
						Thing thing3 = thingList[l];
						if (thing3 != thingToIgnore)
						{
							if (!GenConstruct.CanPlaceBlueprintOver(entDef, thing3.def))
							{
								return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
							}
						}
					}
					iterator2.MoveNext();
				}
			}
			if (entDef.PlaceWorkers != null)
			{
				for (int m = 0; m < entDef.PlaceWorkers.Count; m++)
				{
					AcceptanceReport result = entDef.PlaceWorkers[m].AllowsPlacing(entDef, center, rot, map, thingToIgnore);
					if (!result.Accepted)
					{
						return result;
					}
				}
			}
			return AcceptanceReport.WasAccepted;
		}

		public static BuildableDef BuiltDefOf(ThingDef def)
		{
			return (def.entityDefToBuild == null) ? def : def.entityDefToBuild;
		}

		public static bool CanPlaceBlueprintOver(BuildableDef newDef, ThingDef oldDef)
		{
			if (oldDef.EverHaulable)
			{
				return true;
			}
			TerrainDef terrainDef = newDef as TerrainDef;
			if (terrainDef != null)
			{
				if (oldDef.category == ThingCategory.Building && !terrainDef.affordances.Contains(oldDef.terrainAffordanceNeeded))
				{
					return false;
				}
				if ((oldDef.IsBlueprint || oldDef.IsFrame) && !terrainDef.affordances.Contains(oldDef.entityDefToBuild.terrainAffordanceNeeded))
				{
					return false;
				}
			}
			ThingDef thingDef = newDef as ThingDef;
			BuildableDef buildableDef = GenConstruct.BuiltDefOf(oldDef);
			ThingDef thingDef2 = buildableDef as ThingDef;
			if (oldDef == ThingDefOf.SteamGeyser && !newDef.ForceAllowPlaceOver(oldDef))
			{
				return false;
			}
			if (oldDef.category == ThingCategory.Plant && oldDef.passability == Traversability.Impassable && thingDef != null && thingDef.category == ThingCategory.Building && !thingDef.building.canPlaceOverImpassablePlant)
			{
				return false;
			}
			if (oldDef.category == ThingCategory.Building || oldDef.IsBlueprint || oldDef.IsFrame)
			{
				if (thingDef != null)
				{
					if (!thingDef.IsEdifice())
					{
						return (oldDef.building == null || oldDef.building.canBuildNonEdificesUnder) && (!thingDef.EverTransmitsPower || !oldDef.EverTransmitsPower);
					}
					if (thingDef.IsEdifice() && oldDef != null && oldDef.category == ThingCategory.Building && !oldDef.IsEdifice())
					{
						return thingDef.building == null || thingDef.building.canBuildNonEdificesUnder;
					}
					if (thingDef2 != null && thingDef2 == ThingDefOf.Wall && thingDef.building != null && thingDef.building.canPlaceOverWall)
					{
						return true;
					}
					if (newDef != ThingDefOf.PowerConduit && buildableDef == ThingDefOf.PowerConduit)
					{
						return true;
					}
				}
				return (newDef is TerrainDef && buildableDef is ThingDef && ((ThingDef)buildableDef).CoexistsWithFloors) || (buildableDef is TerrainDef && !(newDef is TerrainDef));
			}
			return true;
		}

		public static Thing FirstBlockingThing(Thing constructible, Pawn pawnToIgnore)
		{
			Blueprint blueprint = constructible as Blueprint;
			Thing thing;
			if (blueprint != null)
			{
				thing = GenConstruct.MiniToInstallOrBuildingToReinstall(blueprint);
			}
			else
			{
				thing = null;
			}
			CellRect.CellRectIterator iterator = constructible.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(constructible.Map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing2 = thingList[i];
					if (GenConstruct.BlocksConstruction(constructible, thing2) && thing2 != pawnToIgnore && thing2 != thing)
					{
						return thing2;
					}
				}
				iterator.MoveNext();
			}
			return null;
		}

		public static Job HandleBlockingThingJob(Thing constructible, Pawn worker, bool forced = false)
		{
			Thing thing = GenConstruct.FirstBlockingThing(constructible, worker);
			if (thing == null)
			{
				return null;
			}
			if (thing.def.category == ThingCategory.Plant)
			{
				LocalTargetInfo target = thing;
				PathEndMode peMode = PathEndMode.ClosestTouch;
				Danger maxDanger = worker.NormalMaxDanger();
				if (worker.CanReserveAndReach(target, peMode, maxDanger, 1, -1, null, forced))
				{
					return new Job(JobDefOf.CutPlant, thing);
				}
			}
			else if (thing.def.category == ThingCategory.Item)
			{
				if (thing.def.EverHaulable)
				{
					return HaulAIUtility.HaulAsideJobFor(worker, thing);
				}
				Log.ErrorOnce(string.Concat(new object[]
				{
					"Never haulable ",
					thing,
					" blocking ",
					constructible.ToStringSafe<Thing>(),
					" at ",
					constructible.Position
				}), 6429262);
			}
			else if (thing.def.category == ThingCategory.Building)
			{
				LocalTargetInfo target = thing;
				PathEndMode peMode = PathEndMode.Touch;
				Danger maxDanger = worker.NormalMaxDanger();
				if (worker.CanReserveAndReach(target, peMode, maxDanger, 1, -1, null, forced))
				{
					return new Job(JobDefOf.Deconstruct, thing)
					{
						ignoreDesignations = true
					};
				}
			}
			return null;
		}

		public static bool BlocksConstruction(Thing constructible, Thing t)
		{
			if (t == constructible)
			{
				return false;
			}
			ThingDef thingDef;
			if (constructible is Blueprint)
			{
				thingDef = constructible.def;
			}
			else if (constructible is Frame)
			{
				thingDef = constructible.def.entityDefToBuild.blueprintDef;
			}
			else
			{
				thingDef = constructible.def.blueprintDef;
			}
			if (t.def.category == ThingCategory.Building && GenSpawn.SpawningWipes(thingDef.entityDefToBuild, t.def))
			{
				return true;
			}
			if (t.def.category == ThingCategory.Plant)
			{
				return t.def.plant.harvestWork >= 200f;
			}
			if (!thingDef.clearBuildingArea)
			{
				return false;
			}
			if (t.def == ThingDefOf.SteamGeyser && thingDef.entityDefToBuild.ForceAllowPlaceOver(t.def))
			{
				return false;
			}
			ThingDef thingDef2 = thingDef.entityDefToBuild as ThingDef;
			if (thingDef2 != null)
			{
				if (thingDef2.EverTransmitsPower && t.def == ThingDefOf.PowerConduit && thingDef2 != ThingDefOf.PowerConduit)
				{
					return false;
				}
				if (t.def == ThingDefOf.Wall && thingDef2.building != null && thingDef2.building.canPlaceOverWall)
				{
					return false;
				}
			}
			return (t.def.IsEdifice() && thingDef2.IsEdifice()) || (t.def.category == ThingCategory.Pawn || (t.def.category == ThingCategory.Item && thingDef.entityDefToBuild.passability == Traversability.Impassable)) || t.def.Fillage >= FillCategory.Partial;
		}

		public static bool TerrainCanSupport(CellRect rect, Map map, ThingDef thing)
		{
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				if (!iterator.Current.SupportsStructureType(map, thing.terrainAffordanceNeeded))
				{
					return false;
				}
				iterator.MoveNext();
			}
			return true;
		}
	}
}
