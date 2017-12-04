using System;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Ship_Spine : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			ThingDef ship_Beam = ThingDefOf.Ship_Beam;
			Map map = BaseGen.globalSettings.map;
			if (!rp.thingRot.HasValue && !rp.thrustAxis.HasValue)
			{
				rp.thrustAxis = new Rot4?(Rot4.Random);
				rp.thingRot = rp.thrustAxis;
			}
			IntVec2 invalid = IntVec2.Invalid;
			IntVec2 invalid2 = IntVec2.Invalid;
			IntVec2 toIntVec = rp.thingRot.Value.FacingCell.ToIntVec2;
			int num = 0;
			while (true)
			{
				if (rp.thingRot.Value.IsHorizontal)
				{
					int newZ = Rand.Range(rp.rect.minZ + 1, rp.rect.maxZ - 2);
					invalid = new IntVec2((!(rp.thingRot.Value == Rot4.East)) ? rp.rect.maxX : rp.rect.minX, newZ);
					invalid2 = new IntVec2((!(rp.thingRot.Value == Rot4.East)) ? rp.rect.minX : rp.rect.maxX, newZ);
				}
				else
				{
					int newX = Rand.Range(rp.rect.minX + 1, rp.rect.maxX - 2);
					invalid = new IntVec2(newX, (!(rp.thingRot.Value == Rot4.North)) ? rp.rect.maxZ : rp.rect.minZ);
					invalid2 = new IntVec2(newX, (!(rp.thingRot.Value == Rot4.North)) ? rp.rect.minZ : rp.rect.maxZ);
				}
				bool? allowPlacementOffEdge = rp.allowPlacementOffEdge;
				if (!allowPlacementOffEdge.HasValue || allowPlacementOffEdge.Value)
				{
					break;
				}
				if ((invalid - toIntVec).ToIntVec3.GetThingList(map).Any((Thing thing) => thing.def == ThingDefOf.Ship_Beam))
				{
					break;
				}
				if (num == 20)
				{
					return;
				}
				num++;
			}
			int magnitudeManhattan = (invalid - invalid2).MagnitudeManhattan;
			if ((invalid - invalid2).Magnitude < (float)ship_Beam.Size.z)
			{
				return;
			}
			int num3;
			int num4;
			do
			{
				bool? allowPlacementOffEdge2 = rp.allowPlacementOffEdge;
				int arg_2DA_0 = (allowPlacementOffEdge2.HasValue && !allowPlacementOffEdge2.Value) ? 0 : Rand.Range(0, 7);
				int num2 = Rand.Range(0, 7);
				num3 = 0;
				num2 = 0;
				num4 = (magnitudeManhattan - num3 - num2) / ship_Beam.Size.z;
			}
			while (num4 <= 0);
			IntVec2 a = invalid + toIntVec * (num3 + ship_Beam.Size.z / 2 - 1);
			Thing t = null;
			for (int i = 0; i < num4; i++)
			{
				Thing thing2 = ThingMaker.MakeThing(ship_Beam, null);
				thing2.SetFaction(rp.faction, null);
				t = GenSpawn.Spawn(thing2, a.ToIntVec3, map, rp.thingRot.Value, false);
				a += toIntVec * ship_Beam.Size.z;
			}
			bool? allowPlacementOffEdge3 = rp.allowPlacementOffEdge;
			if (!allowPlacementOffEdge3.HasValue || allowPlacementOffEdge3.Value)
			{
				BaseGen.symbolStack.Push("ship_populate", rp);
			}
			CellRect rect;
			Rot4 value;
			CellRect rect2;
			Rot4 value2;
			if (rp.thingRot.Value.IsHorizontal)
			{
				rect = rp.rect;
				rect.minZ = t.OccupiedRect().maxZ + 1;
				value = Rot4.North;
				rect2 = rp.rect;
				rect2.maxZ = t.OccupiedRect().minZ - 1;
				value2 = Rot4.South;
			}
			else
			{
				rect = rp.rect;
				rect.maxX = t.OccupiedRect().minX - 1;
				value = Rot4.West;
				rect2 = rp.rect;
				rect2.minX = t.OccupiedRect().maxX + 1;
				value2 = Rot4.East;
			}
			bool? allowPlacementOffEdge4 = rp.allowPlacementOffEdge;
			if (!allowPlacementOffEdge4.HasValue || allowPlacementOffEdge4.Value || Rand.Value < 0.3f)
			{
				ResolveParams resolveParams = rp;
				resolveParams.rect = rect;
				resolveParams.thingRot = new Rot4?(value);
				resolveParams.allowPlacementOffEdge = new bool?(false);
				BaseGen.symbolStack.Push("ship_spine", resolveParams);
			}
			bool? allowPlacementOffEdge5 = rp.allowPlacementOffEdge;
			if (!allowPlacementOffEdge5.HasValue || allowPlacementOffEdge5.Value || Rand.Value < 0.3f)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.rect = rect2;
				resolveParams2.thingRot = new Rot4?(value2);
				resolveParams2.allowPlacementOffEdge = new bool?(false);
				BaseGen.symbolStack.Push("ship_spine", resolveParams2);
			}
			ResolveParams resolveParams3 = rp;
			resolveParams3.floorDef = TerrainDefOf.Concrete;
			BaseGen.symbolStack.Push("floor", resolveParams3);
		}
	}
}
