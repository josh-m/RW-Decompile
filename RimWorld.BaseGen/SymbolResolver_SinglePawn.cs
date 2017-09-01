using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_SinglePawn : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			IntVec3 intVec;
			return base.CanResolve(rp) && ((rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned) || SymbolResolver_SinglePawn.TryFindSpawnCell(rp, out intVec));
		}

		public override void Resolve(ResolveParams rp)
		{
			if (rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned)
			{
				return;
			}
			Map map = BaseGen.globalSettings.map;
			IntVec3 loc;
			if (!SymbolResolver_SinglePawn.TryFindSpawnCell(rp, out loc))
			{
				if (rp.singlePawnToSpawn != null)
				{
					Find.WorldPawns.PassToWorld(rp.singlePawnToSpawn, PawnDiscardDecideMode.Discard);
				}
				return;
			}
			Pawn pawn;
			if (rp.singlePawnToSpawn == null)
			{
				PawnGenerationRequest value;
				if (rp.singlePawnGenerationRequest.HasValue)
				{
					value = rp.singlePawnGenerationRequest.Value;
				}
				else
				{
					PawnKindDef arg_BE_0;
					if ((arg_BE_0 = rp.singlePawnKindDef) == null)
					{
						arg_BE_0 = (from x in DefDatabase<PawnKindDef>.AllDefsListForReading
						where x.defaultFactionType == null || !x.defaultFactionType.isPlayer
						select x).RandomElement<PawnKindDef>();
					}
					PawnKindDef pawnKindDef = arg_BE_0;
					Faction faction = rp.faction;
					if (faction == null && pawnKindDef.RaceProps.Humanlike)
					{
						if (pawnKindDef.defaultFactionType != null)
						{
							faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
							if (faction == null)
							{
								return;
							}
						}
						else if (!(from x in Find.FactionManager.AllFactions
						where !x.IsPlayer
						select x).TryRandomElement(out faction))
						{
							return;
						}
					}
					int tile = map.Tile;
					value = new PawnGenerationRequest(pawnKindDef, faction, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, false, 1f, false, true, true, false, false, null, null, null, null, null, null);
				}
				pawn = PawnGenerator.GeneratePawn(value);
			}
			else
			{
				pawn = rp.singlePawnToSpawn;
			}
			if (!pawn.Dead && rp.disableSinglePawn.HasValue && rp.disableSinglePawn.Value)
			{
				pawn.mindState.Active = false;
			}
			GenSpawn.Spawn(pawn, loc, map);
			if (rp.singlePawnLord != null)
			{
				rp.singlePawnLord.AddPawn(pawn);
			}
		}

		public static bool TryFindSpawnCell(ResolveParams rp, out IntVec3 cell)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
		}
	}
}
