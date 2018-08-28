using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_PawnGroup : SymbolResolver
	{
		private const float DefaultPoints = 250f;

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			return (from x in rp.rect.Cells
			where x.Standable(BaseGen.globalSettings.map)
			select x).Any<IntVec3>();
		}

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			PawnGroupMakerParms pawnGroupMakerParms = rp.pawnGroupMakerParams;
			if (pawnGroupMakerParms == null)
			{
				pawnGroupMakerParms = new PawnGroupMakerParms();
				pawnGroupMakerParms.tile = map.Tile;
				pawnGroupMakerParms.faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
				pawnGroupMakerParms.points = 250f;
			}
			pawnGroupMakerParms.groupKind = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Combat);
			List<PawnKindDef> list = new List<PawnKindDef>();
			foreach (Pawn current in PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true))
			{
				list.Add(current.kindDef);
				ResolveParams resolveParams = rp;
				resolveParams.singlePawnToSpawn = current;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
		}
	}
}
