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
			return base.CanResolve(rp) && this.TryFindSpawnCell(rp, out intVec);
		}

		public override void Resolve(ResolveParams rp)
		{
			IntVec3 loc;
			if (!this.TryFindSpawnCell(rp, out loc))
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
				PawnKindDef arg_73_0;
				if ((arg_73_0 = rp.singlePawnKindDef) == null)
				{
					arg_73_0 = (from x in DefDatabase<PawnKindDef>.AllDefsListForReading
					where x.defaultFactionType == null || !x.defaultFactionType.isPlayer
					select x).RandomElement<PawnKindDef>();
				}
				PawnKindDef pawnKindDef = arg_73_0;
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
				pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
			}
			else
			{
				pawn = rp.singlePawnToSpawn;
			}
			if (!pawn.Dead && rp.disableSinglePawn.HasValue && rp.disableSinglePawn.Value)
			{
				pawn.mindState.Active = false;
			}
			GenSpawn.Spawn(pawn, loc, BaseGen.globalSettings.map);
			if (rp.singlePawnLord != null)
			{
				rp.singlePawnLord.AddPawn(pawn);
			}
		}

		private bool TryFindSpawnCell(ResolveParams rp, out IntVec3 cell)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
		}
	}
}
