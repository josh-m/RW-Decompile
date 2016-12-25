using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FactionBase : SymbolResolver
	{
		public const int MinDistBetweenSandbagsAndFactionBase = 4;

		private const int MaxRoomCells = 100;

		private const int MinTotalRoomsNonWallCellsCount = 62;

		private const float ChanceToSkipSandbag = 0.25f;

		private static readonly IntRange CampfiresCount = new IntRange(1, 1);

		private static readonly IntRange FirefoamPoppersCount = new IntRange(1, 3);

		private static readonly IntRange RoomDivisionsCount = new IntRange(6, 8);

		private static readonly IntRange FinalRoomsCount = new IntRange(3, 5);

		private static readonly FloatRange NeolithicPawnsPoints = new FloatRange(825f, 1320f);

		private static readonly FloatRange NonNeolithicPawnsPoints = new FloatRange(1320f, 1980f);

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false);
			CellRect cellRect = rp.rect;
			bool flag = FactionBaseSymbolResolverUtility.ShouldUseSandbags(faction);
			if (flag)
			{
				cellRect = cellRect.ContractedBy(4);
			}
			List<RoomOutline> roomOutlines = RoomOutlinesGenerator.GenerateRoomOutlines(cellRect, map, SymbolResolver_FactionBase.RoomDivisionsCount.RandomInRange, SymbolResolver_FactionBase.FinalRoomsCount.RandomInRange, 100, 62);
			this.AddRoomCentersToRootsToUnfog(roomOutlines);
			CellRect rect = rp.rect;
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rect;
			resolveParams.faction = faction;
			resolveParams.singlePawnLord = singlePawnLord;
			resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.FactionBase);
			resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => this.CanReachAnyRoom(x, roomOutlines)));
			if (resolveParams.pawnGroupMakerParams == null)
			{
				float points = (!faction.def.techLevel.IsNeolithicOrWorse()) ? SymbolResolver_FactionBase.NonNeolithicPawnsPoints.RandomInRange : SymbolResolver_FactionBase.NeolithicPawnsPoints.RandomInRange;
				resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams.pawnGroupMakerParams.map = map;
				resolveParams.pawnGroupMakerParams.faction = faction;
				resolveParams.pawnGroupMakerParams.points = points;
			}
			BaseGen.symbolStack.Push("pawnGroup", resolveParams);
			if (!faction.def.techLevel.IsNeolithicOrWorse() && cellRect.Area != 0)
			{
				int randomInRange = SymbolResolver_FactionBase.FirefoamPoppersCount.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					ResolveParams resolveParams2 = rp;
					resolveParams2.rect = cellRect;
					resolveParams2.faction = faction;
					BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
				}
			}
			if (map.mapTemperature.OutdoorTemp < 0f && cellRect.Area != 0)
			{
				int randomInRange2 = SymbolResolver_FactionBase.CampfiresCount.RandomInRange;
				for (int j = 0; j < randomInRange2; j++)
				{
					ResolveParams resolveParams3 = rp;
					resolveParams3.rect = cellRect;
					resolveParams3.faction = faction;
					BaseGen.symbolStack.Push("outdoorsCampfire", resolveParams3);
				}
			}
			RoomOutline roomOutline = roomOutlines.MinBy((RoomOutline x) => x.CellsCountIgnoringWalls);
			for (int k = 0; k < roomOutlines.Count; k++)
			{
				RoomOutline roomOutline2 = roomOutlines[k];
				if (roomOutline2 == roomOutline)
				{
					ResolveParams resolveParams4 = rp;
					resolveParams4.rect = roomOutline2.rect.ContractedBy(1);
					resolveParams4.faction = faction;
					BaseGen.symbolStack.Push("storage", resolveParams4);
				}
				else
				{
					ResolveParams resolveParams5 = rp;
					resolveParams5.rect = roomOutline2.rect.ContractedBy(1);
					resolveParams5.faction = faction;
					BaseGen.symbolStack.Push("barracks", resolveParams5);
				}
				ResolveParams resolveParams6 = rp;
				resolveParams6.rect = roomOutline2.rect;
				resolveParams6.faction = faction;
				BaseGen.symbolStack.Push("doors", resolveParams6);
			}
			for (int l = 0; l < roomOutlines.Count; l++)
			{
				RoomOutline roomOutline3 = roomOutlines[l];
				ResolveParams resolveParams7 = rp;
				resolveParams7.rect = roomOutline3.rect;
				resolveParams7.faction = faction;
				BaseGen.symbolStack.Push("emptyRoom", resolveParams7);
			}
			if (flag)
			{
				ResolveParams resolveParams8 = rp;
				resolveParams8.rect = rp.rect;
				resolveParams8.faction = faction;
				float? chanceToSkipSandbag = rp.chanceToSkipSandbag;
				resolveParams8.chanceToSkipSandbag = new float?((!chanceToSkipSandbag.HasValue) ? 0.25f : chanceToSkipSandbag.Value);
				BaseGen.symbolStack.Push("edgeSandbags", resolveParams8);
			}
		}

		private bool CanReachAnyRoom(IntVec3 root, List<RoomOutline> allRooms)
		{
			Map map = BaseGen.globalSettings.map;
			for (int i = 0; i < allRooms.Count; i++)
			{
				if (map.reachability.CanReach(root, allRooms[i].rect.RandomCell, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
				{
					return true;
				}
			}
			return false;
		}

		private void AddRoomCentersToRootsToUnfog(List<RoomOutline> allRooms)
		{
			if (Current.ProgramState != ProgramState.MapInitializing)
			{
				return;
			}
			List<IntVec3> rootsToUnfog = MapGenerator.rootsToUnfog;
			for (int i = 0; i < allRooms.Count; i++)
			{
				rootsToUnfog.Add(allRooms[i].rect.CenterCell);
			}
		}
	}
}
