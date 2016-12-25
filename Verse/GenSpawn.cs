using RimWorld;
using System;
using System.Linq;

namespace Verse
{
	public static class GenSpawn
	{
		public static Thing Spawn(ThingDef def, IntVec3 loc)
		{
			return GenSpawn.Spawn(ThingMaker.MakeThing(def, null), loc);
		}

		public static Thing Spawn(Thing newThing, IntVec3 loc)
		{
			return GenSpawn.Spawn(newThing, loc, Rot4.North);
		}

		public static Thing Spawn(Thing newThing, IntVec3 loc, Rot4 rot)
		{
			if (!loc.InBounds())
			{
				Log.Error(string.Concat(new object[]
				{
					"Tried to spawn ",
					newThing,
					" out of bounds at ",
					loc,
					"."
				}));
				return null;
			}
			GenSpawn.WipeExistingThings(loc, rot, newThing.def, false);
			if (newThing.def.randomizeRotationOnSpawn)
			{
				newThing.Rotation = Rot4.Random;
			}
			else
			{
				newThing.Rotation = rot;
			}
			newThing.SetPositionDirect(IntVec3.Invalid);
			newThing.Position = loc;
			newThing.SpawnSetup();
			if (newThing.stackCount == 0)
			{
				Log.Error("Spawned thing with 0 stackCount: " + newThing);
				newThing.Destroy(DestroyMode.Vanish);
				return null;
			}
			return newThing;
		}

		public static void WipeExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, bool reclaimResources)
		{
			GenSpawn.WipeExistingThings(thingPos, thingRot, thingDef, reclaimResources, null);
		}

		public static void WipeExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, bool reclaimResources, CellRect? leavingsRect)
		{
			foreach (IntVec3 current in GenAdj.CellsOccupiedBy(thingPos, thingRot, thingDef.Size))
			{
				foreach (Thing current2 in Find.ThingGrid.ThingsAt(current).ToList<Thing>())
				{
					if (GenSpawn.SpawningWipes(thingDef, current2.def))
					{
						if (reclaimResources && current2 is Building)
						{
							CellRect leavingsRect2 = current2.OccupiedRect();
							if (leavingsRect.HasValue)
							{
								leavingsRect2 = leavingsRect.Value;
							}
							GenLeaving.DoLeavingsFor(current2, DestroyMode.Deconstruct, leavingsRect2);
						}
						DestroyMode mode = (!reclaimResources) ? DestroyMode.Vanish : DestroyMode.Cancel;
						current2.Destroy(mode);
					}
				}
			}
		}

		public static bool SpawningWipes(BuildableDef newEntDef, BuildableDef oldEntDef)
		{
			ThingDef thingDef = newEntDef as ThingDef;
			ThingDef thingDef2 = oldEntDef as ThingDef;
			if (thingDef == null || thingDef2 == null)
			{
				return false;
			}
			if (thingDef.category == ThingCategory.Attachment || thingDef.category == ThingCategory.Mote || thingDef.category == ThingCategory.Filth || thingDef.category == ThingCategory.Projectile)
			{
				return false;
			}
			if (!thingDef2.destroyable)
			{
				return false;
			}
			if (thingDef.category == ThingCategory.Plant)
			{
				return false;
			}
			if (thingDef2.category == ThingCategory.Filth && thingDef.passability != Traversability.Standable)
			{
				return true;
			}
			if (thingDef.EverTransmitsPower && thingDef2 == ThingDefOf.PowerConduit)
			{
				return true;
			}
			if (thingDef.IsFrame && GenSpawn.SpawningWipes(thingDef.entityDefToBuild, oldEntDef))
			{
				return true;
			}
			BuildableDef buildableDef = GenConstruct.BuiltDefOf(thingDef);
			BuildableDef buildableDef2 = GenConstruct.BuiltDefOf(thingDef2);
			if (buildableDef == null || buildableDef2 == null)
			{
				return false;
			}
			ThingDef thingDef3 = thingDef.entityDefToBuild as ThingDef;
			if (thingDef2.IsBlueprint)
			{
				if (thingDef.IsBlueprint)
				{
					if (thingDef3 != null && thingDef3.building != null && thingDef3.building.canPlaceOverWall && thingDef2.entityDefToBuild is ThingDef && (ThingDef)thingDef2.entityDefToBuild == ThingDefOf.Wall)
					{
						return true;
					}
					if (thingDef2.entityDefToBuild is TerrainDef)
					{
						if (thingDef.entityDefToBuild is ThingDef && ((ThingDef)thingDef.entityDefToBuild).coversFloor)
						{
							return true;
						}
						if (thingDef.entityDefToBuild is TerrainDef)
						{
							return true;
						}
					}
				}
				return thingDef2.entityDefToBuild == ThingDefOf.PowerConduit && thingDef.entityDefToBuild is ThingDef && (thingDef.entityDefToBuild as ThingDef).EverTransmitsPower;
			}
			if ((thingDef2.IsFrame || thingDef2.IsBlueprint) && thingDef2.entityDefToBuild is TerrainDef)
			{
				ThingDef thingDef4 = buildableDef as ThingDef;
				if (thingDef4 != null && !thingDef4.CoexistsWithFloors)
				{
					return true;
				}
			}
			if (thingDef2 == ThingDefOf.DropPod)
			{
				return false;
			}
			if (thingDef == ThingDefOf.DropPod)
			{
				return thingDef2 != ThingDefOf.DropPod && (thingDef2.category == ThingCategory.Building && thingDef2.passability == Traversability.Impassable);
			}
			if (thingDef.IsEdifice())
			{
				if (thingDef.BlockPlanting && thingDef2.category == ThingCategory.Plant)
				{
					return true;
				}
				if (!(buildableDef is TerrainDef) && buildableDef2.IsEdifice())
				{
					return true;
				}
			}
			return false;
		}
	}
}
