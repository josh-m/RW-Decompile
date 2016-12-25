using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_FarmAnimalsWanderIn : IncidentWorker
	{
		private const float MaxWildness = 0.35f;

		private const float TotalBodySizeToSpawn = 2.5f;

		public override bool TryExecute(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map))
			{
				return false;
			}
			PawnKindDef pawnKindDef;
			if (!(from x in DefDatabase<PawnKindDef>.AllDefs
			where x.RaceProps.Animal && x.RaceProps.wildness < 0.35f && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race)
			select x).TryRandomElementByWeight((PawnKindDef k) => 0.420000017f - k.RaceProps.wildness, out pawnKindDef))
			{
				return false;
			}
			int num = Mathf.Clamp(GenMath.RoundRandom(2.5f / pawnKindDef.RaceProps.baseBodySize), 2, 10);
			for (int i = 0; i < num; i++)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 12);
				Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, null);
				GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
				pawn.SetFaction(Faction.OfPlayer, null);
			}
			Find.LetterStack.ReceiveLetter("LetterLabelFarmAnimalsWanderIn".Translate(new object[]
			{
				pawnKindDef.label
			}).CapitalizeFirst(), "LetterFarmAnimalsWanderIn".Translate(new object[]
			{
				pawnKindDef.label
			}), LetterType.Good, new TargetInfo(intVec, map, false), null);
			return true;
		}
	}
}
