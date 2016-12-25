using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ManhunterPackIncidentUtility
	{
		public static bool TryFindRandomAnimalKind(List<PawnKindDef> candidates, Map map, out PawnKindDef animalKind)
		{
			return (from k in candidates
			where map == null || map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(k.race)
			select k).TryRandomElement(out animalKind);
		}

		public static List<Pawn> GenerateAnimals(PawnKindDef animalKind, Map map, float points)
		{
			int num = Mathf.Max(Mathf.RoundToInt(points / animalKind.combatPower), 1);
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < num; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);
				Pawn item = PawnGenerator.GeneratePawn(request);
				list.Add(item);
			}
			return list;
		}
	}
}
