using System;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_MannedMortar : SymbolResolver
	{
		private const float MaxShellDefMarketValue = 250f;

		public override bool CanResolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			if (!base.CanResolve(rp))
			{
				return false;
			}
			int num = 0;
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				if (iterator.Current.Standable(map))
				{
					num++;
				}
				iterator.MoveNext();
			}
			return num >= 2;
		}

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction arg_3C_0;
			if ((arg_3C_0 = rp.faction) == null)
			{
				arg_3C_0 = (Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Industrial) ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined));
			}
			Faction faction = arg_3C_0;
			Rot4? thingRot = rp.thingRot;
			Rot4 rot = (!thingRot.HasValue) ? Rot4.Random : thingRot.Value;
			ThingDef arg_9D_0;
			if ((arg_9D_0 = rp.mortarDef) == null)
			{
				arg_9D_0 = (from x in DefDatabase<ThingDef>.AllDefsListForReading
				where x.category == ThingCategory.Building && x.building.IsMortar
				select x).RandomElement<ThingDef>();
			}
			ThingDef thingDef = arg_9D_0;
			IntVec3 intVec;
			if (!this.TryFindMortarSpawnCell(rp.rect, rot, thingDef, out intVec))
			{
				return;
			}
			if (thingDef.HasComp(typeof(CompMannable)))
			{
				IntVec3 c = ThingUtility.InteractionCellWhenAt(thingDef, intVec, rot, map);
				Lord singlePawnLord = LordMaker.MakeNewLord(faction, new LordJob_ManTurrets(), map, null);
				PawnKindDef kind = faction.RandomPawnKind();
				Faction faction2 = faction;
				int tile = map.Tile;
				PawnGenerationRequest value = new PawnGenerationRequest(kind, faction2, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, true, 1f, false, true, true, true, false, false, false, null, null, null, null, null, null, null, null);
				ResolveParams resolveParams = rp;
				resolveParams.faction = faction;
				resolveParams.singlePawnGenerationRequest = new PawnGenerationRequest?(value);
				resolveParams.rect = CellRect.SingleCell(c);
				resolveParams.singlePawnLord = singlePawnLord;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
			ThingDef turret = thingDef;
			bool allowEMP = false;
			TechLevel techLevel = faction.def.techLevel;
			ThingDef thingDef2 = TurretGunUtility.TryFindRandomShellDef(turret, allowEMP, true, techLevel, false, 250f);
			if (thingDef2 != null)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.faction = faction;
				resolveParams2.singleThingDef = thingDef2;
				resolveParams2.singleThingStackCount = new int?(Rand.RangeInclusive(5, Mathf.Min(8, thingDef2.stackLimit)));
				BaseGen.symbolStack.Push("thing", resolveParams2);
			}
			ResolveParams resolveParams3 = rp;
			resolveParams3.faction = faction;
			resolveParams3.singleThingDef = thingDef;
			resolveParams3.rect = CellRect.SingleCell(intVec);
			resolveParams3.thingRot = new Rot4?(rot);
			BaseGen.symbolStack.Push("thing", resolveParams3);
		}

		private bool TryFindMortarSpawnCell(CellRect rect, Rot4 rot, ThingDef mortarDef, out IntVec3 cell)
		{
			Map map = BaseGen.globalSettings.map;
			Predicate<CellRect> edgeTouchCheck;
			if (rot == Rot4.North)
			{
				edgeTouchCheck = ((CellRect x) => x.Cells.Any((IntVec3 y) => y.z == rect.maxZ));
			}
			else if (rot == Rot4.South)
			{
				edgeTouchCheck = ((CellRect x) => x.Cells.Any((IntVec3 y) => y.z == rect.minZ));
			}
			else if (rot == Rot4.West)
			{
				edgeTouchCheck = ((CellRect x) => x.Cells.Any((IntVec3 y) => y.x == rect.minX));
			}
			else
			{
				edgeTouchCheck = ((CellRect x) => x.Cells.Any((IntVec3 y) => y.x == rect.maxX));
			}
			return CellFinder.TryFindRandomCellInsideWith(rect, delegate(IntVec3 x)
			{
				CellRect obj = GenAdj.OccupiedRect(x, rot, mortarDef.size);
				return ThingUtility.InteractionCellWhenAt(mortarDef, x, rot, map).Standable(map) && obj.FullyContainedWithin(rect) && edgeTouchCheck(obj);
			}, out cell);
		}
	}
}
