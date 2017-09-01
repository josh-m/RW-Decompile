using System;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_SingleThing : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (rp.singleThingToSpawn != null && rp.singleThingToSpawn.Spawned)
			{
				return true;
			}
			IntVec3 intVec;
			if (rp.singleThingToSpawn is Pawn)
			{
				ResolveParams rp2 = rp;
				rp2.singlePawnToSpawn = (Pawn)rp.singleThingToSpawn;
				if (!SymbolResolver_SinglePawn.TryFindSpawnCell(rp2, out intVec))
				{
					return false;
				}
			}
			return ((rp.singleThingDef == null || rp.singleThingDef.category != ThingCategory.Item) && (rp.singleThingToSpawn == null || rp.singleThingToSpawn.def.category != ThingCategory.Item)) || this.TryFindSpawnCellForItem(rp.rect, out intVec);
		}

		public override void Resolve(ResolveParams rp)
		{
			if (rp.singleThingToSpawn is Pawn)
			{
				ResolveParams resolveParams = rp;
				resolveParams.singlePawnToSpawn = (Pawn)rp.singleThingToSpawn;
				BaseGen.symbolStack.Push("pawn", resolveParams);
				return;
			}
			if (rp.singleThingToSpawn != null && rp.singleThingToSpawn.Spawned)
			{
				return;
			}
			ThingDef thingDef;
			if (rp.singleThingToSpawn == null)
			{
				ThingDef arg_9B_0;
				if ((arg_9B_0 = rp.singleThingDef) == null)
				{
					arg_9B_0 = (from x in ItemCollectionGeneratorUtility.allGeneratableItems
					where x.IsWeapon || x.IsMedicine || x.IsDrug
					select x).RandomElement<ThingDef>();
				}
				thingDef = arg_9B_0;
			}
			else
			{
				thingDef = rp.singleThingToSpawn.def;
			}
			Rot4? thingRot = rp.thingRot;
			Rot4 rot = (!thingRot.HasValue) ? Rot4.North : thingRot.Value;
			if (!thingDef.rotatable)
			{
				rot = Rot4.North;
			}
			IntVec3 loc;
			if (thingDef.category == ThingCategory.Item)
			{
				if (!this.TryFindSpawnCellForItem(rp.rect, out loc))
				{
					if (rp.singleThingToSpawn != null)
					{
						rp.singleThingToSpawn.Destroy(DestroyMode.Vanish);
					}
					return;
				}
			}
			else
			{
				loc = this.FindBestSpawnCellForNonItem(rp.rect, thingDef, rot);
			}
			Thing thing;
			if (rp.singleThingToSpawn == null)
			{
				ThingDef stuff = rp.singleThingStuff ?? GenStuff.DefaultStuffFor(thingDef);
				thing = ThingMaker.MakeThing(thingDef, stuff);
				Thing arg_185_0 = thing;
				int? singleThingStackCount = rp.singleThingStackCount;
				arg_185_0.stackCount = ((!singleThingStackCount.HasValue) ? 1 : singleThingStackCount.Value);
				if (thing.stackCount <= 0)
				{
					thing.stackCount = 1;
				}
				if (thing.def.CanHaveFaction && thing.Faction != rp.faction)
				{
					thing.SetFaction(rp.faction, null);
				}
				CompQuality compQuality = thing.TryGetComp<CompQuality>();
				if (compQuality != null)
				{
					compQuality.SetQuality(QualityUtility.RandomBaseGenItemQuality(), ArtGenerationContext.Outsider);
				}
			}
			else
			{
				thing = rp.singleThingToSpawn;
			}
			thing = GenSpawn.Spawn(thing, loc, BaseGen.globalSettings.map, rot, false);
			if (thing != null && thing.def.category == ThingCategory.Item)
			{
				thing.SetForbidden(true, false);
			}
		}

		private bool TryFindSpawnCellForItem(CellRect rect, out IntVec3 result)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 c)
			{
				if (c.GetFirstItem(map) != null)
				{
					return false;
				}
				if (!c.Standable(map))
				{
					SurfaceType surfaceType = c.GetSurfaceType(map);
					if (surfaceType != SurfaceType.Item && surfaceType != SurfaceType.Eat)
					{
						return false;
					}
				}
				return true;
			}, out result);
		}

		private IntVec3 FindBestSpawnCellForNonItem(CellRect rect, ThingDef thingDef, Rot4 rot)
		{
			Map map = BaseGen.globalSettings.map;
			if (thingDef.category == ThingCategory.Building)
			{
				foreach (IntVec3 current in rect.Cells.InRandomOrder(null))
				{
					CellRect rect2 = GenAdj.OccupiedRect(current, rot, thingDef.size);
					if (rect2.FullyContainedWithin(rect) && !BaseGenUtility.AnyDoorCardinalAdjacentTo(rect2, map) && !this.AnyNonStandableCellOrAnyBuildingInside(rect2) && GenConstruct.TerrainCanSupport(rect2, map, thingDef))
					{
						IntVec3 result = current;
						return result;
					}
				}
				foreach (IntVec3 current2 in rect.Cells.InRandomOrder(null))
				{
					CellRect rect3 = GenAdj.OccupiedRect(current2, rot, thingDef.size);
					if (rect3.FullyContainedWithin(rect) && !BaseGenUtility.AnyDoorCardinalAdjacentTo(rect3, map) && !this.AnyNonStandableCellOrAnyBuildingInside(rect3))
					{
						IntVec3 result = current2;
						return result;
					}
				}
			}
			foreach (IntVec3 current3 in rect.Cells.InRandomOrder(null))
			{
				CellRect rect4 = GenAdj.OccupiedRect(current3, rot, thingDef.size);
				if (rect4.FullyContainedWithin(rect) && !this.AnyNonStandableCellOrAnyBuildingInside(rect4))
				{
					IntVec3 result = current3;
					return result;
				}
			}
			foreach (IntVec3 current4 in rect.Cells.InRandomOrder(null))
			{
				if (GenAdj.OccupiedRect(current4, rot, thingDef.size).FullyContainedWithin(rect))
				{
					IntVec3 result = current4;
					return result;
				}
			}
			IntVec3 centerCell = rect.CenterCell;
			CellRect cellRect = GenAdj.OccupiedRect(centerCell, rot, thingDef.size);
			if (cellRect.minX < 0)
			{
				centerCell.x += -cellRect.minX;
			}
			if (cellRect.minZ < 0)
			{
				centerCell.z += -cellRect.minZ;
			}
			if (cellRect.maxX >= map.Size.x)
			{
				centerCell.x -= cellRect.maxX - map.Size.x + 1;
			}
			if (cellRect.maxZ >= map.Size.z)
			{
				centerCell.z -= cellRect.maxZ - map.Size.z + 1;
			}
			return centerCell;
		}

		private bool AnyNonStandableCellOrAnyBuildingInside(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				if (!iterator.Current.Standable(map))
				{
					return true;
				}
				if (iterator.Current.GetEdifice(map) != null)
				{
					return true;
				}
				iterator.MoveNext();
			}
			return false;
		}
	}
}
